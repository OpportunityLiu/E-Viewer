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
            return GetNamedChildren(reference, childName).FirstOrDefault();
        }

        public static IEnumerable<FrameworkElement> GetNamedChildren(DependencyObject reference, string childName)
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
                        yield return feChild;
                    searchQueue.Enqueue(currentChild);
                }
            }
        }

        public static T GetFirstChild<T>(DependencyObject reference)
            where T : DependencyObject
        {
            return GetChildren<T>(reference).FirstOrDefault();
        }

        public static IEnumerable<T> GetChildren<T>(DependencyObject reference)
            where T : DependencyObject
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
                    var tChild = currentChild as T;
                    if(tChild != null)
                        yield return tChild;
                    searchQueue.Enqueue(currentChild);
                }
            }
        }
    }
}
