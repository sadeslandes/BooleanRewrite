using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooleanRewrite
{
    public enum OperatorType
    {
        LEAF,
        AND,
        OR,
        NOT,
        CONDITIONAL,
        BICONDITIONAL,
        XOR
    };

    public abstract class BoolExpr : ICloneable
    {
        protected BoolExpr(OperatorType op, BoolExpr left, BoolExpr right)
        {
            Op = op;
            Left = left;
            Right = right;
            Lit = null;
            Parent = null;
        }

        protected BoolExpr(String literal)
        {
            Op = OperatorType.LEAF;
            Left = null;
            Right = null;
            Lit = literal;
            Parent = null;
        }

        //
        //  accessor
        //

        public OperatorType Op { get; set; }

        public BoolExpr Left { get; set; }

        public BoolExpr Right { get; set; }

        public BoolExpr Parent { get; set; }

        public String Lit { get; set; }

        

        //
        //  state checker
        //

        public bool IsLeaf()
        {
            return (Op == OperatorType.LEAF);
        }

        public bool IsLiteral()
        {
            return (IsLeaf() || (Op == OperatorType.NOT && Right.IsLeaf()));
        }

        public bool IsContradiction() => Lit == LogicalSymbols.Contradiction.ToString();

        protected BoolExpr(BoolExpr other)
        {
            // No share any object on purpose
            Op = other.Op;
            Left = other.Left == null ? null : new BoolExpr(other.Left);
            Right = other.Right == null ? null : new BoolExpr(other.Right);
            Lit = new StringBuilder(other.Lit).ToString();
        }

        public abstract object Clone();

    }

    static class BoolExprFactory
    {
        public static BoolExpr CreateCopy(BoolExpr other)
        {
            switch (other.Op)
            {
                case OperatorType.LEAF:
                    return CreateLiteral(other.Lit);
                case OperatorType.AND:
                    return new BoolExprConjunction(CreateCopy(other.Left), CreateCopy(other.Right));
                case OperatorType.OR:
                    break;
                case OperatorType.NOT:
                    break;
                case OperatorType.CONDITIONAL:
                    break;
                case OperatorType.BICONDITIONAL:
                    break;
                case OperatorType.XOR:
                    break;
                default:
                    return null;
            }
        }

        public static BoolExpr CreateLiteral(string literal) => new BoolExprLiteral(literal);
        public static BoolExpr CreateNot(BoolExpr child) => new BoolExprNegation(child);

        public static BoolExpr CreateBinary(string op, BoolExpr left, BoolExpr right)
        {
            switch (op)
            {
                case "AND":
                    return new BoolExprConjunction(left, right);
                case "OR":
                    return new BoolExprDisjunction(left, right);
                case "CONDITIONAL":
                    return new BoolExprConditional(left, right);
                case "BICONDITIONAL":
                    return new BoolExprBiconditional(left, right);
                case "XOR":
                    return new BoolExprXOR(left, right);
                default:
                    throw new Exception();
            }
        }

        public static BoolExpr CreateContradiction() => new BoolExprLiteral(LogicalSymbols.Contradiction.ToString());
    }

    class BoolExprLiteral : BoolExpr
    {
        public BoolExprLiteral(string literal) : base(literal)
        {     
        }
    }

    class BoolExprNegation : BoolExpr
    {
        public BoolExprNegation(BoolExpr child) : base(OperatorType.NOT, null, child)
        {
        }
    }

    class BoolExprConjunction : BoolExpr
    {
        public BoolExprConjunction(BoolExpr left, BoolExpr right) : base(OperatorType.AND, left, right)
        {
        }
    }

    class BoolExprDisjunction : BoolExpr
    {
        public BoolExprDisjunction(BoolExpr left, BoolExpr right) : base(OperatorType.AND, left, right)
        {
        }
    }

    class BoolExprConditional : BoolExpr
    {
        public BoolExprConditional(BoolExpr left, BoolExpr right) : base(OperatorType.AND, left, right)
        {
        }
    }

    class BoolExprBiconditional : BoolExpr
    {
        public BoolExprBiconditional(BoolExpr left, BoolExpr right) : base(OperatorType.AND, left, right)
        {
        }
    }

    class BoolExprXOR : BoolExpr
    {
        public BoolExprXOR(BoolExpr left, BoolExpr right) : base(OperatorType.AND, left, right)
        {
        }
    }
}



