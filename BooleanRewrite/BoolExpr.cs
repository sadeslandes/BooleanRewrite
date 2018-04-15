using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooleanRewrite
{
    public class BoolExpr
    {
        public enum BOP
        {
            LEAF,
            AND,
            OR,
            NOT
        };

        //
        //  private constructor
        //

        private BoolExpr(BOP op, BoolExpr left, BoolExpr right)
        {
            Op = op;
            Left = left;
            Right = right;
            Lit = null;
            Parent = null;
        }

        private BoolExpr(String literal)
        {
            Op = BOP.LEAF;
            Left = null;
            Right = null;
            Lit = literal;
            Parent = null;
        }

        //
        //  accessor
        //

        public BOP Op { get; set; }

        public BoolExpr Left { get; set; }

        public BoolExpr Right { get; set; }

        public BoolExpr Parent { get; set; }

        public String Lit { get; set; }

        //
        //  public factory
        //

        public static BoolExpr CreateAnd(BoolExpr left, BoolExpr right)
        {
            return new BoolExpr(BOP.AND, left, right);
        }

        public static BoolExpr CreateNot(BoolExpr child)
        {
            return new BoolExpr(BOP.NOT, null, child);
        }

        public static BoolExpr CreateOr(BoolExpr left, BoolExpr right)
        {
            return new BoolExpr(BOP.OR, left, right);
        }

        public static BoolExpr CreateBoolVar(String str)
        {
            return new BoolExpr(str);
        }

        public static BoolExpr CreateContradiction()
        {
            return new BoolExpr("\u22a5");
        }

        


        public BoolExpr(BoolExpr other)
        {
            // No share any object on purpose
            Op = other.Op;
            Left = other.Left == null ? null : new BoolExpr(other.Left);
            Right = other.Right == null ? null : new BoolExpr(other.Right);
            Lit = new StringBuilder(other.Lit).ToString();
        }

        //
        //  state checker
        //

        public bool IsLeaf()
        {
            return (Op == BOP.LEAF);
        }

        public bool IsLiteral()
        {
            return (IsLeaf() || (Op == BOP.NOT && Right.IsLeaf()));
        }

        public bool IsContradiction() => Lit == "\u22a5";

        // unsused test
        public bool IsContradiction2()
        {
            if(this.Op == BOP.AND)
            {
                if(this.Right.Op == BOP.NOT)
                {
                    if(this.Right.Right == this.Left)
                    {
                        return true;
                    }
                }
                if (this.Left.Op == BOP.NOT)
                {
                    if (this.Left.Right == this.Right)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
    }
}
