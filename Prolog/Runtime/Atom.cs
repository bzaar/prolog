namespace Prolog.Runtime
{
    public class Atom : IConcreteValue
    {
        public Atom (string name)
        {
            this.Name = name;
        }

        public string Name {get; private set; }

        T IValue.Accept<T> (IValueVisitor<T> visitor)
        {
            return visitor.Visit (this);
        }

        IConcreteValue IValue.ConcreteValue
        {
            get { return this; }
        }

        T IConcreteValue.Accept<T>(IConcreteValueVisitor<T> visitor)
        {
            return visitor.Visit (this);
        }

        bool IValue.Accept(IValueUnifier visitor)
        {
            return visitor.Visit (this);
        }
    }
}