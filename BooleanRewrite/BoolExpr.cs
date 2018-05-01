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

    #region Base Type
    public abstract class BoolExpr
    {
        //
        // constructors
        //

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

    }
    #endregion

    #region Factory
    static class BoolExprFactory
    {
        /// <summary>
        /// Deep copy of BoolExpr
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public static BoolExpr CreateCopy(BoolExpr other)
        {
            switch (other.Op)
            {
                case OperatorType.LEAF:
                    return CreateLiteral(other.Lit);
                case OperatorType.NOT:
                { // keep copy variable local to this case only
                    var copy = new BoolExprNegation(CreateCopy(other.Right));
                    copy.Right.Parent = copy;
                    return copy;
                }
                case OperatorType.AND:
                case OperatorType.OR:
                case OperatorType.CONDITIONAL:
                case OperatorType.BICONDITIONAL:
                case OperatorType.XOR:
                { // keep copy variable local to this case only
                    var copy =  CreateBinary(other.Op.ToString(), CreateCopy(other.Left), CreateCopy(other.Right));
                    copy.Left.Parent = copy;
                    copy.Right.Parent = copy;
                    return copy;
                }
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
    #endregion

    #region Derived Types
    public abstract class BoolExprBinary : BoolExpr
    {
        public BoolExprBinary(OperatorType op, BoolExpr left, BoolExpr right) : base(op,left,right)
        {
        }

        public override string ToString()
        {
            return $"({Left.ToString()}{Lit}{Right.ToString()})";
        }
    }

    class BoolExprLiteral : BoolExpr
    {
        public BoolExprLiteral(string literal) : base(literal)
        {     
        }

        public override string ToString()
        {
            return Lit;
        }
    }

    class BoolExprNegation : BoolExpr
    {
        public BoolExprNegation(BoolExpr child) : base(OperatorType.NOT, null, child)
        {
        }

        public override string ToString()
        {
            return LogicalSymbols.Not + Right.ToString();
        }
    }

    class BoolExprConjunction : BoolExprBinary
    {
        public BoolExprConjunction(BoolExpr left, BoolExpr right) : base(OperatorType.AND, left, right)
        {
            Lit = LogicalSymbols.And.ToString();
        }
    }

    class BoolExprDisjunction : BoolExprBinary
    {
        public BoolExprDisjunction(BoolExpr left, BoolExpr right) : base(OperatorType.OR, left, right)
        {
            Lit = LogicalSymbols.Or.ToString();
        }
    }

    class BoolExprConditional : BoolExprBinary
    {
        public BoolExprConditional(BoolExpr left, BoolExpr right) : base(OperatorType.CONDITIONAL, left, right)
        {
            Lit = LogicalSymbols.Conditional.ToString();
        }
    }

    class BoolExprBiconditional : BoolExprBinary
    {
        public BoolExprBiconditional(BoolExpr left, BoolExpr right) : base(OperatorType.BICONDITIONAL, left, right)
        {
            Lit = LogicalSymbols.Biconditional.ToString();
        }
    }

    class BoolExprXOR : BoolExprBinary
    {
        public BoolExprXOR(BoolExpr left, BoolExpr right) : base(OperatorType.XOR, left, right)
        {
            Lit = LogicalSymbols.XOr.ToString();
        }
    }
    #endregion
}



