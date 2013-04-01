using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Prolog.Tests
{
    [TestClass]
    public class DcgTests
    {
        [TestMethod]
        public void OneArgInParens ()
        {
            // arglist (Arg) --> '(' Arg ')'.

            var program = Compiler.Compile 
            (
                new AST.Program {
                    DcgClauses = new []
                    {
                        new AST.DcgClause 
                        {
                            Head = Helper.MakeGoal ("arglist", "Arg"),
                            Body = new AST.IDcgGoal[]
                            {
                                new AST.DcgLiteral {Value = new Atom ("(")},
                                new AST.DcgLiteral {Value = new Variable ("Arg")},
                                new AST.DcgLiteral {Value = new Atom (")")}
                            }
                        }
                    }, 
                    ExternalPredicates = Helper.GetConcat ().Select (kvp => kvp.Key).ToArray ()
                }
            );

            program.SetExternalPredicateCallbacks (Helper.GetConcat ());

            var solutions = Helper.GetSolutions(Helper.MakeGoal ("arglist", "Arg", "[(, arg1, )]", "[]"), program);

            Assert.AreEqual (1, solutions.Count ());
            Assert.AreEqual ("arg1", ((Runtime.Atom) solutions.Single()["Arg"]).Name);
        }

        // arglist --> arg.
        // arglist --> arg ';' argList.
        // arg --> Arg.
        static Compiled.Program ArgListProgram ()
        {
            var program = Compiler.Compile 
                (
                    new AST.Program {
                                        DcgClauses = new []
                                                         {
                                                             // arglist --> arg.
                                                             new AST.DcgClause 
                                                                 {
                                                                     Head = Helper.MakeGoal ("arglist"),
                                                                     Body = new AST.IDcgGoal[]
                                                                                {
                                                                                    new AST.DcgSubgoal {PredicateName = "arg", Arguments = new IArgument[0]},
                                                                                }
                                                                 },

                                                             // arglist --> arg ';' argList.
                                                             new AST.DcgClause 
                                                                 {
                                                                     Head = Helper.MakeGoal ("arglist"),
                                                                     Body = new AST.IDcgGoal[]
                                                                                {
                                                                                    new AST.DcgSubgoal {PredicateName = "arg", Arguments = new IArgument[0]},
                                                                                    new AST.DcgLiteral {Value = new Atom (";")},
                                                                                    new AST.DcgSubgoal {PredicateName = "arglist", Arguments = new IArgument[0]}
                                                                                }
                                                                 },

                                                             // arg --> Arg.
                                                             new AST.DcgClause 
                                                                 {
                                                                     Head = Helper.MakeGoal ("arg"),
                                                                     Body = new AST.IDcgGoal[]
                                                                                {
                                                                                    new AST.DcgLiteral {Value = new Variable ("Arg")},
                                                                                }
                                                                 }
                                                         }, 

                                        ExternalPredicates = Helper.GetConcatDeclaration ()
                                    }
                );

            program.SetExternalPredicateCallbacks (Helper.GetConcat ());

            return program;
        }

        [TestMethod]
        public void CommaSeparatedList ()
        {
            var database = ArgListProgram ();

            var solutions = Helper.Solve (Helper.MakeGoal ("arglist", "[cats, ;, like, ;, mice]", "[]"), database);

            Assert.AreEqual ("cats like mice", solutions.Select (SolutionToString).Single ());
        }

        [TestMethod]
        public void CommaSeparatedList_BadInput ()
        {
            const string input = "[cats, like, mice]"; // not comma-separated

            var database = ArgListProgram ();

            var solutions = Helper.Solve (Helper.MakeGoal ("arglist", input, "[]"), database);

            Assert.AreEqual (0, solutions.Count ());
        }

        static string SolutionToString (Runtime.ISolutionTreeNode solution)
        {
            return string.Join (" ", SolutionToStringRecursive (solution));
        }

        static IEnumerable <string> SolutionToStringRecursive (Runtime.ISolutionTreeNode solution)
        {
            for (;;)
            {
                solution = solution ["arglist"];

                if (solution == null) break;

                yield return solution ["arg"].Variables ["Arg"].Accept (new Runtime.ArgumentPrinter ());
            }
        }

        [TestMethod]
        public void ParsePrologGoal ()
        {
            // goal --> Name optional_arglist_inparens.
            // optional_arglist_inparens --> .
            // optional_arglist_inparens --> arglist.
            // arglist --> arg.
            // arglist --> arg ';' argList.
            // arg --> Arg.
        }

        [TestMethod]
        public void ParseDcgClause2 ()
        {
            // # A program defining its own syntax!
            // dcg_clause --> head '-->' body '.'.
            // head --> Name.
            // body --> .
            // body --> dcg_goal body.
            // dcg_goal --> Name.
            var program = Compiler.Compile 
                (
                    new AST.Program {
                                        DcgClauses = new []
                                                         {
                                                            MakeDcgClause ("dcg_clause", "head '-->' body '.'"),
                                                            MakeDcgClause ("head", "Name"),
                                                            MakeDcgClause ("body", ""),
                                                            MakeDcgClause ("body", "dcg_goal body"),
                                                            MakeDcgClause ("dcg_goal", "Name"),
                                                         }, 

                                        ExternalPredicates = Helper.GetConcatDeclaration ()
                                    }
                );

            program.SetExternalPredicateCallbacks (Helper.GetConcat ());

            var goal = Helper.MakeGoal ("dcg_clause", "[a, -->, b, .]", "[]");

            Tracer tracer;
            var solutions = Helper.Solve (program, out tracer, goal).Select (Runtime.SolutionTreePrinter.SolutionTreeToString).ToArray ();

            Assert.AreEqual (1, solutions.Count ());
        }


        private static AST.DcgClause MakeDcgClause (string head, string body)
        {
            return new AST.DcgClause 
                       {
                           Head = Helper.MakeGoal (head),
                           Body = string.IsNullOrWhiteSpace (body) ? new AST.IDcgGoal[0] : body.Split (' ').Select (ParseDcgGoal).ToArray()
                       };
        }

        static AST.IDcgGoal ParseDcgGoal (string s)
        {
            if (s.StartsWith ("'") || s.StartsWith ("\""))
                return new AST.DcgLiteral {Value = new Atom (s.Substring (1, s.Length-2))};


            return (char.IsUpper (s [0])) 
                ? (AST.IDcgGoal) new AST.DcgLiteral {Value = new Variable (s)} 
                : new AST.DcgSubgoal {PredicateName = s, Arguments = new IArgument [0]};
        }
    }
}
