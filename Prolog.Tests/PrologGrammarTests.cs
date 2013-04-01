using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Prolog.Tests
{
    [TestClass]
    [DeploymentItem ("PrologGrammar.txt")]
    public class PrologGrammarTests
    {
        [TestMethod] public void ParseDcgBody () 
        {
            AssertOneSolution ("dcg_body", "a b c");
        }
        [TestMethod] public void ParseDcgBodyOneLiteralInSingleQuotes () 
        {
            AssertOneSolution ("dcg_body", "'x'");
        }
        [TestMethod] public void ParseDcgBodyOneLiteralInDoubleQuotes () 
        {
            AssertOneSolution ("dcg_body", "\"x\"");
        }
        [TestMethod] public void ParseDcgBodySingleQuoteInDoubleQuotes ()
        {
            AssertOneSolution ("dcg_body", "\"'\"");
        }
        [TestMethod] public void ParseDcgBodyDoubleQuoteInSingleQuotes ()
        {
            AssertOneSolution ("dcg_body", "'\"'");
        }
        [TestMethod] public void ParseDcgBodyDoubleQuoteInSingleQuotesFollowedBySubgoal ()
        {
            AssertOneSolution ("dcg_body", "'\"' subgoal");
        }
        [TestMethod] public void ParseDcgBodyDoubleQuoteInSingleQuotesFollowedBySubgoalAndThenAnotherDoubleQuoteInSingleQuotes ()
        {
            AssertOneSolution ("dcg_body", "'\"' Literal '\"'");
        }
        [TestMethod] public void ParseDcgClause ()
        {
            AssertOneSolution ("dcg_clause", "a --> b.");
        }
        [TestMethod] public void ParseDcgClauseWithTwoSubgoals ()
        {
            AssertOneSolution ("dcg_clause", "a --> b c.");
        }
        [TestMethod] public void ParseDcgGoalList ()
        {
            AssertOneSolution ("dcg_goal_list", "b c");
        }
        [TestMethod] public void ParseDcgGoalList3 ()
        {
            AssertOneSolution ("dcg_goal_list", "b c d");
        }
        [TestMethod] public void ParsePrologClauseWithArgs ()
        {
            AssertOneSolution ("prolog_clause", "good_boy (X) :- boy (X), good (X).");
        }
        [TestMethod] public void ParsePrologClauseWithoutArgs ()
        {
            AssertOneSolution ("prolog_clause", "run :- goal1 (X), goal2 (X).");
        }
        [TestMethod] public void ParseFact ()
        {
            AssertOneSolution ("prolog_clause", "man (adam).");
        }
        [TestMethod] public void ParseFactWithoutArgs ()
        {
            AssertOneSolution ("prolog_clause", "true.");
        }
        [TestMethod] public void ParseFactWithoutArgsWithParens ()
        {
            AssertOneSolution ("prolog_clause", "true ().");
        }
        [TestMethod] public void ParsePrologClauseWithoutArgs2 ()
        {
            AssertOneSolution ("prolog_clause", "run :- get_lexer_output (L), program (L, []).");
        }
        [TestMethod] public void ParsePrologClauseWithoutArgs2AsProgram ()
        {
            AssertOneSolution ("program", @"run :- get_lexer_output (L), program (L, []). ");
        }
        [TestMethod] public void ParseEmptyList ()
        {
            AssertOneSolution ("list", "[]");
        }
        [TestMethod] public void ParseListOf1 ()
        {
            AssertOneSolution ("list", "[a]");
        }
        [TestMethod] public void ParseListOf2 ()
        {
            AssertOneSolution ("list", "[a,b]");
        }
        [TestMethod] public void ParseListOfLists ()
        {
            AssertOneSolution ("list", "[a, [b], [c,d]]");
        }
        [TestMethod] public void ParseRecursiveListOfLists ()
        {
            AssertOneSolution ("list", "[[[[[[x]]]]]]");
        }
        [TestMethod] public void UnmatchedBrackets ()
        {
            AssertNoSolutions ("[[[[[[x]]]]]", "list");
        }
        [TestMethod] public void ExtraBrackets ()
        {
            AssertNoSolutions ("[[[[[[x]]]]]]]", "list");
        }
        [TestMethod] public void ParseListAsArg ()
        {
            AssertOneSolution ("arg", "[]", s => s ["arg"] ["list"] != null);
        }
        [TestMethod] public void ParseGoal ()
        {
            AssertOneSolution ("goal", "program (L, [])");
        }
        [TestMethod] public void ParseBody ()
        {
            AssertOneSolution ("body", "get_lexer_output (L), program (L, [])", 
                s => s ["body"] ["goal_list"] ["goal"] ["lowercase"].Variables ["Name"].Accept (new Runtime.ArgumentPrinter()) == "get_lexer_output"
                  && s ["body"] ["goal_list"] ["goal_list"] ["goal"] ["lowercase"].Variables ["Name"].Accept (new Runtime.ArgumentPrinter()) == "program"
                );
        }

        [TestMethod]
        public void CompileSelf ()
        {
            var program = GetGrammarToCompile (new StreamReader ("PrologGrammar.txt"));

            //using (var file = new FileStream (@"C:\Users\Sergey\Documents\mt\russian\morpher\Prolog\Prolog.Compiler\Grammar.bin", FileMode.Create, FileAccess.Write))
            //{
            //    new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter ().Serialize (file, program);
            //}

            Tracer tracer;
            var solutions = Helper.Solve (program, out tracer, Helper.MakeGoal ("get_lexer_output", "L"));

            Assert.AreEqual (1, solutions.Count ());
        }

        [TestMethod]
        public void ParseProgramByClause ()
        {
            foreach (string line in File.ReadAllLines("PrologGrammar.txt").Where (line => !string.IsNullOrWhiteSpace (line)))
            {
                AssertOneSolution ("clause", line);
            }
        }

        private static void AssertOneSolution (string goal, string line, System.Func <Runtime.ISolutionTreeNode, bool> isSolutionCorrect = null)
        {
            if (isSolutionCorrect == null)
            {
                isSolutionCorrect = delegate (Runtime.ISolutionTreeNode node) {return true;};
            }

            var file = new StringReader (line);

            var solutions = Compile (file, goal).Select (isSolutionCorrect).ToArray ();

            Assert.AreEqual (1, solutions.Length);
            Assert.IsTrue (solutions [0], "The solution is incorrect.");
        }

        private static void AssertNoSolutions (string expr, string goal)
        {
            var line = new StringReader (expr);

            var solutions = Compile (line, goal);

            Assert.AreEqual (0, solutions.Count ());
        }

        private static System.Collections.Generic.IEnumerable <Runtime.ISolutionTreeNode> Compile (StringReader file, string goal)
        {
            var program1 = GetGrammarToCompile (file);

            Tracer tracer;
            return Helper.Solve (program1, out tracer, 
                                 Helper.MakeGoal ("get_lexer_output", "L"),
                                 Helper.MakeGoal (goal, "L", "[]"));
        }

        private static Compiled.Program GetGrammarToCompile (TextReader input)
        {
            program.SetExternalPredicateCallbacks (Parser.GetExternalPredicates (input));
            return program;
        }

        private static readonly Compiler compiler = new Compiler ();
        private static readonly Compiled.Program program = compiler.Compile ("PrologGrammar.txt", Parser.GetExternalPredicateDeclarations ());
    }
}
