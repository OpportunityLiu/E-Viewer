using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.ObjectModel;

namespace MathExpression
{
    class Analyzer
    {
        public Analyzer(IEnumerator<Token> tokens)
        {
            this.tokens = tokens;
        }

        private readonly IEnumerator<Token> tokens;

        public bool Ended
        {
            get;
            private set;
        }

        public Token Current => tokens.Current;

        public Stack<string> Expressions
        {
            get;
        } = new Stack<string>();

        public string ExprStr => string.Join("", Expressions.Reverse());

        public bool MoveNext()
        {
            var r = tokens.MoveNext();
            Ended = !r;
            if(r)
                Expressions.Push(Current.ToString());
            return r;
        }

        public Dictionary<string, ParameterExpression> Parameters
        {
            get;
        } = new Dictionary<string, ParameterExpression>(StringComparer.OrdinalIgnoreCase);

        public Expression Expr
        {
            get; set;
        }
    }

    public static class Parser
    {
        public static ParseResult Parse(string expression)
        {
            return new ParseResult(parseImpl(expression));
        }

        public static ParseResult<Func<double>> Parse0(string expression)
        {
            return new ParseResult<Func<double>>(parseImpl(expression));
        }

        public static ParseResult<Func<double, double>> Parse1(string expression)
        {
            return new ParseResult<Func<double, double>>(parseImpl(expression));
        }

        public static ParseResult<Func<double, double, double>> Parse2(string expression)
        {
            return new ParseResult<Func<double, double, double>>(parseImpl(expression));
        }

        public static ParseResult<Func<double, double, double, double>> Parse3(string expression)
        {
            return new ParseResult<Func<double, double, double, double>>(parseImpl(expression));
        }

        private static Analyzer parseImpl(string expression)
        {
            if(string.IsNullOrEmpty(expression))
                throw new ArgumentNullException(nameof(expression));
            using(var tokens = Tokenizer.Tokenize(expression).GetEnumerator())
            {
                var analyzer = new Analyzer(tokens);
                if(!analyzer.MoveNext())
                    throw ParseException.EmptyToken();
                analyzer.Expr = Parser.expression(analyzer);
                if(!analyzer.Ended)
                    throw ParseException.UnexpectedToken(analyzer);
                return analyzer;
            }
        }

        // Expression -> [ Addop ] Term { Addop Term }
        // Addop -> "+" | "-"
        private static Expression expression(Analyzer analyzer)
        {
            var terms = new List<Expression>();
            var addOps = new List<Token>();
            addOps.Add(Token.Plus(0));
            if(analyzer.Current.IsAddOp())
            {
                addOps[0] = analyzer.Current;
                if(!analyzer.MoveNext())
                    throw ParseException.WrongEneded();
            }
            terms.Add(term(analyzer));
            while(!analyzer.Ended)
            {
                var addOp = analyzer.Current;
                if(!addOp.IsAddOp())
                    break;
                if(!analyzer.MoveNext())
                    throw ParseException.WrongEneded();
                addOps.Add(addOp);
                terms.Add(term(analyzer));
            }
            var termsWithOperater = terms.Zip(addOps, (term, op) =>
            {
                if(op.Type == TokenType.Plus)
                    return term;
                else
                    return Expression.Negate(term);
            }).ToList();
            var result = termsWithOperater[0];
            for(int i = 1; i < termsWithOperater.Count; i++)
            {
                result = Expression.Add(result, termsWithOperater[i]);
            }
            return result;
        }

        // Term -> Power { Mulop Power }
        // Mulop -> "*" | "/"
        private static Expression term(Analyzer analyzer)
        {
            var powers = new List<Expression>();
            var mulOps = new List<Token>();
            powers.Add(power(analyzer));
            while(!analyzer.Ended)
            {
                var mulOp = analyzer.Current;
                if(!mulOp.IsMulOp())
                    break;
                if(!analyzer.MoveNext())
                    throw ParseException.WrongEneded();
                mulOps.Add(mulOp);
                powers.Add(power(analyzer));
            }
            var result = powers[0];
            for(int i = 0; i < mulOps.Count; i++)
            {
                if(mulOps[i].Type == TokenType.Multiply)
                    result = Expression.Multiply(result, powers[i + 1]);
                else
                    result = Expression.Divide(result, powers[i + 1]);
            }
            return result;
        }

        // Power -> Factor { Powop Factor }
        // Powop -> "^"
        private static Expression power(Analyzer analyzer)
        {
            var factors = new List<Expression>();
            var powOps = new List<Token>();
            factors.Add(factor(analyzer));
            while(!analyzer.Ended)
            {
                var powOp = analyzer.Current;
                if(!powOp.IsPowOp())
                    break;
                if(!analyzer.MoveNext())
                    throw ParseException.WrongEneded();
                powOps.Add(powOp);
                factors.Add(factor(analyzer));
            }
            var result = factors[factors.Count - 1];
            for(int i = powOps.Count - 1; i >= 0; i--)
            {
                result = Expression.Power(factors[i], result);
            }
            return result;
        }

