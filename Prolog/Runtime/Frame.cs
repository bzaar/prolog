using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prolog.Runtime
{
    public class Frame : ISolutionTreeNode
    {
        private readonly Goal [] goals;
        private BoundVariableSet boundVariables;
        private readonly Variables variables;

        public Frame (Goal [] goals, Goal parent, BoundVariableSet boundVariables, Dictionary <string, Variable> variables)
        {
            this.goals = goals;
            this.boundVariables = boundVariables;
            this.HeadGoal = parent;

            this.variables = new Variables(variables);

            foreach (var goal in goals)
            {
                goal.Frame = this;
            }
        }

        public Goal CurrentGoal
        {
            get { return this.GoalsProven < this.goals.Length ? this.goals [this.GoalsProven] : null; }
        }

        public int GoalsProven;

        public Frame Parent
        {
            get { return HeadGoal == null ? null : HeadGoal.Frame; }
        }

        public IEnumerable <Frame> ParentFrames
        {
            get
            {
                var frame = Parent;

                while (frame != null)
                {
                    yield return frame;

                    frame = frame.Parent;
                }
            }
        }

        public Goal HeadGoal
        {
            get; set;
        }

        public Goal [] Goals
        {
            get { return goals; }
        }

        public void ReleaseVariables()
        {
            boundVariables.Release ();
            boundVariables = null; // to prevent ReleaseVariables from being called twice.
        }

        public int Level
        {
            get { return ParentFrames.Count (); }
        }

        Variables ISolutionTreeNode.Variables
        {
            get { return variables; }
        }

        ISolutionTreeNode ISolutionTreeNode.this [string goalName]
        {
            get
            {
                var matches = goals.OfType<PrologGoal>().Where(g => g.Predicate.Name == goalName).ToArray();

                switch (matches.Length)
                {
                    case 1: return matches.Single().CurrentFrame;
                    case 0: return null;
                }

                throw new Exception();
            }
        }

        public IEnumerator<ISolutionTreeNode> GetEnumerator()
        {
            return this.goals.Select(g => g.CurrentFrame).GetEnumerator ();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator ();
        }

        public override string ToString()
        {
            var sb = new StringBuilder ();

            SolutionTreePrinter.Print (sb, this);

            return sb.ToString ();
        }
    }
}