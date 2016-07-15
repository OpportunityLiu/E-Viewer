using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml;
using static Windows.UI.Xaml.Media.VisualTreeHelper;

namespace ExViewer
{
    public static class VisualTreeHelperEx
    {
        public static FrameworkElement GetFirstNamedChild(DependencyObject reference, string childName)
        {
            if(string.IsNullOrEmpty(childName))
                throw new ArgumentNullException(nameof(childName));
            var searchQueue = new Queue<DependencyObject>();
            searchQueue.Enqueue(reference);
            while(searchQueue.Count != 0)
            {
                var currentSearching = searchQueue.Dequeue();
                var childrenCount = GetChildrenCount(currentSearching);
                for(int i = 0; i < childrenCount; i++)
                {
                    var currentChild = GetChild(currentSearching, i);
                    var feChild = currentChild as FrameworkElement;
                    if(feChild?.Name == childName)
                        return feChild;
                    searchQueue.Enqueue(currentChild);
                }
            }
            return null;
        }
    }
}
