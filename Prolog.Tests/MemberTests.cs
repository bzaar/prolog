using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Prolog.Tests
{
    [TestClass]
    class MemberTests
    {
        [TestMethod]
        public void Member ()
        {
            var database = GetMemberDatabase ();

            var goal = Helper.MakeGoal ("member", "apple", "[apple]");

            var solutions = Helper.Solve (goal, database).ToArray();

            Assert.AreEqual (1, solutions.Length);

            Assert.IsFalse (solutions [0].Variables.Any());
        }

        [TestMethod]
        public void Member2 ()
        {
            var database = GetMemberDatabase ();

            var goal = Helper.MakeGoal ("member", "apple", "[pear,apple]");

            var solutions = Helper.Solve (goal, database).ToArray();

            Assert.AreEqual (1, solutions.Length);

            Assert.IsFalse (solutions [0].Variables.Any());
        }

        static Compiled.Program GetMemberDatabase ()
        {
            // member (X, L) :- concat ([X], _, L).
            // member (X, L) :- concat ([_], R, L), member (X, R).
            return Compiler.Compile (
                new AST.Program (
                    new []
                    {
                        new AST.Clause
                            {
                                Head = Helper.MakeGoal ("member", "X", "L"),
                                Body = new [] { Helper.MakeGoal ("concat", "[X]", "_", "L")}
                            },
                        new AST.Clause
                            {
                                Head = Helper.MakeGoal ("member", "X", "L"),
                                Body = new [] 
                                { 
                                    Helper.MakeGoal ("concat", "[_]", "R", "L"),
                                    Helper.MakeGoal ("member", "X", "R")
                                }
                            }
                    }, 
                
                    Helper.GetConcatDeclaration ()
                )
            );
        }

    }
}
