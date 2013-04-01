using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Prolog.Tests
{
    [TestClass]
    public class LexerTests
    {
        [TestMethod]
        public void Tokenize_WithRealLifePrologClause ()
        {
            var result = Lexer.Tokenize (new StringReader ("dcg_clause --> head '-->' body '.'."));

            var expected = new []
                              {
                                  "dcg_clause",
                                  "-", "-", ">",
                                  "head",
                                  "'", "-", "-", ">", "'",
                                  "body",
                                  "'", ".", "'", ".", 
                              };

            Assert.IsTrue (result.SequenceEqual (expected));
        }
    }
}
