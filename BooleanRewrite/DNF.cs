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
            for(int i=0;i<expressionList.Count;)
            {
                bool incrementCounter = true;
                foreach(var variable in variables)
                {
                    var group = expressionList[i];
                    int varCount = group.Count(x => x.Name == variable);
                    var originalGroup = new List<DNFLiteral>(group);
                    if (varCount > 1)
                    {
                        bool removed = false;

                        // remove duplicates
                        while(group.Count(x => x.Name == variable && !x.isNegated) > 1)
                        {
                            group.Remove(group.First(x => x.Name == variable && !x.isNegated));
                            removed = true;
                        }
                        if(removed)
                        {
                            steps.Add(new ConversionStep(ToString(), "Idempotence"));
                            removed = false;
                        }

                        while (group.Count(x => x.Name == variable && x.isNegated) > 1)
                        {
                            group.Remove(group.First(x => x.Name == variable && x.isNegated));
                            removed = true;
                        }
                        if (removed)
                        {
                            steps.Add(new ConversionStep(ToString(), "Idempotence"));
                            removed = false;
                        }

                        // remove contradiction
                        if (group.Count(x => x.Name == variable) > 1)
                        {
                            group.RemoveAll(x => x.Name == variable);
                            group.Add(new DNFLiteral() { Name = LogicalSymbols.Contradiction.ToString() });
                            steps.Add(new ConversionStep(ToString(), "Complement"));
                            group.RemoveAll(x => x.Name != LogicalSymbols.Contradiction.ToString());
                            steps.Add(new ConversionStep(ToString(), "Annihilation"));
                            expressionList.Remove(expressionList[i]);
                            steps.Add(new ConversionStep(ToString(), "Identity"));
                            incrementCounter = false;
                            break;
                        }


                    }
                    if(varCount < 1)
                    {
                        // add variable using identity, complement, then distribution
                        // identity
                        group.Add(new DNFLiteral() { Name = $"{LogicalSymbols.Tautology}", isNegated = false, priority = Int32.MaxValue });
                        steps.Add(new ConversionStep(ToString(), "Identity"));
                        // complement
                        group.Remove(group.Last());
                        var newPositive = new DNFLiteral() { Name = variable, isNegated = false, priority = variables.IndexOf(variable) };
                        var newNegative = new DNFLiteral() { Name = variable, isNegated = true, priority = variables.IndexOf(variable) };
                        group.Add(new DNFLiteral() { Name = $"({newPositive.Text}{LogicalSymbols.Or}{newNegative.Text})", priority = Int32.MaxValue });
                        steps.Add(new ConversionStep(ToString(), "Complement"));
                        // distribute
                        expressionList.RemoveAt(i);
                        var positiveGroup = originalGroup;
                        var negativeGroup = new List<DNFLiteral>(originalGroup);
                        positiveGroup.Add(newPositive);
                        expressionList.Insert(i, positiveGroup);
                        negativeGroup.Add(newNegative);
                        expressionList.Insert(i, negativeGroup);
                        steps.Add(new ConversionStep(ToString(), "Distribution"));

                        incrementCounter = false;
                    }
                    // order group
                    expressionList[i].Sort();
                }
                if(incrementCounter)
                {
                    i++;
                }
            }
            //expressionList.Sort(this);
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
