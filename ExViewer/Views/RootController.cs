using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace ExViewer.Views
{
    public partial class RootControl
    {
        internal static class RootController
        {
            internal static RootControl root;

            public static void SwitchSplitView()
            {
                if(root == null)
                    return;
                root.sv_root.IsPaneOpen = !root.sv_root.IsPaneOpen;
            }

            public static IAsyncOperation<ContentDialogResult> RequireLogOn()
            {
                return new LogOnDialog().ShowAsync();
            }
        }
    }
}
