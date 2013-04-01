using System;
using System.IO;
using System.Windows.Forms;
using Prolog.Runtime;

namespace Prolog.IDE
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string [] args)
        {
            Compiled.Program program = GetProgram (args[0]);

            var externalPredicates = new [] {Concat.GetConcat (), Lexer.GetLexer (new StringReader ("ордена ленина полк"))};

            program.SetExternalPredicateCallbacks (externalPredicates);

            var engine = new EngineInternals ();

            var events = engine.Run (program);

            Application.EnableVisualStyles ();            
            Application.SetCompatibleTextRenderingDefault (false);
            Application.Run (new Form1 (events));
        }

        private static Compiled.Program GetProgram (string fileName)
        {
            var externalPredicateDeclarations = new []
                                                    {
                                                        Concat.GetConcat().Key,
                                                        Lexer.GetExternalPredicateDeclaration ()
                                                    };

            return new Compiler ().Compile (fileName, externalPredicateDeclarations);
        }
    }
}
