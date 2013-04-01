using System;
using System.Collections.Generic;
using System.Linq;

namespace Prolog.Tests
{
    public static class Helper
    {
        public static IEnumerable <Runtime.ISolutionTreeNode> Solve (AST.Goal goal, Compiled.Program program)
        {
            return Compiler.Solve (new Runtime.Engine (), new [] {goal}, program);
        }

        static IArgument GetArgument (string name)
        {
            name = name.Trim();

            if (name.StartsWith ("["))
            {
                name = name.Substring (1, name.Length-2);

                var elems = String.IsNullOrWhiteSpace (name) ? new string [0] : name.Split(',');

                return new List (elems.Select(GetArgument).ToArray());
            }

            return name.StartsWith ("_") || Char.IsUpper (name [0]) ? (IArgument) new Variable {Name = name} : new Atom {Name = name};
        }

        public static AST.Clause MakeFact (string predicate, params string [] args)
        {
            return new AST.Clause {Head = MakeGoal (predicate, args), Body = new AST.Goal [0]};
        }

        public static AST.Goal MakeGoal (string predicate, params string [] args)
        {
            return new AST.Goal
                       {
                           PredicateName = predicate,

                           Arguments = args.Select (GetArgument).ToArray()
                       };
        }

        public static IEnumerable <KeyValuePair <ExternalPredicateDeclaration, ExternalPredicateDefinition>> GetConcat ()
        {
            return new [] {Concat.GetConcat ()};
        }

        public static ExternalPredicateDeclaration [] GetConcatDeclaration ()
        {
            return GetConcat ().Select (kvp => kvp.Key).ToArray ();
        }

        public static IEnumerable <Dictionary <string, Runtime.IConcreteValue>> GetSolutions (AST.Goal goal, Compiled.Program program)
        {
            var solutions = Solve (goal, program);

            return solutions.Select(GetTopLevelVariables);
        }

        public static Dictionary <string, Runtime.IConcreteValue> GetTopLevelVariables (Runtime.ISolutionTreeNode s)
        {
            return s.Variables.ToDictionary(v => v.Key, v => v.Value);
        }

        public static IEnumerable <Runtime.ISolutionTreeNode> Solve (Compiled.Program program, out Tracer tracer, params AST.Goal [] goals)
        {
            var engine = new Runtime.Engine ();

            tracer = new Tracer();
            //engine.NewGoal += tracer.Engine_NewGoal;
            //engine.Unified += tracer.Engine_Unified;
            //engine.Failed  += tracer.Engine_Failed;

            return Compiler.Solve (engine, goals, program);
        }

    }
}