        // Factor -> Function | Id | Number | "(" Expression ")"
        // Function -> Id  "(" Expression ")"
        private static Expression factor(Analyzer analyzer)
        {
            var first = analyzer.Current;
            switch(first.Type)
            {
            case TokenType.Number:
                analyzer.MoveNext();
                return Expression.Constant(first.Number, typeof(double));
            case TokenType.Id:
                MethodInfo func;
                double constValue;
                if(Functions.TryGetValue(first.Id, out func))
                {
                    analyzer.Expressions.Pop();
                    analyzer.Expressions.Push(FunctionNames[first.Id]);
                    if(!analyzer.MoveNext())
                        throw ParseException.WrongEneded();
                    if(analyzer.Current.Type != TokenType.LeftBracket)
                        throw ParseException.UnexpectedToken(analyzer, TokenType.LeftBracket);
                    if(!analyzer.MoveNext())
                        throw ParseException.WrongEneded();
                    var exprInFunc = expression(analyzer);
                    if(analyzer.Current.Type != TokenType.RightBracket)
                        throw ParseException.UnexpectedToken(analyzer, TokenType.RightBracket);
                    analyzer.MoveNext();
                    return Expression.Call(func, exprInFunc);
                }
                else if(ConstantValues.TryGetValue(first.Id, out constValue))
                {
                    analyzer.Expressions.Pop();
                    analyzer.Expressions.Push(ConstantNames[first.Id]);
                    analyzer.MoveNext();
                    return Expression.Constant(constValue, typeof(double));
                }
                else
                {
                    analyzer.MoveNext();
                    ParameterExpression param;
                    if(analyzer.Parameters.TryGetValue(first.Id, out param))
                    {
                        return param;
                    }
                    else
                    {
                        param = Expression.Parameter(typeof(double), first.Id);
                        analyzer.Parameters.Add(first.Id, param);
                        return param;
                    }
                }
            case TokenType.LeftBracket:
                if(!analyzer.MoveNext())
                    throw ParseException.WrongEneded();
                var expr = expression(analyzer);
                if(analyzer.Current.Type != TokenType.RightBracket)
                    throw ParseException.UnexpectedToken(analyzer, TokenType.RightBracket);
                analyzer.MoveNext();
                return expr;
            case TokenType.RightBracket:
            case TokenType.Plus:
            case TokenType.Minus:
            case TokenType.Multiply:
            case TokenType.Divide:
            case TokenType.Power:
            default:
                throw ParseException.UnexpectedToken(analyzer, TokenType.Number | TokenType.Id | TokenType.LeftBracket);
            }
        }

        public static IReadOnlyDictionary<string, double> ConstantValues
        {
            get;
        } = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            ["PI"] = Math.PI,
            ["E"] = Math.E,
        };

        private static Dictionary<string, string> ConstantNames = ConstantValues.Keys.ToDictionary(s => s, StringComparer.OrdinalIgnoreCase);

        public static IReadOnlyDictionary<string, MethodInfo> Functions
        {
            get;
        } = loadFunctions();

        private static Dictionary<string, string> FunctionNames = Functions.Keys.ToDictionary(s => s, StringComparer.OrdinalIgnoreCase);

        private static Dictionary<string, MethodInfo> loadFunctions()
        {
            var r = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase);
            var math = typeof(Math);
            foreach(var item in math.GetMethods())
            {
                if(item.ReturnType == typeof(double) && item.GetParameters().Length == 1)
                    r[item.Name] = item;
            }
            return r;
        }
    }

    static class TokenExtention
    {
        public static bool IsAddOp(this Token that)
        {
            return that.Type == TokenType.Plus || that.Type == TokenType.Minus;
        }

        public static bool IsMulOp(this Token that)
        {
            return that.Type == TokenType.Multiply || that.Type == TokenType.Divide;
        }

        public static bool IsPowOp(this Token that)
        {
            return that.Type == TokenType.Power;
        }

        public static bool IsId(this Token that)
        {
            return that.Type == TokenType.Id;
        }

        public static bool IsNumber(this Token that)
        {
            return that.Type == TokenType.Number;
        }

        public static bool IsLeftBracket(this Token that)
        {
            return that.Type == TokenType.LeftBracket;
        }

        public static bool IsRightBracket(this Token that)
        {
            return that.Type == TokenType.RightBracket;
        }
    }

    public class ParseResult
    {
        internal ParseResult(Analyzer analyzer)
        {
            Formatted = analyzer.ExprStr;
            Expression = System.Linq.Expressions.Expression.Lambda(analyzer.Expr, Formatted, analyzer.Parameters.Values);
            Compiled = Expression.Compile();
            Parameters = new ReadOnlyCollection<string>(analyzer.Parameters.Keys.ToList());
        }

        public LambdaExpression Expression
        {
            get;
        }

        public Delegate Compiled
        {
            get;
        }

        public IReadOnlyList<string> Parameters
        {
            get;
        }

        public string Formatted
        {
            get;
        }
    }

    public class ParseResult<TDelegate> : ParseResult
    {
        internal ParseResult(Analyzer analyzer)
            : base(analyzer)
        {
            Expression = System.Linq.Expressions.Expression.Lambda<TDelegate>(analyzer.Expr, analyzer.ExprStr, analyzer.Parameters.Values);
            Compiled = Expression.Compile();
        }

        public new Expression<TDelegate> Expression
        {
            get;
        }

        public new TDelegate Compiled
        {
            get;
        }
    }

    public class ParseException : Exception
    {
        internal static ParseException UnexpectedToken(Analyzer analyzer, TokenType? expected = null)
        {
            if(expected.HasValue)
                return new ParseException($"Unexpected token has been detected.{expected.Value} expected.\nPostion: {analyzer.Current.Position + 1}");
            else
                return new ParseException($"Unexpected token has been detected.\nPostion: {analyzer.Current.Position + 1}");
        }

        internal static ParseException WrongEneded()
            => new ParseException($"Expression ended at an unexpected position.");

        internal static ParseException EmptyToken()
            => new ParseException($"No tokens found.");

        internal ParseException(string message) : base(message) { }
        internal ParseException(string message, Exception inner) : base(message, inner) { }
    }
}
