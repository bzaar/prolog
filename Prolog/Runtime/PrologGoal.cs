using System.Collections.Generic;
using System.Linq;

namespace Prolog.Runtime
{
    class PrologGoal : Goal
    {
        private readonly Compiled.PrologPredicate predicate;

        public PrologGoal (Compiled.PrologPredicate predicate)
        {
            this.predicate = predicate;
        }

        protected override IEnumerable <Frame> GetFrames ()
        {
            return this.Predicate.Clauses.Select (Unify).Where (f => f != null);
        }

        Frame Unify (Compiled.Clause clause)
        {
            var boundVariables = new BoundVariableSet ();
            var argumentInstantiator = new ArgumentInstantiator ();

            var clauseHeadArguments = clause.Head.Arguments.Select (a => a.Accept(argumentInstantiator)).ToArray();

            if (boundVariables.ZipUnify (Arguments, clauseHeadArguments))
            {
                Goal [] goals = EngineInternals.InstantiateGoals (clause.Body, argumentInstantiator);

                return new Frame (goals, this, boundVariables, argumentInstantiator.Variables);
            }

            return null;
        }

        public Compiled.PrologPredicate Predicate
        {
            get { return predicate; }
        }
    }
}