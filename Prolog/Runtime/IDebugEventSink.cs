using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prolog.Runtime
{
    public interface IDebugEventSink
    {
        void Visit(Solution solution);

        void Visit(Enter enter);

        void Visit(Leave leave);
    }
}
