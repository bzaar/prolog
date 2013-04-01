using System.Linq;
using System.Collections.Generic;

namespace Prolog.Runtime
{
    class ArgumentInstantiator : IArgumentVisitor <IValue>
    {
        private readonly Dictionary<string, Variable> variables = new Dictionary <string, Variable>();

        public Dictionary <string, Variable> Variables
        {
            get { return variables; }
        }

        IValue IArgumentVisitor<IValue>.Visit(Prolog.Atom atom)
        {
            return new Atom (atom.Name);
        }

        IValue IArgumentVisitor<IValue>.Visit(Prolog.Variable variableDef)
        {
            Variable variable;
            if (!variables.TryGetValue (variableDef.Name, out variable))
            {
                variable = new Variable (variableDef.Name);

                variables.Add (variableDef.Name, variable);
            }

            return variable;
        }

        IValue IArgumentVisitor<IValue>.Visit(Prolog.List list)
        {
            return new List (list.Elements.Select (e => e.Accept(this)).ToArray());
        }
    }
}