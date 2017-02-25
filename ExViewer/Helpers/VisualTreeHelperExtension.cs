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
    public static class VisualTreeHelperExtension
    {
        #region Children

        public static FrameworkElement FirstChild(this DependencyObject reference, string childName)
        {
            return FirstChild<FrameworkElement>(reference, childName);
        }

        public static IEnumerable<FrameworkElement> Children(this DependencyObject reference, string childName)
        {
            return Children<FrameworkElement>(reference, childName);
        }

        public static T FirstChild<T>(this DependencyObject reference, string childName)
            where T : FrameworkElement
        {
            return Children<T>(reference, childName).FirstOrDefault();
        }

        public static IEnumerable<T> Children<T>(this DependencyObject reference, string childName)
            where T : FrameworkElement
        {
            return Children<T>(reference).Where(item => item.Name == childName);
        }

        public static T FirstChild<T>(this DependencyObject reference)
            where T : DependencyObject
        {
            return Children<T>(reference).FirstOrDefault();
        }

        public static IEnumerable<T> Children<T>(this DependencyObject reference)
            where T : DependencyObject
        {
            return Children(reference).OfType<T>();
        }

        public static DependencyObject FirstChild(this DependencyObject reference)
        {
            return Children(reference).FirstOrDefault();
        }

        public static IEnumerable<DependencyObject> Children(this DependencyObject reference)
        {
            var childrenCount = GetChildrenCount(reference);
            for(int i = 0; i < childrenCount; i++)
                yield return GetChild(reference, i);
        }

        public static DependencyObject Child(this DependencyObject reference, int childIndex)
        {
            return GetChild(reference, childIndex);
        }

        public static int ChildrenCount(this DependencyObject reference)
        {
            return GetChildrenCount(reference);
        }

        #endregion Children

        #region Descendants

        public static FrameworkElement FirstDescendant(this DependencyObject reference, string descendantName)
        {
            return FirstDescendant<FrameworkElement>(reference, descendantName);
        }

        public static IEnumerable<FrameworkElement> Descendants(this DependencyObject reference, string descendantName)
        {
            return Descendants<FrameworkElement>(reference, descendantName);
        }

        public static T FirstDescendant<T>(this DependencyObject reference, string descendantName)
            where T : FrameworkElement
        {
            return Descendants<T>(reference, descendantName).FirstOrDefault();
        }

        public static IEnumerable<T> Descendants<T>(this DependencyObject reference, string descendantName)
            where T : FrameworkElement
        {
            return Descendants<T>(reference).Where(item => item.Name == descendantName);
        }

        public static T FirstDescendant<T>(this DependencyObject reference)
            where T : DependencyObject
        {
            return Descendants<T>(reference).FirstOrDefault();
        }

        public static IEnumerable<T> Descendants<T>(this DependencyObject reference)
            where T : DependencyObject
        {
            return Descendants(reference).OfType<T>();
        }

        public static DependencyObject FirstDescendant(this DependencyObject reference)
        {
            return Descendants(reference).FirstOrDefault();
        }

        public static IEnumerable<DependencyObject> Descendants(this DependencyObject reference)
        {
            var searchQueue = new Queue<DependencyObject>(10);
            searchQueue.Enqueue(reference);
            while(searchQueue.Count != 0)
            {
                var currentSearching = searchQueue.Dequeue();
                foreach(var item in currentSearching.Children())
                {
                    searchQueue.Enqueue(item);
                    yield return item;
                }
            }
        }

        #endregion Descendants

        #region Ancestors

        public static FrameworkElement FirstAncestor(this DependencyObject reference, string ancestorName)
        {
            return FirstAncestor<FrameworkElement>(reference, ancestorName);
        }

        public static IEnumerable<FrameworkElement> Ancestors(this DependencyObject reference, string ancestorName)
        {
            return Ancestors<FrameworkElement>(reference, ancestorName);
        }

        public static T FirstAncestor<T>(this DependencyObject reference, string ancestorName)
            where T : FrameworkElement
        {
            return Ancestors<T>(reference, ancestorName).FirstOrDefault();
        }

        public static IEnumerable<T> Ancestors<T>(this DependencyObject reference, string ancestorName)
            where T : FrameworkElement
        {
            return Ancestors<T>(reference).Where(item => item.Name == ancestorName);
        }

        public static T FirstAncestor<T>(this DependencyObject reference)
            where T : DependencyObject
        {
            return Ancestors<T>(reference).FirstOrDefault();
        }

        public static IEnumerable<T> Ancestors<T>(this DependencyObject reference)
            where T : DependencyObject
        {
            return Ancestors(reference).OfType<T>();
        }

        public static IEnumerable<DependencyObject> Ancestors(this DependencyObject reference)
        {
            while((reference = GetParent(reference)) != null)
                yield return reference;
        }

        #endregion Ancestors

        #region Parent

        public static T Parent<T>(this DependencyObject reference)
            where T : DependencyObject
        {
            return GetParent(reference) as T;
        }

        public static DependencyObject Parent(this DependencyObject reference)
        {
            return GetParent(reference);
        }

        #endregion Parent
    }
}
