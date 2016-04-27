using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExViewer.Views
{
    public enum MainPageControlCommand
    {
        SwitchSplitView
    }

    internal interface IMainPageController
    {
        event EventHandler<MainPageControlCommand> CommandExecuted;
    }
}
