using System.Collections.Generic;

namespace Prolog
{
    public delegate IEnumerable <Runtime.BoundVariableSet> ExternalPredicateDefinition (Runtime.IValue [] arguments);

    public class ExternalPredicateDeclaration
    {
        public ExternalPredicateDeclaration (string name, int arity)
        {
            Name = name;
            Arity = arity;
        }

        public string Name {get; private set;}

        public int Arity {get; private set;}
    }
}