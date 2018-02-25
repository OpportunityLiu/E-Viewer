using System;
using Windows.UI.Xaml;

namespace ExViewer.Controls
{
    public static class InAppNavigator
    {
        public static Uri GetInAppUri(DependencyObject obj)
        {
            return (Uri)obj.GetValue(InAppUriProperty);
        }

        public static void SetInAppUri(DependencyObject obj, Uri value)
        {
            obj.SetValue(InAppUriProperty, value);
        }

        // Using a DependencyProperty as the backing store for InAppUri.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InAppUriProperty =
            DependencyProperty.RegisterAttached("InAppUri", typeof(Uri), typeof(DependencyObject), new PropertyMetadata(null));
    }
}
