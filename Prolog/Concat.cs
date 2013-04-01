using System;
using System.Collections.Generic;
using Prolog.Runtime;

namespace Prolog
{
    public static class Concat 
    {
        private static IEnumerable <BoundVariableSet> BindVariables (IValue [] arguments)
        {
            var list1 = GetList (arguments [0]);
            var list2 = GetList (arguments [1]);
            var list3 = GetList (arguments [2]);

            if (list1 != null && list3 != null)
            {
                string s1 = ((IValue) list1).Accept (new ArgumentPrinter ());
                string s3 = ((IValue) list3).Accept (new ArgumentPrinter ());

                string s = s1 + s3;

                var variables = new BoundVariableSet ();

                if (list3.IsAtLeastAsLongAs (list1) && variables.ZipUnify (list1, list3))
                {
                    var restOfList3 = list3.SkipLengthOf (list1);

                    if (list2 == null)
                    {
                        var arg2 = (Runtime.Variable) arguments [1];

                        variables.Bind (arg2, restOfList3);

                        yield return variables;
                    }
                    else
                    {
                        if (variables.Unify (list2, restOfList3))
                        {
                            yield return variables;
                        }
                    }
                }
            }
            else
            {
                throw new Exception ("IO pattern not supported.");
            }
        }

        private static Runtime.List GetList (IValue value)
        {
            // Must be a List, otherwise throws.
            return (Runtime.List) value.ConcreteValue;
        }

        public static KeyValuePair <ExternalPredicateDeclaration, ExternalPredicateDefinition> GetConcat ()
        {
            return new KeyValuePair <ExternalPredicateDeclaration, ExternalPredicateDefinition> (new ExternalPredicateDeclaration ("concat", 3), BindVariables);
        }
    }
}