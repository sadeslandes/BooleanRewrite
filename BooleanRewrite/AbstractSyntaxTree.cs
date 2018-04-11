using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooleanRewrite
{
    static class AST
    {
        const char _lnot = '\u00ac';
        const char _land = '\u2227';
        const char _lor = '\u2228';

        static public BoolExpr Make(ref List<Token>.Enumerator polishNotationTokensEnumerator)
        {
            if (polishNotationTokensEnumerator.Current.type == Token.TokenType.LITERAL)
            {
                BoolExpr lit = BoolExpr.CreateBoolVar(polishNotationTokensEnumerator.Current.value);
                polishNotationTokensEnumerator.MoveNext();
                return lit;
            }
            else
            {
                if (polishNotationTokensEnumerator.Current.value == "NOT")
                {
                    polishNotationTokensEnumerator.MoveNext();
                    BoolExpr operand = Make(ref polishNotationTokensEnumerator);
                    return BoolExpr.CreateNot(operand);
                }
                else if (polishNotationTokensEnumerator.Current.value == "AND")
                {
                    polishNotationTokensEnumerator.MoveNext();
                    BoolExpr right = Make(ref polishNotationTokensEnumerator);
                    BoolExpr left = Make(ref polishNotationTokensEnumerator);
                    return BoolExpr.CreateAnd(left, right);
                }
                else if (polishNotationTokensEnumerator.Current.value == "OR")
                {
                    polishNotationTokensEnumerator.MoveNext();
                    BoolExpr right = Make(ref polishNotationTokensEnumerator);
                    BoolExpr left = Make(ref polishNotationTokensEnumerator);
                    return BoolExpr.CreateOr(left, right);
                }
            }
            return null;
        }

        static public string PrettyPrint(BoolExpr root)
        {
            StringBuilder output = new StringBuilder();
            PrettyPrintHelper(ref output, root);
            if (Char.IsWhiteSpace(output[0]))
            {
                output.Remove(0, 1);
            }
            return output.ToString();
        }

        static void PrettyPrintHelper(ref StringBuilder stringBuilder, BoolExpr node)
        {
            if (node.Op == BoolExpr.BOP.LEAF)
            {
                stringBuilder.Append(node.Lit);
            }
            else if (node.Op == BoolExpr.BOP.NOT)
            {
                stringBuilder.Append(_lnot);
                //if (stringBuilder.Length == 0 || Char.IsWhiteSpace(stringBuilder[stringBuilder.Length - 1]))
                //{
                //    stringBuilder.Append("not ");
                //}
                //else
                //{
                //    stringBuilder.Append(" not ");
                //}
                PrettyPrintHelper(ref stringBuilder, node.Right);
            }
            else if (node.Op == BoolExpr.BOP.AND)
            {
                stringBuilder.Append('(');
                PrettyPrintHelper(ref stringBuilder, node.Left);
                stringBuilder.Append(_land);
                //stringBuilder.Append(" and ");
                PrettyPrintHelper(ref stringBuilder, node.Right);
                stringBuilder.Append(')');
            }
            else if (node.Op == BoolExpr.BOP.OR)
            {
                stringBuilder.Append('(');
                PrettyPrintHelper(ref stringBuilder, node.Left);
                //stringBuilder.Append(" or ");
                stringBuilder.Append(_lor);
                PrettyPrintHelper(ref stringBuilder, node.Right);
                stringBuilder.Append(')');
            }
        }
    }
}
