using System;

namespace Prolog.Compiled
{
    [Serializable]
    public class Clause
    {
        public Goal Head {get; set; }

        public Goal [] Body {get; set;}
    }
}