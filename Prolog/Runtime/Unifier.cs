namespace Prolog.Runtime
{
    class Unifier : IConcreteValueVisitor <bool>
    {
        private readonly IConcreteValue lhs;
        private readonly BoundVariableSet boundVariables;

        public Unifier (IConcreteValue lhs, BoundVariableSet boundVariables)
        {
            this.lhs = lhs;
            this.boundVariables = boundVariables;
        }

        bool IConcreteValueVisitor <bool>.Visit(Atom rhs)
        {
            return lhs.Accept (new AtomUnifier(rhs));
        }

        bool IConcreteValueVisitor <bool>.Visit(List rhs)
        {
            return lhs.Accept (new ListUnifier(rhs, boundVariables));
        }
    }

    class AtomUnifier : IConcreteValueVisitor <bool>
    {
        private readonly Atom rhs;

        public AtomUnifier(Atom rhs)
        {
            this.rhs = rhs;
        }

        bool IConcreteValueVisitor <bool>.Visit(Atom lhs)
        {
            return lhs.Name == rhs.Name;
        }

        bool IConcreteValueVisitor <bool>.Visit(List list)
        {
            return false;
        }
    }

    class ListUnifier : IConcreteValueVisitor <bool>
    {
        private readonly List rhsList;
        private readonly BoundVariableSet boundVariables;

        public ListUnifier(List rhsList, BoundVariableSet boundVariables)
        {
            this.rhsList = rhsList;
            this.boundVariables = boundVariables;
        }

        bool IConcreteValueVisitor <bool>.Visit(Atom atom)
        {
            return false;
        }

        bool IConcreteValueVisitor <bool>.Visit(List lhsList)
        {
            return lhsList.IsSameLength (rhsList) && boundVariables.ZipUnify (rhsList, lhsList);
        }
    }
}