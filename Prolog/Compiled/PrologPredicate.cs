using System;
using System.Collections.Generic;

namespace Prolog.Compiled
{
    [Serializable]
    public class PrologPredicate : Predicate
    {
        public IEnumerable<Clause> Clauses { get; set; }

        public override T Accept<T>(IPredicateVisitor<T> visitor)
        {
            return visitor.Visit (this);
        }
    }
}