using System;

namespace Prolog.Compiled
{
    [Serializable]
    public abstract class Predicate
    {
        public string Name;

        public abstract T Accept <T> (IPredicateVisitor<T> visitor);
    }
}