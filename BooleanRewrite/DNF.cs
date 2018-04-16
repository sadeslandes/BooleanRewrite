using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooleanRewrite
{
    class DNF
    {
        struct DNFLiteral
        {
            string Name;
            bool isNegated;
            string Text { get => isNegated ? LogicalSymbols.Not + Name : Name; }
        }

        private List<List<DNFLiteral>> expressionList;
        private Dictionary<string, int> priority;
        private int numVariables;

        public DNF(BoolExpr root, IEnumerable<string> variables)
        {
            int i = 0;
            foreach (var variable in variables)
            {
                priority.Add(variable, i++);
            }
            numVariables = i;

            buildListFromTree(root);
        }

        private void buildListFromTree(BoolExpr node)
        {
            if (node == null || node.Op == BoolExpr.BOP.LEAF)
                return;

            
        }
    }
}
