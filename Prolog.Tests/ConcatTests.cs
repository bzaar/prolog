using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Prolog.Tests
{
    /// <summary>
    /// Tests for the <see cref="Concat"/> predicate.
    /// </summary>
    [TestClass]
    public class ConcatTests
    {
        // Not all I/O patterns are supported yet; more will be added as needed.

        [TestMethod] public void Empty  () {AssertX ("[]",  "[]", "X", "[]");}

        [TestMethod] public void a1  () {AssertX ("[]",  "[a]", "X", "[a]");}
        [TestMethod] public void a2  () {AssertX ("[a]", "[]", "X", "[a]");}

        [TestMethod] public void ab1 () {AssertX ("[a, b]", "[]", "X", "[a,b]");}
        [TestMethod] public void ab2 () {AssertX ("[b]",   "[a]", "X", "[a,b]");}
        [TestMethod] public void ab3 () {AssertX ("[]",  "[a,b]", "X", "[a,b]");}

        [TestMethod] public void aaa ()
        {
            AssertNoSolutions (Helper.MakeGoal ("concat", "[a]", "[a]", "[a]"));
        }

        [TestMethod] public void a_empty_ab ()
        {
            AssertNoSolutions (Helper.MakeGoal ("concat", "[a]", "[]", "[a,b]"));
        }

        private static void AssertNoSolutions (AST.Goal goal)
        {
            var solutions = CallConcat (goal);

            Assert.IsFalse (solutions.Any ());
        }

        private static void AssertX (string x, params string [] args)
        {
            var solutions = CallConcat (Helper.MakeGoal ("concat", args));

            Assert.AreEqual (x, solutions.Single () ["X"].Accept (new Runtime.ArgumentPrinter ()));

            var solutions2 = CallConcat (Helper.MakeGoal ("concat", args [0], x, args [2]));

            Assert.AreEqual (1, solutions2.Count ());
        }

        private static IEnumerable <Dictionary <string, Runtime.IConcreteValue>> CallConcat (AST.Goal makeGoal)
        {
            var program = Compiler.Compile (new AST.Program {ExternalPredicates = Helper.GetConcatDeclaration ()});

            program.SetExternalPredicateCallbacks (Helper.GetConcat ());

            return Helper.GetSolutions (makeGoal, program);
        }
    }
}
