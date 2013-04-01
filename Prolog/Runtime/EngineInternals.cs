using System.Collections.Generic;
using System.Linq;

namespace Prolog.Runtime
{
    public class EngineInternals
    {
        Frame frame;

        public IEnumerable <IDebugEvent> Run (Compiled.Program program)
        {
            return Solve (program.GetStartupGoal ());
        }

        public IEnumerable <IDebugEvent> Solve (Compiled.Goal [] goalDefs)
        {
            // Instantiate the goals.
            var queryArgumentInstantiator = new ArgumentInstantiator();
            var goals = InstantiateGoals (goalDefs, queryArgumentInstantiator);

            frame = new Frame (goals, null, new BoundVariableSet (), queryArgumentInstantiator.Variables);

// ReSharper disable ConditionIsAlwaysTrueOrFalse
            while (frame != null)
// ReSharper restore ConditionIsAlwaysTrueOrFalse
            {
                if (frame.GoalsProven == frame.Goals.Length)
                {
                    // No more goals to prove or it's a fact.
                    System.Diagnostics.Debug.Assert (frame.Goals.All (g => g.CurrentFrame != null));

                    if (frame.Parent == null)
                    {
                        yield return new Solution (frame);

                        foreach (var e in GoToLastChoicePoint ()) 
                            yield return e;
                    }
                    else
                    {
                        // go on and prove the next parent goal.
                        frame = frame.Parent;
                        frame.GoalsProven++;
                    }

                    continue;
                }

                var goal = frame.Goals [frame.GoalsProven];

                // Check that the same goal is not on the stack to prevent infinite loops.
                if (AlreadyOnTheStack (goal))
                {
                    foreach (var e in Backtrack ()) 
                        yield return e;
                }
                else
                {
                    var newFrame = goal.GetNextFrame ();

                    if (newFrame == null)
                    {
                        foreach (var e in Backtrack ()) 
                            yield return e;
                    }
                    else
                    {
                        yield return new Enter (newFrame);

                        frame = newFrame; // decend the tree
                    }
                }
            }
        }

        IEnumerable <IDebugEvent> Backtrack ()
        {
            // Current frame's current goal does not unify with any (more) clauses.
            if (frame.GoalsProven == 0)
            {
                if (frame.Parent != null)
                    yield return new Leave (frame);

                frame = frame.Parent; // ascend the tree; frame may become null at this point.
            }
            else
            {
                frame.CurrentGoal.Restart();

                foreach (var e in GoToLastChoicePoint ()) 
                    yield return e;
            }
        }

        IEnumerable <IDebugEvent> GoToLastChoicePoint ()
        {
            while (frame.Goals.Length > 0)
            {
                frame.GoalsProven--;

                var lastProvenGoal = frame.CurrentGoal;

                frame = lastProvenGoal.CurrentFrame;
            }

            yield return new Leave (frame);

            frame = frame.Parent; // ascend the tree; frame CANNOT become null at this point.
        }

        static bool AlreadyOnTheStack (Goal goal)
        {
            int count = goal.Frame.ParentFrames
                .Where  (f => f.GoalsProven < f.Goals.Length)
                .Select (f => f.Goals [f.GoalsProven])
                .Count  (g => AreEqual (g, goal));

            bool alreadyOnTheStack = count >  0;

            if (alreadyOnTheStack)
            {
            }

            return alreadyOnTheStack;
        }

        static bool IsEmptyDcgGoal (Goal goal)
        {
            var arguments = goal.Arguments;

            return arguments.Length >= 2 
                && arguments [arguments.Length-1] is List 
                && arguments [arguments.Length-2] is List;
        }

        static bool AreEqual (Goal goal1, Goal goal2)
        {
            return goal1.Definition.Predicate == goal2.Definition.Predicate
                   && goal1.Arguments.Zip (goal2.Arguments, AreEqual).All(x => x);
        }

        internal static bool AreEqual (IValue a1, IValue a2)
        {
            return (a1.ConcreteValue == null == (a2.ConcreteValue == null))
                   && ((a2.ConcreteValue == null) || a2.ConcreteValue.Accept (new ConcreteValueEqualityComparer (a1.ConcreteValue)));
        }

// ReSharper disable ParameterTypeCanBeEnumerable.Global
        internal static Goal [] InstantiateGoals (Compiled.Goal [] goalDefs, ArgumentInstantiator argumentInstantiator)
// ReSharper restore ParameterTypeCanBeEnumerable.Global
        {
            return goalDefs.Select (goalDef => CreateGoal (argumentInstantiator, goalDef)).ToArray ();
        }

        private static Goal CreateGoal (ArgumentInstantiator visitor, Compiled.Goal goalDef)
        {
            var arguments = goalDef.Arguments.Select(a => a.Accept(visitor)).ToArray();

            var predicate = goalDef.Predicate;

            var goal = predicate.Accept (new GoalInstantiator ());

            goal.Arguments = arguments;
            goal.Definition = goalDef;

            goal.Restart ();

            return goal;
        }
    }
}