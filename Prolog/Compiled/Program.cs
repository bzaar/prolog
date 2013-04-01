using System;
using System.Collections.Generic;

namespace Prolog.Compiled
{
    [Serializable]
    public class Program
    {
        public readonly Dictionary <Tuple <string, int>, Predicate> predicatesByName;

        public Program (Dictionary <Tuple <string, int>, Predicate> predicatesByName)
        {
            this.predicatesByName = predicatesByName;
        }

        public void SetExternalPredicateCallbacks (IEnumerable <KeyValuePair <ExternalPredicateDeclaration, ExternalPredicateDefinition>> externalPredicates)
        {
            foreach (var astPredicate in externalPredicates)
            {
                var runtimePredicate = (ExternalPredicate) predicatesByName [Tuple.Create (astPredicate.Key.Name, astPredicate.Key.Arity)];

                runtimePredicate.Callback = astPredicate.Value;
            }
        }

        public Goal [] GetStartupGoal ()
        {
            return new [] 
                       {
                           new Goal
                               {
                                   Predicate = predicatesByName [Tuple.Create ("run", 0)],

                                   Arguments = new IArgument [0]
                               }
                       };
        }
    }
}