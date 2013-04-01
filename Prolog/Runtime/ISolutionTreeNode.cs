using System.Collections.Generic;
using System.Text;

namespace Prolog.Runtime
{
    public interface ISolutionTreeNode : IEnumerable<ISolutionTreeNode>
    {
        Variables Variables {get;}
        Goal HeadGoal { get; set; }

        ISolutionTreeNode this [string goalName] {get;}
    }
}