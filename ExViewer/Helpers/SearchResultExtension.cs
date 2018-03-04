using ExClient.Search;
using ExViewer.Settings;
using Windows.UI.Xaml;

namespace ExClient
{
    static class SearchResultExtension
    {
        public static Visibility IsEmptyVisible(int count, bool hasMoreItems)
        {
            if (count == 0 && !hasMoreItems)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }
    }
}
