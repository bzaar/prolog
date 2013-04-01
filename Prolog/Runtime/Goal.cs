using System.Collections.Generic;
using System.Text;

namespace Prolog.Runtime
{
    public abstract class Goal 
    {
        internal Frame GetNextFrame ()
        {
            if (CurrentFrame != null)
            {
                CurrentFrame.ReleaseVariables();
            }

            return frameEnumerator.MoveNext () ? CurrentFrame : null;
        }

        internal  Frame CurrentFrame
        {
            get { return frameEnumerator.Current; }
        }

        /// <summary>
        /// The frame that contains this goal and other sibling goals.
        /// </summary>
        public Frame Frame {get; set;}

        public Compiled.Goal Definition {get; set;}
        public IValue [] Arguments {get; set;}

        public int Level
        {
            get {
                return this.Frame.Level;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            SolutionTreePrinter.Print(this, sb);
            return sb.ToString ();
        }

        IEnumerator <Frame> frameEnumerator;

        internal void Restart()
        {
            if (frameEnumerator != null && CurrentFrame != null)
            {
                CurrentFrame.ReleaseVariables();
            }

            frameEnumerator = GetFrames ().GetEnumerator ();
        }

        protected abstract IEnumerable<Frame> GetFrames();
    }
}