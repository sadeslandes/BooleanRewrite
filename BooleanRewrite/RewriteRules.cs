namespace BooleanRewrite
{
    /// <summary>
    /// Contains rewrite rules for implementing various equivalence rules on a tree of BoolExpr objects
    /// </summary>
    static class Rewrite
    {
        /// <summary>
        /// Distributes and over or
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool Distribution(ref BoolExpr node)
        {
            if (node.Op == OperatorType.AND)
            {
                if (node.Right.Op == OperatorType.OR)
                {
                    DistributionHelper(ref node);
                    return true;
                }
                else if (node.Left.Op == OperatorType.OR)
                {
                    DistributionHelper(ref node);
                    return true;
                }
            }
            return false;
        }
        private static void DistributionHelper(ref BoolExpr node)
        {
            var oldRight = node.Right;
            var oldLeft = node.Left;
            var oldNode = node;

            var left = new BoolExprConjunction(oldLeft.Left, oldRight);
            var right = new BoolExprConjunction(oldLeft.Right, oldRight);

            node = new BoolExprDisjunction(left, right);
            node.Parent = oldNode.Parent;
            left.Parent = node;
            right.Parent = node;

            UpdateParent(node, oldNode);
        }


        /// <summary>
        /// DeMorgan's Law
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool DeM(ref BoolExpr node)
        {
            if (node.Op == OperatorType.NOT)
            {
                if (node.Right.Op == OperatorType.AND)
                {
                    DeMHelper(ref node);
                    return true;
                }
                else if (node.Right.Op == OperatorType.OR)
                {
                    DeMHelper(ref node);
                    return true;
                }
            }
            return false;
        }
        private static void DeMHelper(ref BoolExpr node)
        {
            var oldNode = node;
            var temp = node.Right;
            var left = new BoolExprNegation(temp.Left);
            var right = new BoolExprNegation(temp.Right);
            node = new BoolExprDisjunction(left, right);
            node.Parent = oldNode.Parent;
            left.Parent = node;
            right.Parent = node;

            UpdateParent(node, oldNode);
        }

        /// <summary>
        /// Double negation
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool DN(ref BoolExpr node)
        {
            if (node.Op == OperatorType.NOT && node.Right.Op == OperatorType.NOT)
            {
                var oldNode = node;
                node = node.Right.Right;
                node.Parent = oldNode.Parent;

                UpdateParent(node, oldNode);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Implication rule
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool Implication(ref BoolExpr node)
        {
            if (node.Op == OperatorType.CONDITIONAL)
            {
                var oldNode = node;
                var left = new BoolExprNegation(node.Left);
                var right = node.Right;
                node = new BoolExprDisjunction(left, right);
                node.Parent = oldNode.Parent;
                left.Parent = node;
                right.Parent = node;

                UpdateParent(node, oldNode);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Equivalence rule
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool Equivalence(ref BoolExpr node)
        {
            if (node.Op == OperatorType.BICONDITIONAL)
            {
                var oldNode = node;

                var copyLeft = new BoolExpr(node.Left);
                var copyRight = new BoolExpr(node.Right);
                var negatedLeft = new BoolExprNegation(copyLeft);
                var negatedRight = new BoolExprNegation(copyRight);
                copyLeft.Parent = negatedLeft;
                copyRight.Parent = negatedRight;

                var left = new BoolExprConjunction(node.Left, node.Right);
                node.Left.Parent = left;
                node.Right.Parent = left;

                var right = new BoolExprConjunction(negatedLeft, negatedRight);
                negatedLeft.Parent = right;
                negatedRight.Parent = right;

                node = new BoolExprDisjunction(left, right);

                node.Parent = oldNode.Parent;
                left.Parent = node;
                right.Parent = node;

                UpdateParent(node, oldNode);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Xor brekdown
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static bool XOR(ref BoolExpr node)
        {
            if (node.Op == OperatorType.XOR)
            {
                var oldNode = node;

                var copyLeft = new BoolExpr(node.Left);
                var copyRight = new BoolExpr(node.Right);
                var negatedLeft = new BoolExprNegation(copyLeft);
                var negatedRight = new BoolExprNegation(copyRight);
                copyLeft.Parent = negatedLeft;
                copyRight.Parent = negatedRight;

                var left = new BoolExprConjunction(node.Left, negatedRight);
                node.Left.Parent = left;
                negatedRight.Parent = left;

                var right = new BoolExprConjunction(negatedLeft, node.Right);
                negatedLeft.Parent = right;
                node.Right.Parent = right;

                node = new BoolExprDisjunction(left, right);

                node.Parent = oldNode.Parent;
                left.Parent = node;
                right.Parent = node;

                UpdateParent(node, oldNode);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates the oldNode parent node to point to the new node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="oldNode"></param>
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
