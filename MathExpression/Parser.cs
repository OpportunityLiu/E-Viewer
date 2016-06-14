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

        public string ExprStr => string.Join(" ", Expressions);

        public bool MoveNext()
        {
            var r = tokens.MoveNext();
            Ended = !r;
            if(r)
                Expressions.Push(Current.ToString());
            return r;
        }

        public List<ParameterExpression> Parameters
        {
            get;
        } = new List<ParameterExpression>();

        public Expression Expr
        {
            get; set;
        }
    }

    public static class Parser
    {
        private static Analyzer parseImpl(string expression)
        {
            if(string.IsNullOrEmpty(expression))
                throw new ArgumentNullException(nameof(expression));
            using(var tokens = Tokenizer.Tokenize(expression).GetEnumerator())
            {
                var analyzer = new Analyzer(tokens);
                if(!analyzer.MoveNext())
                    throw new ParseException("No tokens found.");
                analyzer.Expr = Parser.expression(analyzer);
                return analyzer;
            }
        }

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
                    throw new ParseException("Expression ended at an unexcepted position.");
            }
            terms.Add(term(analyzer));
            while(!analyzer.Ended)
            {
                var addOp = analyzer.Current;
                if(!addOp.IsAddOp())
                    break;
                if(!analyzer.MoveNext())
                    throw new ParseException($"Expression ended at an unexcepted position.");
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
                    throw new ParseException($"Expression ended at an unexcepted position.");
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
                    throw new ParseException($"Expression ended at an unexcepted position.");
                powOps.Add(powOp);
                factors.Add(factor(analyzer));
            }
            var result = factors[0];
            for(int i = 0; i < powOps.Count; i++)
            {
                result = Expression.Power(result, factors[i + 1]);
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
                    if(!analyzer.MoveNext())
                        throw new ParseException($"Expression ended at an unexcepted position.");
                    if(analyzer.Current.Type != TokenType.LeftBracket)
                        throw new ParseException($"Unexcepted token has been detected.\nPostion: {analyzer.Current.Position}");
                    if(!analyzer.MoveNext())
                        throw new ParseException($"Expression ended at an unexcepted position.");
                    var exprInFunc = expression(analyzer);
                    if(analyzer.Current.Type != TokenType.RightBracket)
                        throw new ParseException($"Bracket not match.\nPosition: {analyzer.Current.Position}");
                    analyzer.MoveNext();
                    return Expression.Call(func, exprInFunc);
                }
                else if(ConstantValues.TryGetValue(first.Id, out constValue))
                {
                    analyzer.MoveNext();
                    return Expression.Constant(constValue, typeof(double));
                }
                else
                {
                    analyzer.MoveNext();
                    var param = Expression.Parameter(typeof(double), first.Id);
                    analyzer.Parameters.Add(param);
                    return param;
                }
            case TokenType.LeftBracket:
                if(!analyzer.MoveNext())
                    throw new ParseException($"Expression ended at an unexcepted position.");
                var expr = expression(analyzer);
                if(analyzer.Current.Type != TokenType.RightBracket)
                    throw new ParseException($"Bracket not match.\nPosition: {analyzer.Current.Position}");
                analyzer.MoveNext();
                return expr;
            case TokenType.RightBracket:
            case TokenType.Plus:
            case TokenType.Minus:
            case TokenType.Multiply:
            case TokenType.Divide:
            case TokenType.Power:
            default:
                throw new ParseException($"Unexcepted token has been detected.\nPostion: {first.Position}");
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

        public static IReadOnlyDictionary<string, MethodInfo> Functions
        {
            get;
        } = loadFunctions();

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
            Expression = System.Linq.Expressions.Expression.Lambda(analyzer.Expr, Formatted, analyzer.Parameters);
            Compiled = Expression.Compile();
            Parameters = new ReadOnlyCollection<string>(analyzer.Parameters.Select(p => p.Name).ToList());
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
            Expression = System.Linq.Expressions.Expression.Lambda<TDelegate>(analyzer.Expr, analyzer.ExprStr, analyzer.Parameters);
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
        public ParseException()
        {
        }
        public ParseException(string message) : base(message) { }
        public ParseException(string message, Exception inner) : base(message, inner) { }
    }
}
