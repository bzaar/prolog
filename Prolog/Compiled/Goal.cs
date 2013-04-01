using System;

namespace Prolog.Compiled
{
    [Serializable]
    public class Goal
    {
        public Predicate Predicate {get; set;}

        public IArgument [] Arguments {get; set;}
    }
}