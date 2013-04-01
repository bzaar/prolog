using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Prolog.Runtime
{
    public class List : IEnumerable <IValue>, IConcreteValue
    {
        private readonly IValue [] elements;

        public List (IValue [] elements)
        {
            this.elements = elements;
        }

        T IValue.Accept<T>(IValueVisitor <T> visitor)
        {
            return visitor.Visit (this);
        }

        public IEnumerator <IValue> GetEnumerator()
        {
            return elements.Select(x => x).GetEnumerator ();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator ();
        }

        public bool IsAtLeastAsLongAs (List list1)
        {
            return elements.Length >= list1.elements.Length;
        }

        public List SkipLengthOf (List other)
        {
            return new List (this.Skip (other.elements.Length).ToArray ());
        }

        public bool IsSameLength (List list)
        {
            return elements.Length == list.elements.Length;
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