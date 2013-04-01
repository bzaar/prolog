using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Prolog.Runtime;

namespace Prolog.Tests
{
    [TestClass]
    public class EngineEventTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            TraceTest ("run :- good (X). good (boy).", 
@"Enter: run()
Enter:     good(X=boy)
Solution found
Leave:     good(X=boy)
Leave: run()
");
        }

        [TestMethod]
        public void TestMethod2()
        {
            TraceTest ("run :- good (X), boy (X). good (tom). good (kate). boy (tom).", 
@"Enter: run()
Enter:     good(X=tom)
Enter:     boy(X=tom)
Solution found
Leave:     boy(X=tom)
Leave:     good(X=tom)
Enter:     good(X=kate)
Leave:     good(X=kate)
Leave: run()
");
        }

        [TestMethod]
        public void TestMethod3()
        {
            TraceTest ("run :- good (X), boy (X). good (kate). good (tom). boy (tom).", 
@"Enter: run()
Enter:     good(X=kate)
Leave:     good(X=kate)
Enter:     good(X=tom)
Enter:     boy(X=tom)
Solution found
Leave:     boy(X=tom)
Leave:     good(X=tom)
Leave: run()
");
        }

        [TestMethod]
        public void TestMethod4()
        {
            TraceTest ("run :- good (X), boy (X). good (kate). good (jane). boy (tom).", 
@"Enter: run()
Enter:     good(X=kate)
Leave:     good(X=kate)
Enter:     good(X=jane)
Leave:     good(X=jane)
Leave: run()
");
        }

        private static readonly Compiler compiler = new Compiler ();

        private static void TraceTest (string programText, string expectedTrace)
        {
            var program = compiler.Compile (new StringReader (programText));

            var eventTracer = new EventTracer ();

            foreach (var @event in new EngineInternals().Run (program))
            {
                @event.Accept (eventTracer);
            }

            Assert.AreEqual (expectedTrace, eventTracer.Trace);
        }
    }

    class EventTracer : IDebugEventSink
    {
        readonly StringBuilder sb = new StringBuilder ();

        public string Trace
        {
            get {return sb.ToString ();}
        }

        void IDebugEventSink.Visit(Solution solution)
        {
            sb.AppendLine ("Solution found");
        }

        void IDebugEventSink.Visit(Enter enter)
        {
            PrintGoal ("Enter: ", enter.Node.HeadGoal);
        }

        void IDebugEventSink.Visit (Leave leave)
        {
            PrintGoal ("Leave: ", leave.Node.HeadGoal);
        }

        private void PrintGoal (string eventName, Goal goal)
        {
            sb.Append (eventName);
            sb.Append (new string (' ', goal.Level * 4));
            SolutionTreePrinter.Print (goal, sb); 
            sb.AppendLine ();
        }
    }
}
