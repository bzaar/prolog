namespace Prolog.Compiled
{
// ReSharper disable TypeParameterCanBeVariant
    public interface IPredicateVisitor <T>
// ReSharper restore TypeParameterCanBeVariant
    {
        T Visit (PrologPredicate predicate);
        T Visit (ExternalPredicate predicate);
    }
}