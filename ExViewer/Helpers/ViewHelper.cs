using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace ExViewer.Views
{
    static class ViewHelper
    {
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
