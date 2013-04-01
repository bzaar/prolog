using System;

namespace Prolog
{
    [Serializable]
    public class Variable : IArgument
    {
        public Variable () {}

        public Variable (string name)
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