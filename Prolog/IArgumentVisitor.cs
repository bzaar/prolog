namespace Prolog
{
    public interface IArgumentVisitor<T>
// ReSharper restore TypeParameterCanBeVariant
    {
        T Visit (Atom atom);
        T Visit (Variable variable);
        T Visit (List list);
    }
}