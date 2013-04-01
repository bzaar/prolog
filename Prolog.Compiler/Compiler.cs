using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Prolog
{
    public class Compiler
    {
        private readonly Parser parser = new Parser ();

        public Compiled.Program Compile (string fileName, params ExternalPredicateDeclaration [] externalPredicateDeclarations)
        {
            using (var file = new StreamReader (fileName))
            {
                return Compile (file, externalPredicateDeclarations);
            }
        }

// ReSharper disable MemberCanBePrivate.Global
        public Compiled.Program Compile (TextReader file, params ExternalPredicateDeclaration [] externalPredicateDeclarations)
// ReSharper restore MemberCanBePrivate.Global
        {
            var program = parser.ParseProgram (file);

            program.ExternalPredicates = externalPredicateDeclarations;

            return Compile (program);
        }

        public static IEnumerable <Runtime.ISolutionTreeNode> Solve (Runtime.Engine engine, AST.Goal [] goals, Compiled.Program program)
        {
            Compiled.Goal[] compiledGoals = Compile (goals, program.predicatesByName);

            return engine.Solve (compiledGoals);
        }

        private static AST.Clause DcgClauseToNormalClause (AST.DcgClause dcgClause)
        {
            var dcgGoalConverter = new AST.DcgGoalConverter ();

            var body = dcgClause.Body.Select (dcgGoalConverter.DcgGoalToNormalGoal).ToArray ();

            return new AST.Clause 
            {
                Head = new AST.Goal 
                {
                    PredicateName = dcgClause.Head.PredicateName, 
                    Arguments = new []
                                    {
                                        dcgClause.Head.Arguments,
                                        new [] // add 2 extra arguments, L0 and Ln
                                            {
                                                new Variable ("L0"),
                                                new Variable ("L" + dcgGoalConverter.DcgSubgoalCount)
                                            }
                                    }.SelectMany (a => a).ToArray ()
                },

                Body = body
            };
        }

        /// <summary>
        /// Compiles AST clauses into an executable program understood by the <see cref="Runtime.Engine"/> .
        /// The input clauses are not required to be in any particular order.
        /// </summary>
        public static Compiled.Program Compile (AST.Program program)
        {
            var allClauses = new []
                                 {
                                     program.DcgClauses.Select (DcgClauseToNormalClause),
                                     program.Clauses
                                 }.SelectMany (a => a);

            var clauseGroups = allClauses.GroupBy (c => Tuple.Create (c.Head.PredicateName, c.Head.Arguments.Length), c => c).ToArray ();

            var prologPredicates = clauseGroups.ToDictionary (p => p.Key, p => new Compiled.PrologPredicate {Name = p.Key.Item1});

            // Dictionary is not covariant so it takes a new dictionary to get a dictionary of base types (Predicate).
            var predicates = prologPredicates.ToDictionary (p => p.Key, CastPrologPredicateToBasePredicate); 

            if (program.ExternalPredicates != null)
            {
                foreach (var externalPredicate in program.ExternalPredicates)
                {
                    predicates.Add (Tuple.Create(externalPredicate.Name, externalPredicate.Arity), new Compiled.ExternalPredicate (externalPredicate.Name));
                }
            }

            foreach (var clauseGroup in clauseGroups)
            {
                var prologPredicate = prologPredicates [clauseGroup.Key];

                prologPredicate.Clauses = clauseGroup.Select (c => new Compiled.Clause {Head = new Compiled.Goal {Predicate = prologPredicate, Arguments = c.Head.Arguments},
                                                                                      Body = Compile (c.Body, predicates)}).ToArray ();
            }

            return new Compiled.Program (predicates);
        }

        private static Compiled.Predicate CastPrologPredicateToBasePredicate (KeyValuePair <Tuple <string, int>, Compiled.PrologPredicate> p)
        {
            return p.Value;
        }

// ReSharper disable ParameterTypeCanBeEnumerable.Local
        private static Compiled.Goal [] Compile (AST.Goal [] goals, IDictionary <Tuple <string, int>, Compiled.Predicate> predicatesByName)
// ReSharper restore ParameterTypeCanBeEnumerable.Local
        {
            return goals.Select (g => new Compiled.Goal {Predicate = predicatesByName[Tuple.Create (g.PredicateName, g.Arguments.Length)], Arguments = g.Arguments}).ToArray ();
        }
    }
}
