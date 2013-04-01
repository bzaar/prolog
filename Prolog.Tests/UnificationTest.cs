using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Prolog.Tests
{
    [TestClass]
    public class UnificationTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            Assert.AreEqual (2, GetSolutionCount ("run :- p(X), q(X). p(Any). q(a). q(b)."));
        }

        [TestMethod]
        public void TestMethod2()
        {
            Assert.AreEqual (1, GetSolutionCount ("run :- p(X), r(X), q(X). p(Any). r(z). r(a). q(a). q(b)."));
        }

        private static int GetSolutionCount (string programText)
        {
            var program = new Compiler ().Compile (new System.IO.StringReader (programText));

            return new Runtime.Engine ().Run (program).Count ();
        }
    }
}
