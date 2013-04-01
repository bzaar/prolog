using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace Prolog
{
    public class Parser
    {
        private readonly Compiled.Program program = LoadProgramFromResource ();

        public AST.Program ParseProgram (TextReader r)
        {
            program.SetExternalPredicateCallbacks (GetExternalPredicates (r));

            var engine = new Runtime.Engine ();

            IEnumerable <Runtime.ISolutionTreeNode> solutions = engine.Run (program);

            return solutions.Select (ConvertToAst).FirstOrDefault ();
        }

        static Compiled.Program LoadProgramFromResource ()
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Prolog.Grammar.bin"))
            {
// ReSharper disable AssignNullToNotNullAttribute
                var program = (Compiled.Program) new BinaryFormatter ().Deserialize (stream);
                return program;
// ReSharper restore AssignNullToNotNullAttribute
            }
        }

        private static AST.Program ConvertToAst(Runtime.ISolutionTreeNode unnamed_top_node)
        {
            var clause_nodes = ParsePossiblyEmptyList (unnamed_top_node ["run"] ["program"], "clause_list", "clause");

            var dcgClauses = new List <AST.DcgClause> ();
            var prologClauses = new List <AST.Clause> ();

            foreach (var clause_node in clause_nodes)
            {
                var dgc_clause_node = clause_node ["dcg_clause"];

                if (dgc_clause_node != null)
                {
                    var head = GoalToAst (dgc_clause_node ["dcg_head"] ["goal"]);

                    var dcgClause = new AST.DcgClause    
                                        {
                                            Head = new AST.Goal {
                                                                    PredicateName = head.PredicateName,
                                                                    Arguments = head.Arguments
                                                                },
                                            Body = ParsePossiblyEmptyList (dgc_clause_node ["dcg_body"], "dcg_goal_list", "dcg_goal").Select (DcgGoalToAst).ToArray ()
                                        };

                    dcgClauses.Add (dcgClause);
                }
                else
                {
                    var prolog_clause_node = clause_node ["prolog_clause"];

                    if (prolog_clause_node != null)
                    {
                        var prologClause = new AST.Clause 
                        {
                            Head = GoalToAst (prolog_clause_node ["head"] ["goal"]),
                            Body = ParsePossiblyEmptyList (prolog_clause_node ["body"], "goal_list", "goal").Select (GoalToAst).ToArray ()
                        };

                        prologClauses.Add (prologClause);
                    }
                    else
                    {
                        throw new Exception ("A 'clause' is expected to be either a 'dcg_clause' or a 'prolog_clause'.");
                    }
                }
            }

            return new AST.Program {
                Clauses = prologClauses.ToArray (),
                DcgClauses = dcgClauses.ToArray ()
            };
        }

        private static string ToString (Runtime.IConcreteValue value)
        {
            return value.Accept (new Runtime.ArgumentPrinter());
        }

        private static AST.IDcgGoal DcgGoalToAst(Runtime.ISolutionTreeNode node)
        {
            Runtime.ISolutionTreeNode n;

            if ((n = node ["goal"]) != null) 
            {
                var goal = GoalToAst (n);

                return new AST.DcgSubgoal
                    {
                        PredicateName = goal.PredicateName,
                        Arguments = goal.Arguments
                    };
            }

            if ((n = node ["embedded_goal"]) != null)
            {
                var goal = GoalToAst (n ["goal"]);

                return new AST.DcgNonDcgGoal
                    {
                        PredicateName = goal.PredicateName,
                        Arguments = goal.Arguments
                    };
            }

            if ((n = node ["variable"]) != null) 
            {
                return new AST.DcgLiteral {Value = new Variable (ToString (n.Variables ["Name"]))};
            }

            Runtime.IConcreteValue variable;

            if ((variable = node.Variables ["Literal"]) != null) 
            {
                return new AST.DcgLiteral {Value = new Atom (ToString (variable))};
            }

            throw new Exception ("Unrecognized DCG goal.");
        }

        private static AST.Goal GoalToAst (Runtime.ISolutionTreeNode goal_node)
        {
            return new AST.Goal 
            {
                PredicateName = ToString (goal_node ["lowercase"].Variables ["Name"]),
                Arguments = ParseArgumentList (goal_node)
            };
        }

        private static IArgument [] ParseArgumentList (Runtime.ISolutionTreeNode goal_node)
        {
            return ParsePossiblyEmptyList (goal_node, "arg_list", "arg").Select (ArgumentToAst).ToArray ();
        }

        private static IArgument ArgumentToAst(Runtime.ISolutionTreeNode arg_node)
        {
            var variable_node = arg_node ["variable"];

            if (variable_node != null)
            {
                return new Variable (ToString (variable_node.Variables ["Name"]));
            }

            var atom_node = arg_node ["atom"];

            if (atom_node != null)
            {
                return new Atom (ToString (atom_node ["lowercase"].Variables ["Name"]));
            }

            var list_node = arg_node ["list"];

            if (list_node != null)
            {
                return new List (ParseArgumentList (list_node));
            }

            throw new Exception ("An 'arg' is expected to be either an 'atom' or a 'variable' or a 'list'.");
        }

        static IEnumerable <Runtime.ISolutionTreeNode> ParsePossiblyEmptyList (Runtime.ISolutionTreeNode node, string listName, string elementName)
        {
            if (node != null)
            {
                while (true)
                {
                    node = node [listName];

                    if (node == null) break;

                    var elementNode = node [elementName];

                    if (elementNode != null)
                    {
                        yield return elementNode;
                    }
                }
            }
        }

        public static KeyValuePair <ExternalPredicateDeclaration, ExternalPredicateDefinition> [] GetExternalPredicates (TextReader input)
        {
            return new [] 
                       {
                           Concat.GetConcat (),
                           Lexer.GetLexer (input),
                           new KeyValuePair <ExternalPredicateDeclaration, ExternalPredicateDefinition> (new ExternalPredicateDeclaration ("is_uppercase", 1), IsUpperCase),
                           new KeyValuePair <ExternalPredicateDeclaration, ExternalPredicateDefinition> (new ExternalPredicateDeclaration ("is_lowercase", 1), IsLowerCase)
                       };
        }

        public static ExternalPredicateDeclaration [] GetExternalPredicateDeclarations ()
        {
            return GetExternalPredicates (null).Select (kvp => kvp.Key).ToArray ();
        }

        static IEnumerable <Runtime.BoundVariableSet> IsUpperCase (Runtime.IValue [] arguments)
        {
            string name = ((Runtime.Atom) arguments [0].ConcreteValue).Name;

            if (name.All (Lexer.IsWordChar) && Char.IsUpper (name [0]))
            {
                yield return new Runtime.BoundVariableSet ();
            }
        }

        static IEnumerable <Runtime.BoundVariableSet> IsLowerCase (Runtime.IValue [] arguments)
        {
            string name = ((Runtime.Atom) arguments [0].ConcreteValue).Name;

            if (name.All (Lexer.IsWordChar) && Char.IsLower (name [0]))
            {
                yield return new Runtime.BoundVariableSet ();
            }
        }
    }
}