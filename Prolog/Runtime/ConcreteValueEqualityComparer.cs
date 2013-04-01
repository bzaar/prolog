namespace Prolog.Runtime
{
    class ConcreteValueEqualityComparer : IConcreteValueVisitor<bool>
    {
        private readonly IConcreteValue a1;

        public ConcreteValueEqualityComparer(IConcreteValue a1)
        {
            this.a1 = a1;
        }

        bool IConcreteValueVisitor<bool>.Visit(Atom rhs)
        {
            var lhs = a1 as Atom;

            return lhs != null && lhs.Name == rhs.Name;
        }

        bool IConcreteValueVisitor<bool>.Visit(List rhs)
        {
            var lhs = a1 as List;

            return lhs != null && AreEqual (lhs, rhs);
        }

// ReSharper disable ParameterTypeCanBeEnumerable.Local
        private static bool AreEqual(List lhs, List rhs)
// ReSharper restore ParameterTypeCanBeEnumerable.Local
        {
            var e1 = lhs.GetEnumerator ();
            var e2 = rhs.GetEnumerator ();

            while (true)
            {
                bool more1 = e1.MoveNext ();
                bool more2 = e2.MoveNext ();

                if (more1 && more2)
                {
                    if (!EngineInternals.AreEqual (e1.Current, e2.Current))
                    {
                        return false;
                    }
                }
                else
                {
                    return more1 == more2; // lists are of same length
                }
            }
        }
    }
}
