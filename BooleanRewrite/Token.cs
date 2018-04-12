using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BooleanRewrite
{
    class Token
    {
        static Dictionary<char, KeyValuePair<TokenType, string>> dict = new Dictionary<char, KeyValuePair<TokenType, string>>()
        {
            {
                '(', new KeyValuePair<TokenType, string>(TokenType.OPEN_PAREN, "(")
            },
            {
                ')', new KeyValuePair<TokenType, string>(TokenType.CLOSE_PAREN, ")")
            },
            {
                '!', new KeyValuePair<TokenType, string>(TokenType.NEGATION_OP, "NOT")
            },
            {
                '~', new KeyValuePair<TokenType, string>(TokenType.NEGATION_OP, "NOT")
            },
            {
                '&', new KeyValuePair<TokenType, string>(TokenType.BINARY_OP, "AND")
            },
            {
                '|', new KeyValuePair<TokenType, string>(TokenType.BINARY_OP, "OR")
            }
        };

        public enum TokenType
        {
            OPEN_PAREN,
            CLOSE_PAREN,
            NEGATION_OP,
            BINARY_OP,
            LITERAL,
            EXPR_END
        }

        public TokenType type;
        public string value;

        Token(StringReader s)
        {
            int c = s.Read();
            if (c == -1)
            {
                type = TokenType.EXPR_END;
                value = "";
                return;
            }

            char ch = (char)c;

            if (dict.ContainsKey(ch))
            {
                type = dict[ch].Key;
                value = dict[ch].Value;
            }
            else
            {
                string str = "";
                str += ch;
                while (s.Peek() != -1 && !dict.ContainsKey((char)s.Peek()))
                {
                    str += (char)s.Read();
                }
                type = TokenType.LITERAL;
                value = str;
            }
        }

        public static List<Token> Tokenize(string text)
        {
            List<Token> tokens = new List<Token>();
            StringReader reader = new StringReader(text);

            //Tokenize the expression
            Token t = null;
            do
            {
                t = new Token(reader);
                tokens.Add(t);
            } while (t.type != Token.TokenType.EXPR_END);

            //Use a minimal version of the Shunting Yard algorithm to transform the token list to polish notation
            List<Token> polishNotation = TransformToPolishNotation(tokens);

            return polishNotation;
        }

        static Regex illegalRegex = new Regex(@"^[a-zA-Z_0-9!&|]");
        static void ValidateInput(string text)
        {
            var operators = "!&|";
            if(illegalRegex.IsMatch(text) || operators.Contains(text.LastOrDefault()))
            {
                throw new InvalidInputException();
            }
        }

        static List<Token> TransformToPolishNotation(List<Token> infixTokenList)
        {
            Queue<Token> outputQueue = new Queue<Token>();
            Stack<Token> stack = new Stack<Token>();

            int needNegationOperand = 0;
            int negationOperandInParenthesis = 0;

            int index = 0;
            while (infixTokenList.Count > index)
            {
                Token t = infixTokenList[index];

                switch (t.type)
                {
                    case Token.TokenType.LITERAL:
                        outputQueue.Enqueue(t);
                        if(needNegationOperand > negationOperandInParenthesis)
                        {
                            outputQueue.Enqueue(stack.Pop());
                            needNegationOperand--;
                        }
                        break;
                    case Token.TokenType.BINARY_OP:
                        stack.Push(t);
                        break;
                    case Token.TokenType.OPEN_PAREN:
                        stack.Push(t);
                        if(needNegationOperand > 0)
                            negationOperandInParenthesis++;
                        break;
                    case Token.TokenType.NEGATION_OP:
                        stack.Push(t);
                        needNegationOperand++;
                        break;
                    case Token.TokenType.CLOSE_PAREN:
                        while (stack.Peek().type != Token.TokenType.OPEN_PAREN)
                        {
                            outputQueue.Enqueue(stack.Pop());
                        }
                        stack.Pop();
                        if (stack.Count > 0 && stack.Peek().type == Token.TokenType.NEGATION_OP)
                        {
                            outputQueue.Enqueue(stack.Pop());
                        }
                        if(needNegationOperand>0)
                        {
                            negationOperandInParenthesis--;
                            needNegationOperand--;
                        }
                        break;
                    default:
                        break;
                }

                ++index;
            }
            while (stack.Count > 0)
            {
                outputQueue.Enqueue(stack.Pop());
            }

            return outputQueue.Reverse().ToList();
        }

    }

    [Serializable]
    internal class InvalidInputException : Exception
    {
        public InvalidInputException()
        {
        }

        public InvalidInputException(string message) : base(message)
        {
        }

        public InvalidInputException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidInputException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
