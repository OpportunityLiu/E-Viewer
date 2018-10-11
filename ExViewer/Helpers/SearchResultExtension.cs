using ExClient.Search;
using ExViewer.Settings;
using Windows.UI.Xaml;

namespace ExClient
{
    internal static class SearchResultExtension
    {
        public static Visibility IsEmptyVisible(int count,int pageCount)
        {
            if (count == 0 && pageCount == 0)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }
    }
}
