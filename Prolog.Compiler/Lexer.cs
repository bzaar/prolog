using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Prolog.Runtime;

namespace Prolog
{
    public class Lexer
    {
        private readonly TextReader textReader;

        private Lexer (TextReader textReader)
        {
            this.textReader = textReader;
        }

        private IEnumerable <BoundVariableSet> GetOutput (IValue [] arguments)
        {
            var variableSet = new BoundVariableSet ();

            if (variableSet.Unify (arguments [0], new Runtime.List (Tokenize (textReader).Select (t => new Runtime.Atom(t)).ToArray ())))
            {
                yield return variableSet;
            }
        }

        public static IEnumerable <string> Tokenize (TextReader r)
        {
            var word = new StringBuilder ();

            for (;;)
            {
                int code = r.Read ();

                if (code == -1) 
                {
                    if (word.Length > 0)
                    {
                        yield return word.ToString ();
                    }

                    break;
                }

                var c = (char) code;

                if (IsWordChar (c))
                {
                    word.Append (c);
                }
                else
                {
                    if (word.Length > 0)
                    {
                        yield return word.ToString ();

                        word.Clear ();
                    }

                    if (! Char.IsWhiteSpace (c))
                    {
                        yield return c.ToString ();
                    }
                }
            }

        }

        public static bool IsWordChar (char c)
        {
            return Char.IsLetterOrDigit (c) || c == '_';
        }

        public static KeyValuePair <ExternalPredicateDeclaration, ExternalPredicateDefinition> GetLexer (TextReader input)
        {
            return new KeyValuePair <ExternalPredicateDeclaration, ExternalPredicateDefinition> (
                GetExternalPredicateDeclaration (), 
                new Lexer(input).GetOutput);
        }

        public static ExternalPredicateDeclaration GetExternalPredicateDeclaration ()
        {
            return new ExternalPredicateDeclaration ("get_lexer_output", 1);
        }
    }
}