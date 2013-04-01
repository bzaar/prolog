using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Prolog.Runtime
{
    public class BoundVariableSet : IEnumerable <Variable>
    {
        private readonly List <Variable> boundVariables = new List <Variable> ();

        public void Release ()
        {
            boundVariables.ForEach(v => v.BoundTo = null);
            boundVariables.Clear();
        }

        public IEnumerator <Variable> GetEnumerator()
        {
            return boundVariables.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Variable var)
        {
            boundVariables.Add(var);
        }

        public bool Unify(IValue lhs, IValue rhs)
        {
            bool success = lhs.Accept (new ValueUnifier(rhs, this));

            if (!success)
            {
                Release ();
            }

            return success;
        }

        /// <summary>
        /// Unifies two enumerables element-wise.
        /// </summary>
        /// <remarks>
        /// The two collections can be of different lengths.
        /// The shortest collection defines the number of elements getting unified.
        /// If one collection is empty, unification will succeed; if this is not desired, 
        /// lengths of collections must be checked before calling <see cref="ZipUnify"/>.
        /// </remarks>
        public bool ZipUnify (IEnumerable <IValue> hiList, IEnumerable <IValue> loList)
        {
            return hiList.Zip (loList, Unify).All (x => x);
        }

        public void Bind (Variable variable, IValue value)
        {
            variable.BoundTo = value;

            Add (variable);
        }
    }

    public interface IValueUnifier
    {
        bool Visit (IConcreteValue lhsConcreteValue);
        bool Visit (Variable lhsVariable);
    }

    class ValueUnifier : IValueUnifier
    {
        readonly BoundVariableSet boundVariables;
        readonly IValue rhsValue;

        public ValueUnifier (IValue rhsValue, BoundVariableSet boundVariables)
        {
            this.boundVariables = boundVariables;
            this.rhsValue = rhsValue;
        }

        public bool Visit (IConcreteValue lhsConcreteValue)
        {
            return rhsValue.Accept (new LhsConcreteValueUnifier (lhsConcreteValue, boundVariables));
        }

        public bool Visit (Variable lhsVariable)
        {
            if (lhsVariable.ConcreteValue == null)
            {
                lhsVariable.BoundTo = rhsValue;

                boundVariables.Add (lhsVariable);

                return true;
            }

            return rhsValue.Accept (new LhsConcreteValueUnifier (lhsVariable.ConcreteValue, boundVariables));
        }
    }

    class LhsConcreteValueUnifier : IValueUnifier
    {
        private readonly IConcreteValue lhsConcreteValue;
        private readonly BoundVariableSet boundVariables;

        public LhsConcreteValueUnifier(IConcreteValue lhsConcreteValue, BoundVariableSet boundVariables)
        {
            this.lhsConcreteValue = lhsConcreteValue;
            this.boundVariables = boundVariables;
        }

        public bool Visit (IConcreteValue rhsConcreteValue)
        {
            return lhsConcreteValue.Accept (new Unifier (rhsConcreteValue, boundVariables));
        }

        public bool Visit (Variable rhsVariable)
        {
            if (rhsVariable.BoundTo == null)
            {
                rhsVariable.BoundTo = lhsConcreteValue;

                boundVariables.Add (rhsVariable);

                return true;
            }

            if (rhsVariable.ConcreteValue == null)
            {
                throw new Exception ();
            }

            return lhsConcreteValue.Accept (new Unifier (rhsVariable.ConcreteValue, boundVariables));
        }
    }
}