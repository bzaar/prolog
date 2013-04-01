using System.Collections.Generic;
using System.Linq;

namespace Prolog.Runtime
{
    class ExternalGoal : Goal
    {
        private readonly ExternalPredicateDefinition impl;

        public ExternalGoal (ExternalPredicateDefinition impl)
        {
            this.impl = impl;
        }

        protected override IEnumerable<Frame> GetFrames()
        {
            return impl (Arguments).Select (
                boundVariables => new Frame (new Goal [0], this, boundVariables, new Dictionary <string, Variable> ()));
        }
    }
}