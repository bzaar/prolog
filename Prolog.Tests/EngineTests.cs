using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Prolog.Tests
{
    [TestClass]
    public class EngineTests
    {
        [TestMethod]
        public void WhatDoesBillLike()
        {
            // likes (_, icecream).
            Compiled.Program program = EveryOneLikesIcecream ();

            // ?- likes (bill, X).
            var goal = new AST.Goal 
            {
                PredicateName = "likes",

                Arguments = new IArgument[]
                {
                    new Atom {Name = "bill"},
                    new Variable {Name = "X"}
                }
            };

            var solutions = Helper.GetSolutions (goal, program);

            var variables = solutions.Single();

            var variable = variables.Single();

            Assert.AreEqual ("X", variable.Key);
            Assert.AreEqual ("icecream", ((Runtime.Atom) variable.Value).Name);
        }

        [TestMethod]
        public void DoesBillLikeIcecream()
        {
            // likes (_, icecream).
            Compiled.Program program = EveryOneLikesIcecream ();

            // ?- likes (bill, icecream).
            var goal = new AST.Goal 
            {
                PredicateName = "likes",

                Arguments = new IArgument[]
                {
                    new Atom {Name = "bill"},
                    new Atom {Name = "icecream"}
                }
            };

            var solutions = Helper.Solve (goal, program);

            var solution = solutions.Single(); // a single solution means prolog says 'yes'

            Assert.IsFalse (solution.Variables.Any()); // no variables in query
        }

        private static Compiled.Program EveryOneLikesIcecream ()
        {
            return Compiler.Compile(
                new AST.Program (
                    new [] {
                        new AST.Clause {
                            Head = new AST.Goal 
                            {
                                PredicateName = "likes", 
                                Arguments = new IArgument[] {
                                                                new Variable {Name = "_"},
                                                                new Atom {Name = "icecream"}
                                                            }
                            },
                            Body = new AST.Goal [0]
                        }
            }));
        }

        [TestMethod]
        public void IsSpikeyAnAnimal()
        {
            var database = MakeDogsDatabase ();

            // ?- animal (spikey).
            var goal = Helper.MakeGoal ("animal", "spikey");

            Tracer tracer;
            var solutions = Helper.Solve (database, out tracer, goal);

            var solution = solutions.Single(); 

            Assert.IsFalse (solution.Variables.Any()); // no variables in query
        }

        [TestMethod]
        public void WhoIsADog()
        {
            var database = MakeDogsDatabase ();

            // ?- dog (X).
            var goal = Helper.MakeGoal ("dog", "X");

            var solutions = Helper.GetSolutions (goal, database);

            AssertWhoIsADogResults (solutions);
        }

        private static void AssertWhoIsADogResults (IEnumerable <Dictionary<string, Runtime.IConcreteValue>> solutions)
        {
            Assert.IsTrue("spikey,motty".Split(',').Zip (solutions,
                (expected, solution) => expected == ((Runtime.Atom) solution.Single().Value).Name)
                .All(x => x));
        }

        [TestMethod]
        public void EngineSolve_WhenCalledTwice_ReturnsSameResults()
        {
            var database = MakeDogsDatabase ();

            // ?- dog (X).
            var goal = Helper.MakeGoal ("dog", "X");

            for (int i = 0; i < 2; i++)
            {
                var solutions = Helper.GetSolutions (goal, database);

                AssertWhoIsADogResults (solutions);
            }
        }

        [TestMethod]
        public void Solutions_WhenIteratedTwice_ReturnsSameResults()
        {
            var database = MakeDogsDatabase ();

            // ?- dog (X).
            var goal = Helper.MakeGoal ("dog", "X");

            int timesStarted = 0;
            var engine = new Runtime.Engine ();
            engine.Start += delegate {++timesStarted;};

            var solutions = Compiler.Solve (engine, new [] {goal}, database);
            var variables = solutions.Select(s => s.Variables.ToDictionary(v => v.Key, v => v.Value));

            AssertWhoIsADogResults (variables);
            AssertWhoIsADogResults (variables);

            Assert.AreEqual (2, timesStarted);
        }

        [TestMethod]
        [Ignore] // Yields different results from run to run.
        public void EngineSolve_WhenIteratedOnce_DoesntLeak()
        {
            var database = MakeDogsDatabase ();

            // ?- dog (X).
            var goal = Helper.MakeGoal ("dog", "X");

            var engine = new Runtime.Engine ();

            using (new LeakDetector ())
            {
                var solutions = Compiler.Solve (engine, new [] {goal}, database);

                Assert.IsTrue (solutions.Any());
            }
        }

        class LeakDetector : IDisposable
        {
            private readonly long startBytes;

            public LeakDetector()
            {
                startBytes = GetMem ();
            }

            private static long GetMem ()
            {
                GC.Collect ();
                GC.WaitForPendingFinalizers ();
                GC.Collect ();
                GC.WaitForPendingFinalizers ();
                return GC.GetTotalMemory (true);
            }

            public void Dispose ()
            {
                long diff = GetMem () - startBytes;

                Assert.AreEqual (0, diff);
            }
        }

        [TestMethod]
        public void IsJohnAnAnimal()
        {
            var database = MakeDogsDatabase ();

            // ?- animal (john).
            var goal = new AST.Goal 
            {
                PredicateName = "animal",

                Arguments = new IArgument[]
                {
                    new Atom {Name = "john"}
                }
            };

            var solutions = Helper.Solve (goal, database);

            Assert.IsFalse (solutions.Any()); // prolog says 'don't know'.
        }

        [TestMethod]
        public void CanIGetFromMoscowToVladivostok()
        {
            var database = MakePathsDatabase();

            // ?- path (moscow, vladivostok).
            var goal = Helper.MakeGoal ("path", "moscow", "vladivostok");

            Tracer tracer;
            var solutions = Helper.Solve (database, out tracer, goal).ToArray();

            Assert.AreEqual (1, solutions.Length);

            Assert.IsFalse(solutions [0].Variables.Any());
        }

        [TestMethod]
        public void TraceUnifiedTest()
        {
            var database = MakePathsDatabase();

            // ?- path (moscow, vladivostok).
            var goal = Helper.MakeGoal ("path", "moscow", "novosib");

            var engine = new Runtime.Engine ();

            var tracer = new Tracer();
            engine.Unified += tracer.Engine_Unified;
            Compiler.Solve (engine, new [] {goal}, database).Any();

            Assert.AreEqual (@"Unified: path(moscow, novosib)
Unified: path(moscow, novosib)
Unified:     road(X=moscow, Z=ekat)
Unified:     path(Z=ekat, Y=novosib)
Unified:     path(Z=ekat, Y=novosib)
Unified:         road(X=ekat, Z=vladivostok)
Unified:         path(Z=vladivostok, Y=novosib)
Unified:         path(Z=vladivostok, Y=novosib)
", tracer.Trace);
        }

        [TestMethod]
        public void CanIGetFromMoscowToNovosib()
        {
            var database = MakePathsDatabase();

            // ?- path (moscow, novosib).
            var goal = Helper.MakeGoal ("path", "moscow", "novosib"); // novosib is not in the database.

            var solutions = Helper.Solve (goal, database);

            Assert.IsFalse (solutions.Any()); 
        }

        // animal (X) :- dog (X).
        // dog (spikey).
        // dog (motty).
        private static Compiled.Program MakeDogsDatabase ()
        {
            return Compiler.Compile(new AST.Program (new [] {
                                                                     new AST.Clause {
                                                                                        Head = new AST.Goal {
                                                                                                                PredicateName = "animal", 
                                                                                                                Arguments = new IArgument[] {
                                                                                                                                                new Variable {Name = "A"}
                                                                                                                                            }
                                                                                                            },
                                                                                        Body = new []
                                                                                                   {
                                                                                                       new AST.Goal {
                                                                                                                        PredicateName = "dog",
                                                                                                                        Arguments = new IArgument[] {
                                                                                                                                                        new Variable {Name = "A"}
                                                                                                                                                    }
                                                                                                                    }
                                                                                                   }
                                                                                    },
                                                                     new AST.Clause {
                                                                                        Head = new AST.Goal {
                                                                                                                PredicateName = "dog", 
                                                                                                                Arguments = new IArgument[] {
                                                                                                                                                new Atom {Name = "spikey"}
                                                                                                                                            }
                                                                                                            },
                                                                                        Body = new AST.Goal [0]
                                                                                    },
                                                                     new AST.Clause {
                                                                                        Head = new AST.Goal {
                                                                                                                PredicateName = "dog", 
                                                                                                                Arguments = new IArgument[] {
                                                                                                                                                new Atom {Name = "motty"}
                                                                                                                                            }
                                                                                                            },
                                                                                        Body = new AST.Goal [0]
                                                                                    },
                                                                 }));
        }

        // road (moscow, ekat).
        // road (ekat, vladivostok).
        // path (X,Y) :- road (X,Y).
        // path (X,Y) :- road (X,Z), path (Z,Y).
        private static Compiled.Program MakePathsDatabase ()
        {
            return Compiler.Compile(new AST.Program (new [] {
                                                                     Helper.MakeFact ("road", "moscow", "ekat"),      
                                                                     Helper.MakeFact ("road", "ekat", "vladivostok"),
                                                                     new AST.Clause {
                                                                                        Head = Helper.MakeGoal ("path", "X", "Y"),
                                                                                        Body = new [] {Helper.MakeGoal ("road", "X", "Y")}
                                                                                    },
                                                                     new AST.Clause {
                                                                                        Head = Helper.MakeGoal ("path", "X", "Y"),
                                                                                        Body = new [] {
                                                                                                          Helper.MakeGoal ("road", "X", "Z"),
                                                                                                          Helper.MakeGoal ("path", "Z", "Y"),
                                                                                                      }
                                                                                    },
                                                                 }));
        }

        [TestMethod]
        public void UnifyList()
        {
            Compiled.Program program = GetStarsDatabase ();

            var goal = Helper.MakeGoal ("stars", "Xs");

            var solutions = Helper.GetSolutions (goal, program);

            var list = (Runtime.List) solutions.Single().Single().Value;
            
            Assert.AreEqual ("sun",    ((Runtime.Atom) list.First ()).Name);
            Assert.AreEqual ("sirius", ((Runtime.Atom) list.Skip (1).First ()).Name);
        }

        [TestMethod]
        public void UnifyListElements()
        {
            Compiled.Program program = GetStarsDatabase ();

            var goal = Helper.MakeGoal ("stars", "[X,Y]");

            var solutions = Helper.GetSolutions (goal, program);

            var variables = solutions.Single();

            Assert.AreEqual ("sun",    ((Runtime.Atom) variables ["X"]).Name);
            Assert.AreEqual ("sirius", ((Runtime.Atom) variables ["Y"]).Name);
        }

        [TestMethod]
        public void ExternalPredicateTest()
        {
            var externalPredicate = new ExternalPredicateDeclaration ("externalPredicate", 1);

            Compiled.Program program = Compiler.Compile (new AST.Program (new AST.Clause []
                                                                                     {
                                                                                     }, new []
                                                                                            {
                                                                                                externalPredicate
                                                                                            }));


            program.SetExternalPredicateCallbacks (
                new []
                {
                    new KeyValuePair <ExternalPredicateDeclaration, ExternalPredicateDefinition> (externalPredicate, new SampleExternalPredicate("john").BindVariables)
                }
            );

            var goal = Helper.MakeGoal ("externalPredicate", "X");

            AssertExternalPredicateSolutionIsCorrect (goal, program);
        }

        [TestMethod]
        public void ExternalPredicateReturningMulpipleAnswersTest()
        {
            var externalPredicate = new ExternalPredicateDeclaration ("externalPredicate", 1);

            Compiled.Program program = Compiler.Compile (new AST.Program (new AST.Clause []
                                                                                     {
                                                                                     }, new []
                                                                                            {
                                                                                                externalPredicate
                                                                                            }));

            program.SetExternalPredicateCallbacks (
                new []
                {
                    new KeyValuePair <ExternalPredicateDeclaration, ExternalPredicateDefinition> (externalPredicate, new SampleExternalPredicate("john", "jane").BindVariables)
                }
            );

            var goal = Helper.MakeGoal ("externalPredicate", "X");

            var solutions = Helper.GetSolutions (goal, program).ToArray();

            Assert.AreEqual (2, solutions.Length);

            Assert.AreEqual ("john", ((Runtime.Atom) solutions [0] ["X"]).Name);
            Assert.AreEqual ("jane", ((Runtime.Atom) solutions [1] ["X"]).Name);
        }

        private static void AssertExternalPredicateSolutionIsCorrect (AST.Goal goal, Compiled.Program program)
        {
            var solutions = Helper.GetSolutions (goal, program);

            var variables = solutions.Single();

            Assert.AreEqual ("john", ((Runtime.Atom) variables ["X"]).Name);
        }

        [TestMethod]
        public void PrologPredicateCallingExternalPredicateTest()
        {
            var externalPredicate = new ExternalPredicateDeclaration ("externalPredicate", 1);

            Compiled.Program program = Compiler.Compile (new AST.Program (new []
                                                                                     {
                                                                                         new AST.Clause
                                                                                             {
                                                                                                 Head = Helper.MakeGoal ("callExternalPredicate", "X"),
                                                                                                 Body = new [] {Helper.MakeGoal ("externalPredicate", "X")}
                                                                                             }
                                                                                     }, new []
                                                                                            {
                                                                                                externalPredicate
                                                                                            }));

            program.SetExternalPredicateCallbacks (
                new []
                {
                    new KeyValuePair <ExternalPredicateDeclaration, ExternalPredicateDefinition> (externalPredicate, new SampleExternalPredicate("john").BindVariables)
                }
            );

            var goal = Helper.MakeGoal ("callExternalPredicate", "X");

            AssertExternalPredicateSolutionIsCorrect (goal, program);
        }

        [TestMethod]
        public void UnifyListElementsFails()
        {
            Compiled.Program program = GetStarsDatabase ();

            var goal = Helper.MakeGoal ("stars", "[X,Y,Z]");

            var solutions = Helper.Solve (goal, program);

            Assert.IsFalse(solutions.Any());
        }

        private static Compiled.Program GetStarsDatabase ()
        {
            return Compiler.Compile (new AST.Program (new []
                                                               {
                                                                   Helper.MakeFact ("stars", "[sun, sirius]"),
                                                               }));
        }

        [TestMethod]
        [DeploymentItem("FarmerProblem.txt")]
        public void FarmerProblemHasTwoDistinctSolutions()
        {
            // It is currently giving 64 non-unique solutions because cabb_safe and goat_safe yield multiple solutions.
            var solutions = FindFarmerProblemSolutions ().Select (SolutionToString).ToArray ();

            var distinctSolutions = solutions.Distinct();

            Assert.AreEqual (2, distinctSolutions.Count());
        }

        private static IEnumerable <Runtime.ISolutionTreeNode> FindFarmerProblemSolutions ()
        {
            Compiled.Program program = new Compiler ().Compile ("FarmerProblem.txt");

            var solutions = new Runtime.Engine ().Run (program);

            return solutions;
        }

        private static string SolutionToString (Runtime.ISolutionTreeNode solution)
        {
            const string variableName = "NextState";

            return string.Join (Environment.NewLine, 
                                SelectPath (solution ["run"], "solve")
                                    .Where (n => n.Variables.Contains(variableName))
                                    .Select (n => n.Variables[variableName].Accept (new Runtime.ArgumentPrinter ())));
        }

        static IEnumerable<Runtime.ISolutionTreeNode> SelectPath (Runtime.ISolutionTreeNode node, string goalName)
        {
            while (node != null)
            {
                yield return node;
                node = node [goalName];
            }
        }

        [TestMethod]
        public void BacktrackTest()
        {
            var database = Compiler.Compile (new AST.Program (new []
                                                                       {
                                                                           new AST.Clause
                                                                               {
                                                                                   Head = Helper.MakeGoal("find_match", "P"),
                                                                                   Body = new []
                                                                                              {
                                                                                                  Helper.MakeGoal("match", "P"),
                                                                                                  Helper.MakeGoal("good_match", "P")
                                                                                              }
                                                                               },

                                                                           Helper.MakeFact("match", "bill"),
                                                                           Helper.MakeFact("match", "john"),
                                                                           Helper.MakeFact("good_match", "john"),
                                                                       }));

            var goal = Helper.MakeGoal ("find_match", "X");

            Tracer tracer;
            var solutions = Helper.Solve(database, out tracer, goal).Select (Helper.GetTopLevelVariables).ToArray ();

            Assert.AreEqual (1, solutions.Count ());
            Assert.AreEqual ("john", solutions.Single () ["X"].Accept (new Runtime.ArgumentPrinter ()));
        }
    }
}
