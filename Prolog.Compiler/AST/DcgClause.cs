namespace Prolog.AST
{
    public class DcgClause
    {
        public Goal Head {get; set;}

        public IDcgGoal [] Body {get; set;}
    }
}