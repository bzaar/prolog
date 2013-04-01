using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Prolog.Runtime
{
    public class Variables : IEnumerable <KeyValuePair <string, IConcreteValue>>
    {
        private readonly Dictionary <string, Variable> variables;

        internal Variables (Dictionary <string, Variable> variables)
        {
            this.variables = variables;
        }

        public IConcreteValue this [string variableName]
        {
            get
            {
                Variable variable;

                return variables.TryGetValue (variableName, out variable) ? variable.ConcreteValue : null;
            }
        }

        public bool Contains (string variableName)
        {
            return this.variables.ContainsKey (variableName);
        }

        public IEnumerator <KeyValuePair <string, IConcreteValue>> GetEnumerator()
        {
            return variables.ToDictionary(
                variable => variable.Key, 
                variable => variable.Value.ConcreteValue)
                .GetEnumerator ();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get {return this.variables.Count;}
        }
    }
}