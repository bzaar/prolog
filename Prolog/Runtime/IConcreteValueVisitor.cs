namespace Prolog.Runtime
{
// ReSharper disable TypeParameterCanBeVariant
    public interface IConcreteValueVisitor <T>
// ReSharper restore TypeParameterCanBeVariant
    {
        T Visit (Atom atom);
        T Visit (List list);
    }
}