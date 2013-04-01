namespace Prolog
{
    public interface IArgument
    {
        T Accept <T> (IArgumentVisitor<T> visitor);
    }
}