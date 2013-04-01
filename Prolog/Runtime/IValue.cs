namespace Prolog.Runtime
{
    public interface IValue
    {
        T Accept <T> (IValueVisitor<T> visitor);

        bool Accept (IValueUnifier visitor);


        IConcreteValue ConcreteValue {get;}
    }
}