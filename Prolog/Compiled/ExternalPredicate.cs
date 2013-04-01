using System;

namespace Prolog.Compiled
{
    [Serializable]
    public class ExternalPredicate : Predicate
    {
        public ExternalPredicate (string name)
        {
            this.Name = name;
        }

        [NonSerialized]
        private ExternalPredicateDefinition callback;

        public ExternalPredicateDefinition Callback
        {
            get { return callback; }
            set { callback = value; }
        }

        public override T Accept<T>(IPredicateVisitor<T> visitor)
        {
            return visitor.Visit (this);
        }
    }
}