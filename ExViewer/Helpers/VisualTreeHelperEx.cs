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

namespace Windows.UI.Xaml.Media
{
    public static class VisualTreeHelperEx
    {
        public static FrameworkElement GetFirstChild(DependencyObject reference, string childName)
        {
            return GetFirstChild<FrameworkElement>(reference, childName);
        }

        public static IEnumerable<FrameworkElement> GetChildren(DependencyObject reference, string childName)
        {
            return GetChildren<FrameworkElement>(reference, childName);
        }

        public static T GetFirstChild<T>(DependencyObject reference, string childName)
            where T : FrameworkElement
        {
            return GetChildren<T>(reference, childName).FirstOrDefault();
        }

        public static IEnumerable<T> GetChildren<T>(DependencyObject reference, string childName)
            where T : FrameworkElement
        {
            return GetChildren<T>(reference).Where(item => item.Name == childName);
        }

        public static T GetFirstChild<T>(DependencyObject reference)
            where T : DependencyObject
        {
            return GetChildren<T>(reference).FirstOrDefault();
        }

        public static IEnumerable<T> GetChildren<T>(DependencyObject reference)
            where T : DependencyObject
        {
            return GetChildren(reference).OfType<T>();
        }

        public static DependencyObject GetFirstChild(DependencyObject reference)
        {
            return GetChildren(reference).FirstOrDefault();
        }

        public static IEnumerable<DependencyObject> GetChildren(DependencyObject reference)
        {
            var searchQueue = new Queue<DependencyObject>();
            searchQueue.Enqueue(reference);
            while(searchQueue.Count != 0)
            {
                var currentSearching = searchQueue.Dequeue();
                var childrenCount = GetChildrenCount(currentSearching);
                for(int i = 0; i < childrenCount; i++)
                {
                    var currentChild = GetChild(currentSearching, i);
                    searchQueue.Enqueue(currentChild);
                    yield return currentChild;
                }
            }
        }
    }
}
