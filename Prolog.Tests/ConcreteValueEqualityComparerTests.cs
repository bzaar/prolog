using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prolog.Runtime;

namespace Prolog.Tests
{
    using Atom = Runtime.Atom;
    using List = Runtime.List;
    using Variable = Runtime.Variable;

    [TestClass]
    public class ConcreteValueEqualityComparerTests
    {
        [TestMethod]
        public void TwoAtomsThatAreEqual()
        {
            Assert.IsTrue (EngineInternals.AreEqual (new Atom("x"), new Atom("x")));
        }

        [TestMethod]
        public void TwoAtomsThatAreNotEqual()
        {
            Assert.IsFalse (EngineInternals.AreEqual (new Atom("x"), new Atom("y")));
        }

        [TestMethod]
        public void ListVsAtom()
        {
            Assert.IsFalse (EngineInternals.AreEqual (
                new List (new IValue [] {new Atom("x")}), 
                new Atom("y")));
        }

        [TestMethod]
        public void TwoListsOfLengthOneThatAreEqual()
        {
            Assert.IsTrue (EngineInternals.AreEqual (
                new List (new [] {new Atom("x")}), 
                new List (new [] {new Atom("x")})));
        }

        [TestMethod]
        public void TwoListsOfLengthTwoThatAreEqual()
        {
            Assert.IsTrue (EngineInternals.AreEqual (
                new List (new IValue [] {new Atom("x"), new Atom("y")}), 
                new List (new IValue [] {new Atom("x"), new Atom("y")})));
        }

        [TestMethod]
        public void TwoEmptyLists()
        {
            Assert.IsTrue (EngineInternals.AreEqual (
                new List (new IValue [] {}), 
                new List (new IValue [] {})));
        }

        [TestMethod]
        public void UnboundVariableVsAtom()
        {
            Assert.IsFalse (EngineInternals.AreEqual (
                new Variable("X"), 
                new Atom ("X")));
        }

        [TestMethod]
        public void AtomVsVariableBoundToSameAtom()
        {
            Assert.IsTrue (EngineInternals.AreEqual (
                new Variable("X") {BoundTo = new Atom ("X")}, 
                new Atom ("X")));
        }
    }
}
