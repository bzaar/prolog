using System.Linq;

namespace Prolog.AST
{
    class DcgGoalConverter : IDcgGoalVisitor<Goal>
    {
        int i;

        public int DcgSubgoalCount {get {return i;}}

        public Goal DcgGoalToNormalGoal (IDcgGoal g)
        {
            return g.Accept (this);
        }

        Goal IDcgGoalVisitor<Goal>.Visit (DcgLiteral literal)
        {
            return new Goal
            {
                PredicateName = "concat",
                Arguments = new IArgument []
                {
                    new List (literal.Value),
                    new Variable ("L" + (++i)),
                    new Variable ("L" + (i - 1))
                }
            };
        }

        // argList --> arg.
        // argList (L0, L1) :- arg (L0, L1).
        Goal IDcgGoalVisitor<Goal>.Visit (DcgSubgoal goal)
        {
            return new Goal
            {
                PredicateName = goal.PredicateName,
                Arguments = new []
                {
                    goal.Arguments,
                    
                    new IArgument []
                    {
                        new Variable ("L" + i),
                        new Variable ("L" + (++i))
                    }
                }
                .SelectMany (a => a).ToArray ()
            };
        }

        Goal IDcgGoalVisitor<Goal>.Visit (DcgNonDcgGoal goal)
        {
            return goal;
        }
    }
}