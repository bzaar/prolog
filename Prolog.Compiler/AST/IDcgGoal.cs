namespace Prolog.AST
{
    public interface IDcgGoal
    {
        T Accept <T> (IDcgGoalVisitor <T> visitor);
    }

    public interface IDcgGoalVisitor <T>
    {
        T Visit (DcgLiteral literal);
        T Visit (DcgSubgoal goal);
        T Visit (DcgNonDcgGoal goal);
    }

    public class DcgLiteral : IDcgGoal
    {
        public IArgument Value {get; set;}

        T IDcgGoal.Accept<T>(IDcgGoalVisitor<T> visitor)
        {
            return visitor.Visit (this);
        }
    }

    public class DcgSubgoal : Goal, IDcgGoal
    {
        T IDcgGoal.Accept<T>(IDcgGoalVisitor<T> visitor)
        {
            return visitor.Visit (this);
        }
    }

    /// <summary>
    /// A normal (non-DCG) predicate goal embedded into a DCG clause.
    /// </summary>
    public class DcgNonDcgGoal : Goal, IDcgGoal
    {
        T IDcgGoal.Accept<T>(IDcgGoalVisitor<T> visitor)
        {
            return visitor.Visit (this);
        }
    }
}
