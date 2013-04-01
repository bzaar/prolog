namespace Prolog.Runtime
{
    public interface IConcreteValue : IValue
    {
        T Accept<T> (IConcreteValueVisitor<T> visitor);
    }
}