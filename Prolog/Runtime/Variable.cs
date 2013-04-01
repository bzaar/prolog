namespace Prolog.Runtime
{
    public class Variable : IValue
    {
        public Variable (string name)
        {
            this.Name = name;
        }

        private IValue boundTo;
        public IValue BoundTo
        {
            get { return boundTo; }
            set { boundTo = value; }
        }

        T IValue.Accept<T>(IValueVisitor<T> visitor)
        {
            return visitor.Visit (this);
        }

        public string Name {get; private set; }

        public IConcreteValue ConcreteValue
        {
            get { return BoundTo == null ? null : BoundTo.ConcreteValue; }
        }


        bool IValue.Accept(IValueUnifier visitor)
        {
            return visitor.Visit (this);
        }
    }
}