using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prolog.Runtime;

namespace Prolog.Tests
{
    [TestClass]
    [DeploymentItem ("Morpher.txt")]
    public class MorpherTests
    {
        [TestMethod] public void СинийКарандаш ()
        {
            AssertOneSolution ("синий карандаш", 1);
        }

        [TestMethod] public void СинийКарандашИРучка ()
        {
            AssertOneSolution ("синий карандаш и ручка", 1);
        }

        [TestMethod] public void СинийКарандашИКраснаяРучка ()
        {
            AssertOneSolution ("синий карандаш и красная ручка", 1);
        }

        [TestMethod] public void НачальникАвтобазы ()
        {
            AssertOneSolution ("начальник автобазы", 1);
        }

        [Ignore]
        [TestMethod] public void ГвардейскийОрденаЛенинаКраснознаменныйПолк ()
        {
            AssertOneSolution ("гвардейский ордена ленина краснознаменный полк", 3);
        }

        [Ignore]
        [TestMethod] public void ОрденаЛенинаПолк ()
        {
            AssertOneSolution ("ордена ленина полк", 3);
        }

        [TestMethod] public void ПрофессорКислыхЩей ()
        {
            AssertOneSolution ("профессор кислых щей", 1);
        }

        private static void AssertOneSolution (string text, int expectedSolutionCount)
        {
            var externalPredicates = new [] {Concat.GetConcat (), Lexer.GetLexer (new StringReader (text))};

            program.SetExternalPredicateCallbacks (externalPredicates);

            var solutionTreePrinter = new SolutionTreePrinter (SolutionTreePrinter.PrintDcgNode);

            var engine = new Engine ();

            var tracer = new Tracer();
            engine.Unified += tracer.Engine_Unified;
            engine.Failed  += tracer.Engine_Failed;

            var solutions = engine.Run (program).Select (solutionTreePrinter.Print).ToArray ();

            Assert.AreEqual (expectedSolutionCount, solutions.Count ());
        }

        private static Compiled.Program GetProgram ()
        {
            var externalPredicateDeclarations = new []
                                                    {
                                                        Concat.GetConcat().Key,
                                                        Lexer.GetExternalPredicateDeclaration ()
                                                    };

            return new Compiler ().Compile ("Morpher.txt", externalPredicateDeclarations);
        }

        private static readonly Compiled.Program program = GetProgram ();
    }
}
