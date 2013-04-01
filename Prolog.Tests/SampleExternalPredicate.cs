using System;
using System.Collections.Generic;
using System.Linq;
using Prolog.Runtime;

namespace Prolog.Tests
{
    /// <summary>
    /// An external predicate that returns values passed to it in the constructor.
    /// </summary>
    class SampleExternalPredicate 
    {
        private readonly string [] values;

        public SampleExternalPredicate (params string [] values)
        {
            this.values = values;
        }

        public IEnumerable <BoundVariableSet> BindVariables (IValue [] arguments)
        {
            foreach (var value in values)
            {
                var var = (Runtime.Variable) arguments.Single();

                if (var.ConcreteValue != null)
                {
                    // In real life may want to check here if the var is already bound to the correct value 
                    // but that is not needed for this test.
                    throw new Exception ("Variable already bound.");
                }

                var.BoundTo = new Runtime.Atom (value);

                yield return new BoundVariableSet {var};
            }
        }
    }
}