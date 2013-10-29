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
            AssertSolutionCount ("синий карандаш", 1);
        }

        [TestMethod] public void СинийКарандашИРучка ()
        {
            AssertSolutionCount ("синий карандаш и ручка", 1);
        }

        [TestMethod] public void СинийКарандашИКраснаяРучка ()
        {
            AssertSolutionCount ("синий карандаш и красная ручка", 1);
        }

        [TestMethod] public void НачальникАвтобазы ()
        {
            AssertSolutionCount ("начальник автобазы", 1);
        }

        [Ignore]
        [TestMethod] public void ГвардейскийОрденаЛенинаКраснознаменныйПолк ()
        {
            AssertSolutionCount ("гвардейский ордена ленина краснознаменный полк", 3);
        }

        [TestMethod] public void ОрденаЛенинаПолк ()
        {
            AssertSolutionCount ("ордена ленина полк", 3);
        }

        [TestMethod] public void ПрофессорКислыхЩей ()
        {
            AssertSolutionCount ("профессор кислых щей", 1);
        }

        private static void AssertSolutionCount (string text, int expectedSolutionCount)
        {
            var externalPredicates = new [] {Concat.GetConcat (), Lexer.GetLexer (new StringReader (text))};

            program.SetExternalPredicateCallbacks (externalPredicates);

            var engine = new Engine ();

            //var tracer = new Tracer();
            //engine.Unified += tracer.Engine_Unified;
            //engine.Failed  += tracer.Engine_Failed;

            var solutions = engine.Run (program).Select (Print).ToArray ();

            Assert.AreEqual (expectedSolutionCount, solutions.Count ());
        }

        private static string Print (ISolutionTreeNode solution)
        {
            return new SolutionTreePrinter (SolutionTreePrinter.PrintDcgNode).Print (solution);
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
