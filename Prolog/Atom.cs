using System;

namespace Prolog
{
    [Serializable]
    public class Atom : IArgument
    {
        public Atom () {}

        public Atom (string name)
        {
            Name = name;
        }

        public string Name {get; set;}

        T IArgument.Accept<T>(IArgumentVisitor<T> visitor)
        {
            return visitor.Visit (this);
        }
    }
}