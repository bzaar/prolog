namespace Prolog.Runtime
{
    class GoalInstantiator : Compiled.IPredicateVisitor<Goal>
    {
        Goal Compiled.IPredicateVisitor<Goal>.Visit (Compiled.PrologPredicate predicate)
        {
            return new PrologGoal (predicate);
        }

        Goal Compiled.IPredicateVisitor<Goal>.Visit (Compiled.ExternalPredicate predicate)
        {
            return new ExternalGoal (predicate.Callback);
        }
    }
}