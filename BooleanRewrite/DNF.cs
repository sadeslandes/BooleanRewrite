using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooleanRewrite
{
    /// <summary>
    /// Represents a propositional logic literal used in a DNFConjunctionGroup
    /// </summary>
    class DNFLiteral : IComparable<DNFLiteral>
    {
        public string Name;
        public bool isNegated;
        public string Text { get => isNegated ? LogicalSymbols.Not + Name : Name; }
        public int priority;
        public override string ToString() => Text;
        public int CompareTo(DNFLiteral other)
        {
            return this.priority.CompareTo(other.priority);
        }
    }

    /// <summary>
    /// Represents a conjunction of literals in a DNFExpression
    /// </summary>
    class DNFConjunctionGroup : List<DNFLiteral>, IComparable<DNFConjunctionGroup>, IEquatable<DNFConjunctionGroup>
    {
        public DNFConjunctionGroup() : base() { }
        public DNFConjunctionGroup(DNFConjunctionGroup other) : base(other) { }

        public int CompareTo(DNFConjunctionGroup other)
        {
            string binaryX = String.Join("", this.Select(l => l.isNegated ? "1" : "0"));
            string binaryY = String.Join("", other.Select(l => l.isNegated ? "1" : "0"));
            return Convert.ToInt32(binaryX, 2).CompareTo(Convert.ToInt32(binaryY, 2));
        }

        public bool Equals(DNFConjunctionGroup other)
        {
            return this.GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode()
        {
            int hash = 0;
            foreach (var literal in this)
            {
                hash += literal.Text.GetHashCode();
            }
            hash *= this.Count;
            return hash;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Count > 1) sb.Append("(");
            foreach(var literal in this)
            {
                sb.Append(literal.ToString());
                if(literal != this.Last())
                {
                    sb.Append(LogicalSymbols.And);
                }
            }
            if (Count > 1) sb.Append(")");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Representation of a DNF expressions. It is simply a collection of DNFConjunctionGroups
    /// </summary>
    class DNFExpression
    {
        private List<DNFConjunctionGroup> expressionList;
        private List<string> variables;

        public DNFExpression(BoolExpr root, IEnumerable<string> variables)
        {
            this.variables = variables.ToList();
            expressionList = new List<DNFConjunctionGroup>();
            buildListFromTree(root);
        }

        public void ConvertToCDNF(IList<ConversionStep> steps, bool reverseOrder)
        {
            for(int i=0;i<expressionList.Count;)
            {
                bool incrementCounter = true;
                bool removedGroup = false;
                foreach (var variable in variables)
                {
                    var group = expressionList[i];
                    int varCount = group.Count(x => x.Name == variable);
                    if (varCount > 1)
                    {
                        bool removed = false;

                        // remove duplicates
                        while (group.Count(x => x.Name == variable && !x.isNegated) > 1)
                        {
                            group.Remove(group.First(x => x.Name == variable && !x.isNegated));
                            removed = true;
                        }
                        if (removed)
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
                            if (group.Count > 1)
                            {
                                group.RemoveAll(x => x.Name != LogicalSymbols.Contradiction.ToString());
                                steps.Add(new ConversionStep(ToString(), "Annihilation"));
                            }
                            expressionList.Remove(expressionList[i]);
                            steps.Add(new ConversionStep(ToString(), "Identity"));
                            removedGroup = true;
                            break;
                        }
                    }
                }

                if (removedGroup)
                    continue;

                foreach (var variable in variables)
                {
                    var group = expressionList[i];
                    int varCount = group.Count(x => x.Name == variable);
                    var originalGroup = new DNFConjunctionGroup(group);
                    if (varCount < 1)
                    {
                        // Need to add variable

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
                        var negativeGroup = new DNFConjunctionGroup(originalGroup);
                        negativeGroup.Add(newNegative);
                        expressionList.Insert(i, negativeGroup);
                        positiveGroup.Add(newPositive);
                        expressionList.Insert(i, positiveGroup);
                        steps.Add(new ConversionStep(ToString(), "Distribution"));

                        incrementCounter = false;
                    }
                    // order group
                    expressionList[i].Sort();
                }

                if (incrementCounter)
                {
                    i++;
                }
            }
            // to show proper ordering within conjunctions
            steps.Add(new ConversionStep(ToString(), "Commutation"));

            // check for duplicates
            if (expressionList.Distinct().Count() != expressionList.Count)
            {
                expressionList = expressionList.Distinct().ToList();
                steps.Add(new ConversionStep(ToString(), "Idempotence"));
            }

            // to show proper ordering among disjunctions
            expressionList.Sort();
            if(reverseOrder)
            {
                expressionList.Reverse();
            }
            steps.Add(new ConversionStep(ToString(), "Commutation"));

        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < expressionList.Count; i++)
            {
                var group = expressionList[i];
                sb.Append(group.ToString());
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

            if (node.Op != OperatorType.OR)
            {
                var group = new DNFConjunctionGroup();
                buildNode(node, group);
                //group.Sort();
                expressionList.Add(group);
                return;
            }

            buildListFromTree(node.Left);
            buildListFromTree(node.Right);
            
        }

        private void buildNode(BoolExpr node, IList<DNFLiteral> group)
        {
            if (node == null)
                return;

            if(node.Op == OperatorType.LEAF)
            {
                group.Add(new DNFLiteral() { Name = node.Lit, isNegated = false, priority = variables.IndexOf(node.Lit) });
                return;
            }

            if (node.Op == OperatorType.NOT)
            {
                group.Add(new DNFLiteral() { Name = node.Right.Lit, isNegated = true, priority = variables.IndexOf(node.Right.Lit) });
                return;
            }

            buildNode(node.Left, group);
            buildNode(node.Right, group);
        }
    }
}
