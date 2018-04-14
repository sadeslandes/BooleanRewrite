using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BooleanRewrite
{
    public class AST
    {

        public AST(List<Token> tokens)
        {
            var enumerator = tokens.GetEnumerator();
            enumerator.MoveNext();
            Root = Make(ref enumerator);
        }

        const char _lnot = '\u00ac';
        const char _land = '\u2227';
        const char _lor = '\u2228';

        public BoolExpr Root
        {
            get;
            set;
        }

        BoolExpr Make(ref List<Token>.Enumerator polishNotationTokensEnumerator)
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
                    var parent = BoolExpr.CreateNot(operand);
                    operand.Parent = parent;
                    return parent;
                }
                else if (polishNotationTokensEnumerator.Current.value == "AND")
                {
                    polishNotationTokensEnumerator.MoveNext();
                    BoolExpr right = Make(ref polishNotationTokensEnumerator);
                    BoolExpr left = Make(ref polishNotationTokensEnumerator);
                    var parent = BoolExpr.CreateAnd(left, right);
                    left.Parent = parent;
                    right.Parent = parent;
                    return parent;
                }
                else if (polishNotationTokensEnumerator.Current.value == "OR")
                {
                    polishNotationTokensEnumerator.MoveNext();
                    BoolExpr right = Make(ref polishNotationTokensEnumerator);
                    BoolExpr left = Make(ref polishNotationTokensEnumerator);
                    var parent = BoolExpr.CreateOr(left, right);
                    left.Parent = parent;
                    right.Parent = parent;
                    return parent;
                }
            }
            return null;
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();

            PrettyPrintHelper(output, Root);
            if (Char.IsWhiteSpace(output[0]))
            {
                output.Remove(0, 1);
            }
            return output.ToString();
        }

        void PrettyPrintHelper(StringBuilder stringBuilder, BoolExpr node)
        {
            if (node.Op == BoolExpr.BOP.LEAF)
            {
                stringBuilder.Append(node.Lit);
            }
            else if (node.Op == BoolExpr.BOP.NOT)
            {
                stringBuilder.Append(_lnot);
                PrettyPrintHelper(stringBuilder, node.Right);
            }
            else if (node.Op == BoolExpr.BOP.AND)
            {
                stringBuilder.Append('(');
                PrettyPrintHelper(stringBuilder, node.Left);
                stringBuilder.Append(_land);
                PrettyPrintHelper(stringBuilder, node.Right);
                stringBuilder.Append(')');
            }
            else if (node.Op == BoolExpr.BOP.OR)
            {
                stringBuilder.Append('(');
                PrettyPrintHelper(stringBuilder, node.Left);
                stringBuilder.Append(_lor);
                PrettyPrintHelper(stringBuilder, node.Right);
                stringBuilder.Append(')');
            }
        }

        public IList<ConversionStep> Evaluate()
        {
            BoolExpr root;
            var steps = new List<ConversionStep>();
            steps.Add(new ConversionStep(ToString(), "Input"));

            if(!IsNNF(Root))
            {
                root = Root;
                ConvertToNNF(ref root, steps);
                Root = root;
            }
            Debug.Assert(IsNNF(Root));

            if (!IsDNF(Root))
            {
                root = Root;
                ConvertNNFtoDNF(ref root, steps);
                Root = root;
            }
            Debug.Assert(IsDNF(Root));

            // TODO: clean-up using complement then identity
            root = Root;
            RemoveIdentities(ref root, steps);
            Root = root;
            Debug.Assert(IsDNF(Root));

            // TODO: convert to CDNF

            return steps;
        }

        private void RemoveIdentities(ref BoolExpr node, List<ConversionStep> steps)
        {
            if (node == null || node.Op == BoolExpr.BOP.LEAF)
                return;

            if(Rewrite.ComplementIdentity(ref node))
            {
                if (node.Parent == null)
                {
                    Root = node;
                }
                steps.Add(new ConversionStep(ToString(), "Identity"));
            }

            var right = node.Right;
            var left = node.Left;
            RemoveIdentities(ref right, steps);
            RemoveIdentities(ref left, steps);
            node.Right = right;
            node.Left = left;
        }

        void ConvertToNNF(ref BoolExpr node, IList<ConversionStep> steps)
        {
            if (node == null || node.Op == BoolExpr.BOP.LEAF)
                return;

            // try to apply DN as many times as possible
            while(Rewrite.DN(ref node))
            {
                if (node.Parent == null)
                {
                    Root = node;
                }
                steps.Add(new ConversionStep(ToString(), "Double Negation"));
            }

            // try to apply DeM
            if(Rewrite.DeM(ref node))
            {
                if (node.Parent == null)
                {
                    Root = node;
                }
                steps.Add(new ConversionStep(ToString(), "DeMorgan's"));
            }


            var right = node.Right;
            var left = node.Left;
            ConvertToNNF(ref right, steps);
            ConvertToNNF(ref left, steps);
            node.Right = right;
            node.Left = left;
        }

        void ConvertNNFtoDNF(ref BoolExpr node, IList<ConversionStep> steps)
        {
            if (node == null || node.Op == BoolExpr.BOP.LEAF)
                return;

            // try to apply distribution
            if (Rewrite.Distribution(ref node))
            {
                if (node.Parent == null)
                {
                    Root = node;
                }
                steps.Add(new ConversionStep(ToString(), "Distribution"));
            }

            var right = node.Right;
            var left = node.Left;
            ConvertNNFtoDNF(ref right, steps);
            ConvertNNFtoDNF(ref left, steps);
            node.Right = right;
            node.Left = left;
        }

        bool IsDNF(BoolExpr node)
        {
            switch (node.Op)
            {
                case BoolExpr.BOP.LEAF:
                    return true;
                case BoolExpr.BOP.AND:
                    return IsDNF(node.Right) && IsDNF(node.Left);
                case BoolExpr.BOP.OR:
                    return  (node.Parent == null || node.Parent.Op == BoolExpr.BOP.OR) && IsDNF(node.Right) && IsDNF(node.Left);
                case BoolExpr.BOP.NOT:
                    return node.Right.Op == BoolExpr.BOP.LEAF;
                default:
                    return false;
            }
        }

        bool IsNNF(BoolExpr node)
        {
            switch (node.Op)
            {
                case BoolExpr.BOP.LEAF:
                    return true;
                case BoolExpr.BOP.AND:
                case BoolExpr.BOP.OR:
                    return IsNNF(node.Right) && IsNNF(node.Left);
                case BoolExpr.BOP.NOT:
                    return node.Right.Op == BoolExpr.BOP.LEAF;
                default:
                    return false;
            }
        }
    }

    static class Rewrite
    {
        public static bool ComplementIdentity(ref BoolExpr node)
        {
            bool rewriteCondition = false;
            BoolExpr other;

            if (node.Op == BoolExpr.BOP.AND && node.Parent != null)
            {
                if(node.Left.Op == BoolExpr.BOP.NOT)
                {
                    if(node.Left.Right == node.Right)
                    {
                        rewriteCondition = true;
                    }
                }
                else if(node.Right.Op == BoolExpr.BOP.NOT)
                {
                    if (node.Right.Right.Lit == node.Left.Lit)
                    {
                        rewriteCondition = true;
                    }
                }
            }

            if (rewriteCondition)
            {
                if (node.Parent.Left == node)
                {
                    other = node.Parent.Right;
                }
                else
                {
                    other = node.Parent.Left;
                }

                var newParent = node.Parent.Parent;
                node = other;
                other.Parent = newParent;

                return true;
            }

            return false;
        }

        public static bool Distribution(ref BoolExpr node)
        {
            if (node.Op == BoolExpr.BOP.AND)
            {
                if (node.Right.Op == BoolExpr.BOP.OR)
                {
                    var oldRight = node.Right;
                    var oldLeft = node.Left;
                    var oldNode = node;

                    var left = BoolExpr.CreateAnd(oldLeft, oldRight.Left);
                    var right = BoolExpr.CreateAnd(oldLeft, oldRight.Right);

                    node = BoolExpr.CreateOr(left, right);
                    node.Parent = oldNode.Parent;
                    left.Parent = node;
                    right.Parent = node;
                    
                    UpdateParent(node, oldNode);
                    return true;
                }
                else if (node.Left.Op == BoolExpr.BOP.OR)
                {
                    var oldRight = node.Right;
                    var oldLeft = node.Left;
                    var oldNode = node;

                    var left = BoolExpr.CreateAnd(oldLeft.Left, oldRight);
                    var right = BoolExpr.CreateAnd(oldLeft.Right, oldRight);

                    node = BoolExpr.CreateOr(left, right);
                    node.Parent = oldNode.Parent;
                    left.Parent = node;
                    right.Parent = node;

                    UpdateParent(node, oldNode);
                    return true;
                }
            }
            return false;
        }

        public static bool DeM(ref BoolExpr node)
        {
            if(node.Op == BoolExpr.BOP.NOT)
            {
                if (node.Right.Op == BoolExpr.BOP.AND)
                {
                    var oldNode = node;
                    var temp = node.Right;
                    var left = BoolExpr.CreateNot(temp.Left);
                    var right = BoolExpr.CreateNot(temp.Right);
                    node = BoolExpr.CreateOr(left, right);
                    node.Parent = oldNode.Parent;
                    left.Parent = node;
                    right.Parent = node;
                    
                    UpdateParent(node, oldNode);    
                    return true;
                }
                else if (node.Right.Op == BoolExpr.BOP.OR)
                {
                    var oldNode = node;
                    var temp = node.Right;
                    var left = BoolExpr.CreateNot(temp.Left);
                    var right = BoolExpr.CreateNot(temp.Right);
                    node = BoolExpr.CreateAnd(left, right);
                    node.Parent = oldNode.Parent;
                    left.Parent = node;
                    right.Parent = node;
                    
                    UpdateParent(node, oldNode);
                    return true;
                }
            }
            return false;
        }

        public static bool DN(ref BoolExpr node)
        {
            if(node.Op == BoolExpr.BOP.NOT && node.Right.Op == BoolExpr.BOP.NOT)
            {
                var oldNode = node;
                node = node.Right.Right;
                node.Parent = oldNode.Parent;

                UpdateParent(node, oldNode);
                return true;
            }
            return false;
        }

        private static void UpdateParent(BoolExpr node, BoolExpr oldNode)
        {
            if (node.Parent == null || oldNode.Parent == null)
                return;

            if (oldNode == oldNode.Parent.Left)
            {
                node.Parent.Left = node;
            }
            else
            {
                node.Parent.Right = node;
            }
        }
    }
}
