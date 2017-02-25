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
            return reference.DescendantsAndSelf().Skip(1);
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
            return reference.AncestorsAndSelf().Skip(1);
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

        #region ChildrenAndSelf

        public static FrameworkElement FirstChildOrSelf(this DependencyObject reference, string childName)
        {
            return FirstChildOrSelf<FrameworkElement>(reference, childName);
        }

        public static IEnumerable<FrameworkElement> ChildrenAndSelf(this DependencyObject reference, string childName)
        {
            return ChildrenAndSelf<FrameworkElement>(reference, childName);
        }

        public static T FirstChildOrSelf<T>(this DependencyObject reference, string childName)
            where T : FrameworkElement
        {
            return ChildrenAndSelf<T>(reference, childName).FirstOrDefault();
        }

        public static IEnumerable<T> ChildrenAndSelf<T>(this DependencyObject reference, string childName)
            where T : FrameworkElement
        {
            return ChildrenAndSelf<T>(reference).Where(item => item.Name == childName);
        }

        public static T FirstChildOrSelf<T>(this DependencyObject reference)
            where T : DependencyObject
        {
            return ChildrenAndSelf<T>(reference).FirstOrDefault();
        }

        public static IEnumerable<T> ChildrenAndSelf<T>(this DependencyObject reference)
            where T : DependencyObject
        {
            return ChildrenAndSelf(reference).OfType<T>();
        }

        public static DependencyObject FirstChildOrSelf(this DependencyObject reference)
        {
            return ChildrenAndSelf(reference).FirstOrDefault();
        }

        public static IEnumerable<DependencyObject> ChildrenAndSelf(this DependencyObject reference)
        {
            var childrenCount = GetChildrenCount(reference);
            yield return reference;
            for(int i = 0; i < childrenCount; i++)
                yield return GetChild(reference, i);
        }

        #endregion ChildrenAndSelf

        #region DescendantsAndSelf

        public static FrameworkElement FirstDescendantOrSelf(this DependencyObject reference, string descendantName)
        {
            return FirstDescendantOrSelf<FrameworkElement>(reference, descendantName);
        }

        public static IEnumerable<FrameworkElement> DescendantsAndSelf(this DependencyObject reference, string descendantName)
        {
            return DescendantsAndSelf<FrameworkElement>(reference, descendantName);
        }

        public static T FirstDescendantOrSelf<T>(this DependencyObject reference, string descendantName)
            where T : FrameworkElement
        {
            return DescendantsAndSelf<T>(reference, descendantName).FirstOrDefault();
        }

        public static IEnumerable<T> DescendantsAndSelf<T>(this DependencyObject reference, string descendantName)
            where T : FrameworkElement
        {
            return DescendantsAndSelf<T>(reference).Where(item => item.Name == descendantName);
        }

        public static T FirstDescendantOrSelf<T>(this DependencyObject reference)
            where T : DependencyObject
        {
            return DescendantsAndSelf<T>(reference).FirstOrDefault();
        }

        public static IEnumerable<T> DescendantsAndSelf<T>(this DependencyObject reference)
            where T : DependencyObject
        {
            return DescendantsAndSelf(reference).OfType<T>();
        }

        public static DependencyObject FirstDescendantOrSelf(this DependencyObject reference)
        {
            return DescendantsAndSelf(reference).FirstOrDefault();
        }

        public static IEnumerable<DependencyObject> DescendantsAndSelf(this DependencyObject reference)
        {
            if(reference == null)
                throw new ArgumentNullException(nameof(reference));
            var searchQueue = new Queue<DependencyObject>(10);
            searchQueue.Enqueue(reference);
            while(searchQueue.Count != 0)
            {
                var currentSearching = searchQueue.Dequeue();
                yield return currentSearching;
                foreach(var item in currentSearching.Children())
                {
                    searchQueue.Enqueue(item);
                }
            }
        }

        #endregion DescendantsAndSelf

        #region AncestorsAndSelf

        public static FrameworkElement FirstAncestorOrSelf(this DependencyObject reference, string ancestorName)
        {
            return FirstAncestorOrSelf<FrameworkElement>(reference, ancestorName);
        }

        public static IEnumerable<FrameworkElement> AncestorsAndSelf(this DependencyObject reference, string ancestorName)
        {
            return AncestorsAndSelf<FrameworkElement>(reference, ancestorName);
        }

        public static T FirstAncestorOrSelf<T>(this DependencyObject reference, string ancestorName)
            where T : FrameworkElement
        {
            return AncestorsAndSelf<T>(reference, ancestorName).FirstOrDefault();
        }

        public static IEnumerable<T> AncestorsAndSelf<T>(this DependencyObject reference, string ancestorName)
            where T : FrameworkElement
        {
            return AncestorsAndSelf<T>(reference).Where(item => item.Name == ancestorName);
        }

        public static T FirstAncestorOrSelf<T>(this DependencyObject reference)
            where T : DependencyObject
        {
            return AncestorsAndSelf<T>(reference).FirstOrDefault();
        }

        public static IEnumerable<T> AncestorsAndSelf<T>(this DependencyObject reference)
            where T : DependencyObject
        {
            return AncestorsAndSelf(reference).OfType<T>();
        }

        public static IEnumerable<DependencyObject> AncestorsAndSelf(this DependencyObject reference)
        {
            if(reference == null)
                throw new ArgumentNullException(nameof(reference));
            do
            {
                yield return reference;
            } while((reference = GetParent(reference)) != null);
        }

        #endregion AncestorsAndSelf
    }
}
