using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace ExViewer.Views
{
    internal static class ViewHelper
    {
        public static void SwipeItemCommandHelper(Microsoft.UI.Xaml.Controls.SwipeItem sender, Microsoft.UI.Xaml.Controls.SwipeItemInvokedEventArgs args)
        {
            sender.CommandParameter = args.SwipeControl.DataContext;
        }

        public static async Task<bool> ScrollAndFocus(ListViewBase listView, object item)
        {
            if (item is null || listView is null)
            {
                return false;
            }

            for (var i = 0; i < 5; i++)
            {
                listView.ScrollIntoView(item);
                await listView.Dispatcher.YieldIdle();
                if (listView.ContainerFromItem(item) is Control con)
                {
                    con.Focus(Windows.UI.Xaml.FocusState.Programmatic);
                    con.StartBringIntoView();
                    return true;
                }
            }
            return false;
        }
    }
}
