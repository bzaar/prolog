using System.Linq;

namespace Prolog.Runtime
{
    public class SolutionTreePrinter
    {
        public delegate void NodePrinter (System.Text.StringBuilder sb, ISolutionTreeNode node);

        private readonly NodePrinter nodePrinter;
        private readonly System.Text.StringBuilder sb = new System.Text.StringBuilder ();

        private SolutionTreePrinter ()
        {
            nodePrinter = PrintNode;
        }

        public SolutionTreePrinter (NodePrinter nodePrinter)
        {
            this.nodePrinter = nodePrinter;
        }

        public static string SolutionTreeToString (ISolutionTreeNode solution)
        {
            return new SolutionTreePrinter ().Print (solution);
        }

        public string Print (ISolutionTreeNode solution)
        {
            SolutionTreeToString (solution, 0);

            return sb.ToString ();
        }

// ReSharper disable ParameterTypeCanBeEnumerable.Local
        private void SolutionTreeToString (ISolutionTreeNode solution, int level)
// ReSharper restore ParameterTypeCanBeEnumerable.Local
        {
            foreach (var node in solution)
            {
                string s = NodeToString (node);

                if (!string.IsNullOrWhiteSpace (s))
                {
                    sb.Append (new string (' ', level * 4));

                    sb.AppendLine (s);
                }

                SolutionTreeToString (node, level + 1);
            }
        }

        string NodeToString (ISolutionTreeNode node)
        {
            var stringBuilder = new System.Text.StringBuilder ();

            nodePrinter (stringBuilder, node);

            return stringBuilder.ToString ();
        }

        static void PrintNode (System.Text.StringBuilder stringBuilder, ISolutionTreeNode node)
        {
            Print (stringBuilder, node);
        }

        public static void PrintDcgNode (System.Text.StringBuilder stringBuilder, ISolutionTreeNode node)
        {
            Goal goal = node.HeadGoal;
            string predicate = goal.Definition.Predicate.Name;
            if (predicate != "concat")
            {
                stringBuilder.Append (predicate);
                stringBuilder.Append ("(");
                stringBuilder.Append (string.Join(", ", goal.Arguments.Where(a => !(a.ConcreteValue is List)).Select (new ArgumentPrinter().Print)));
                stringBuilder.Append (")");
            }
        }

        public static void Print (System.Text.StringBuilder sb, ISolutionTreeNode frame)
        {
            Print (frame.HeadGoal, sb);
        }

        public static void Print (Goal goal, System.Text.StringBuilder sb)
        {
            sb.Append (goal.Definition.Predicate.Name);
            sb.Append ("(");
            sb.Append (string.Join(", ", goal.Arguments.Select (new ArgumentPrinter().Print)));
            sb.Append (")");
        }
    }
}
