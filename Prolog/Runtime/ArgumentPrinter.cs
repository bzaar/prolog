using System.Linq;

namespace Prolog.Runtime
{
    public class ArgumentPrinter : IValueVisitor <string>
    {
        public string Visit (Atom atom)
        {
            return atom.Name;
        }

        public string Visit (Variable variable)
        {
            var value = variable.ConcreteValue;

            return variable.Name + ((value == null) ? "" : "=" + value.Accept (this));
        }

        public string Visit (List list)
        {
            return "[" + string.Join (", ", list.Select (Print)) + "]";
        }

        /// <summary>
        /// Convenience method.
        /// </summary>
        public string Print (IValue value)
        {
            return value.Accept (this);
        }
    }
}