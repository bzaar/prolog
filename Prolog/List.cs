using System;
using System.Collections.Generic;

namespace Prolog
{
    [Serializable]
    public class List : IArgument
    {
        private readonly IArgument [] elements;

        public List (params IArgument [] elements)
        {
            this.elements = elements;
        }

        public IEnumerable <IArgument> Elements
        {
            get { return elements; }
        }

        T IArgument.Accept<T>(IArgumentVisitor<T> visitor)
        {
            return visitor.Visit (this);
        }
    }
}