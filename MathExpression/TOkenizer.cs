using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathExpression
{
    public static class Tokenizer
    {
        public static IEnumerable<Token> Tokenize(string experssion)
        {
            var next = 0;
            while(true)
            {
                while(next < experssion.Length && char.IsWhiteSpace(experssion[next]))
                    next++;
                if(next >= experssion.Length)
                    yield break;
                switch(experssion[next])
                {
                case '+':
                    yield return Token.Plus(next++);
                    break;
                case '-':
                    yield return Token.Minus(next++);
                    break;
                case '*':
                    yield return Token.Multiply(next++);
                    break;
                case '/':
                    yield return Token.Divide(next++);
                    break;
                case '(':
                    yield return Token.LeftBracket(next++);
                    break;
                case ')':
                    yield return Token.RightBracket(next++);
                    break;
                case '^':
                    yield return Token.Power(next++);
                    break;
                default:
                    // ID的词法识别分析
                    if(char.IsLetter(experssion[next]))
                    {
                        var startPos = next;
                        var count = 1;
                        next++;
                        while(next < experssion.Length && (char.IsLetter(experssion[next]) || char.IsDigit(experssion[next])))
                        {
                            next++;
                            count++;
                        }
                        yield return new Token(experssion.Substring(startPos, count), startPos);
                    }
                    // NUM的词法识别分析
                    else if(char.IsDigit(experssion[next]) || experssion[next] == '.')
                    {
                        var startPos = next;
                        var count = 1;
                        var hasDot = experssion[next] == '.';
                        next++;
                        while(next < experssion.Length)
                        {
                            if(char.IsDigit(experssion[next]))
                            {
                                next++;
                                count++;
                            }
                            else if(experssion[next] == '.')
                            {
                                if(hasDot)
                                    throw new TokenizeException(experssion, "Multiple '.' were detected.", next);
                                hasDot = true;
                                next++;
                                count++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        var number = experssion.Substring(startPos, count);
                        if(count == 1 && hasDot)
                            yield return new Token(0, startPos);
                        else
                        {
                            double result;
                            try
                            {
                                result = double.Parse(number);
                            }
                            catch(Exception ex)
                            {
                                throw new TokenizeException(experssion, $"Invalid number \"{number}\".", startPos, ex);
                            }
                            yield return new Token(result, startPos);
                        }
                    }
                    else
                    {
                        throw new TokenizeException(experssion, $"Unknown char '{experssion[next]}' was detected.", next);
                    }
                    break;
                }
            }
        }
    }

    public class Token
    {
        private Token(TokenType type, int position)
        {
            this.Type = type;
            this.Position = position;
        }

        public Token(double number, int position)
            : this(TokenType.Number, position)
        {
            this.Number = number;
        }

        public Token(string id, int position)
            : this(TokenType.Id, position)
        {
            this.Id = id;
        }

        public TokenType Type
        {
            get;
        }

        public string Id
        {
            get;
        }

        public double Number
        {
            get;
        }

        public int Position
        {
            get;
        }

        public override string ToString()
        {
            switch(Type)
            {
            case TokenType.Number:
                return Number.ToString();
            case TokenType.Id:
                return Id;
            case TokenType.LeftBracket:
                return "(";
            case TokenType.RightBracket:
                return ")";
            case TokenType.Plus:
                return "+";
            case TokenType.Minus:
                return "-";
            case TokenType.Multiply:
                return "*";
            case TokenType.Divide:
                return "/";
            case TokenType.Power:
                return "^";
            default:
                return base.ToString();
            }
        }

        public static Token Plus(int position) => new Token(TokenType.Plus, position);
        public static Token Minus(int position) => new Token(TokenType.Minus, position);
        public static Token Multiply(int position) => new Token(TokenType.Multiply, position);
        public static Token Divide(int position) => new Token(TokenType.Divide, position);
        public static Token Power(int position) => new Token(TokenType.Power, position);
        public static Token LeftBracket(int position) => new Token(TokenType.LeftBracket, position);
        public static Token RightBracket(int position) => new Token(TokenType.RightBracket, position);
    }

    public enum TokenType
    {
        Number,
        Id,
        LeftBracket,
        RightBracket,
        Plus,
        Minus,
        Multiply,
        Divide,
        Power
    }

    public class TokenizeException : Exception
    {
        private static string getMessage(string info, int position) => $"Tokenize error.\n{info}\nPostion: {position}";

        internal TokenizeException(string expression, string info, int position)
            : base(getMessage(info, position))
        {
            Expression = expression;
        }

        internal TokenizeException(string expression, string info, int position, Exception inner)
            : base(getMessage(info, position), inner)
        {
            Expression = expression;
        }

        public string Expression
        {
            get;
        }
    }
}
