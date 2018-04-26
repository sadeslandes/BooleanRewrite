using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooleanRewrite
{
    static class Rewrite
    {
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
            if (node.Op == BoolExpr.BOP.NOT)
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
            if (node.Op == BoolExpr.BOP.NOT && node.Right.Op == BoolExpr.BOP.NOT)
            {
                var oldNode = node;
                node = node.Right.Right;
                node.Parent = oldNode.Parent;

                UpdateParent(node, oldNode);
                return true;
            }
            return false;
        }

        public static bool Implication(ref BoolExpr node)
        {
            if (node.Op == BoolExpr.BOP.CONDITIONAL)
            {
                var oldNode = node;
                var left = BoolExpr.CreateNot(node.Left);
                var right = node.Right;
                node = BoolExpr.CreateOr(left, right);
                node.Parent = oldNode.Parent;
                left.Parent = node;
                right.Parent = node;

                UpdateParent(node, oldNode);
                return true;
            }
            return false;
        }

        public static bool Equivalence(ref BoolExpr node)
        {
            if (node.Op == BoolExpr.BOP.BICONDITIONAL)
            {
                var oldNode = node;

                var copyLeft = new BoolExpr(node.Left);
                var copyRight = new BoolExpr(node.Right);
                var negatedLeft = BoolExpr.CreateNot(copyLeft);
                var negatedRight = BoolExpr.CreateNot(copyRight);
                copyLeft.Parent = negatedLeft;
                copyRight.Parent = negatedRight;

                var left = BoolExpr.CreateAnd(node.Left, node.Right);
                node.Left.Parent = left;
                node.Right.Parent = left;

                var right = BoolExpr.CreateAnd(negatedLeft, negatedRight);
                negatedLeft.Parent = right;
                negatedRight.Parent = right;

                node = BoolExpr.CreateOr(left, right);

                node.Parent = oldNode.Parent;
                left.Parent = node;
                right.Parent = node;

                UpdateParent(node, oldNode);
                return true;
            }
            return false;
        }

        public static bool XOR(ref BoolExpr node)
        {
            if (node.Op == BoolExpr.BOP.XOR)
            {
                var oldNode = node;

                var copyLeft = new BoolExpr(node.Left);
                var copyRight = new BoolExpr(node.Right);
                var negatedLeft = BoolExpr.CreateNot(copyLeft);
                var negatedRight = BoolExpr.CreateNot(copyRight);
                copyLeft.Parent = negatedLeft;
                copyRight.Parent = negatedRight;

                var left = BoolExpr.CreateAnd(node.Left, negatedRight);
                node.Left.Parent = left;
                negatedRight.Parent = left;

                var right = BoolExpr.CreateAnd(negatedLeft, node.Right);
                negatedLeft.Parent = right;
                node.Right.Parent = right;

                node = BoolExpr.CreateOr(left, right);

                node.Parent = oldNode.Parent;
                left.Parent = node;
                right.Parent = node;

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
