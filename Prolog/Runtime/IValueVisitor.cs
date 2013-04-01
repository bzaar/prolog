namespace Prolog.Runtime
{
    public interface IValueVisitor <T> : IConcreteValueVisitor <T>
    {
        T Visit (Variable variable);
    }
}