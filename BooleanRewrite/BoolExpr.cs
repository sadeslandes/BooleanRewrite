using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooleanRewrite
{
    class BoolExpr
    {
        public enum BOP
        {
            LEAF,
            AND,
            OR,
            NOT
        };

        //
        //  inner state
        //

        private BOP _op;
        private BoolExpr _left;
        private BoolExpr _right;
        private BoolExpr _parent;
        private String _lit;

        //
        //  private constructor
        //

        private BoolExpr(BOP op, BoolExpr left, BoolExpr right)
        {
            _op = op;
            _left = left;
            _right = right;
            _lit = null;
            _parent = null;
        }

        private BoolExpr(String literal)
        {
            _op = BOP.LEAF;
            _left = null;
            _right = null;
            _lit = literal;
            _parent = null;
        }

        //
        //  accessor
        //

        public BOP Op
        {
            get { return _op; }
            set { _op = value; }
        }

        public BoolExpr Left
        {
            get { return _left; }
            set { _left = value; }
        }

        public BoolExpr Right
        {
            get { return _right; }
            set { _right = value; }
        }

        public BoolExpr Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public String Lit
        {
            get { return _lit; }
            set { _lit = value; }
        }

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

        public BoolExpr(BoolExpr other)
        {
            // No share any object on purpose
            _op = other._op;
            _left = other._left == null ? null : new BoolExpr(other._left);
            _right = other._right == null ? null : new BoolExpr(other._right);
            _lit = new StringBuilder(other._lit).ToString();
        }

        //
        //  state checker
        //

        Boolean IsLeaf()
        {
            return (_op == BOP.LEAF);
        }

        Boolean IsAtomic()
        {
            return (IsLeaf() || (_op == BOP.NOT && _left.IsLeaf()));
        }
    }
}
