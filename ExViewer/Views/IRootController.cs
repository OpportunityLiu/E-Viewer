using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExViewer.Views
{
    public enum RootControlCommand
    {
        SwitchSplitView
    }

    internal interface IRootController
    {
        event EventHandler<RootControlCommand> CommandExecuted;
    }
}
