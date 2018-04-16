using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooleanRewrite
{
    class DNFLiteral : IComparable<DNFLiteral>
    {
        public string Name;
        public bool isNegated;
        public string Text { get => isNegated ? LogicalSymbols.Not + Name : Name; }
        public int priority;

        public int CompareTo(DNFLiteral other)
        {
            return this.priority.CompareTo(other.priority);
        }
    }

    class DNF : IComparer<List<DNFLiteral>>
    {

        private List<List<DNFLiteral>> expressionList;
        private List<string> variables;
        private int numVariables;

        public DNF(BoolExpr root, IEnumerable<string> variables)
        {
            this.variables = variables.ToList();
            expressionList = new List<List<DNFLiteral>>();
            buildListFromTree(root);
        }

        public int Compare(List<DNFLiteral> x, List<DNFLiteral> y)
        {
            string binaryX = String.Join("",x.Select(l => l.isNegated ? "0" : "1"));
            string binaryY = String.Join("",x.Select(l => l.isNegated ? "0" : "1"));
            return Convert.ToInt32(binaryX, 2).CompareTo(Convert.ToInt32(binaryY, 2));
        }

        public void ConvertToCDNF(IList<ConversionStep> steps)
        {
            foreach(var group in expressionList)
            {
                foreach(var variable in variables)
                {
                    int varCount = group.Count(x => x.Name == variable);
                    if (varCount > 1)
                    {
                        // remove variable using complement and identity, or idempotence
                    }
                    else if(varCount < 1)
                    {
                        // add variable using identity, complement, then distribution
                    }
                    // order group
                    group.Sort();
                }
            }
            expressionList.Sort(this);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            for(int i=0;i<expressionList.Count; i++)
            {
                var group = expressionList[i];
                sb.Append('(');
                for (int j = 0; j < group.Count; j++)
                {
                    sb.Append(group[j].Text);
                    if (j != group.Count - 1)
                    {
                        sb.Append(LogicalSymbols.And);
                    }
                }
                sb.Append(')');
                if(i != expressionList.Count-1)
                {
                    sb.Append(LogicalSymbols.Or);
                }
            }

            return sb.ToString();
        }

        private void buildListFromTree(BoolExpr node)
        {
            if (node == null)
                return;

            if (node.Op != BoolExpr.BOP.OR)
            {
                var group = new List<DNFLiteral>();
                buildNode(node, group);
                group.Sort();
                expressionList.Add(group);
                return;
            }

            buildListFromTree(node.Left);
            buildListFromTree(node.Right);
            
        }

        private void buildNode(BoolExpr node, List<DNFLiteral> group)
        {
            if (node == null)
                return;

            if(node.Op == BoolExpr.BOP.LEAF)
            {
                group.Add(new DNFLiteral() { Name = node.Lit, isNegated = false, priority = variables.IndexOf(node.Lit) });
                return;
            }

            if (node.Op == BoolExpr.BOP.NOT)
            {
                group.Add(new DNFLiteral() { Name = node.Right.Lit, isNegated = true, priority = variables.IndexOf(node.Right.Lit) });
                return;
            }

            buildNode(node.Left, group);
            buildNode(node.Right, group);
        }
    }
}
