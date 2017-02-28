using ExClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ExClient.Settings;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Controls
{
    public sealed partial class ExcludedTagNamespacesSelector : UserControl
    {
        public ExcludedTagNamespacesSelector()
        {
            this.InitializeComponent();
        }

        public Namespace ExcludedTagNamespaces
        {
            get
            {
                return (Namespace)GetValue(ExcludedTagNamespacesProperty);
            }
            set
            {
                SetValue(ExcludedTagNamespacesProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for ExcludedTagNamespaces.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExcludedTagNamespacesProperty =
            DependencyProperty.Register("ExcludedTagNamespaces", typeof(Namespace), typeof(ExcludedTagNamespacesSelector), new PropertyMetadata(Namespace.Misc, ExcludedTagNamespacesChanged));

        private static void ExcludedTagNamespacesChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var s = (ExcludedTagNamespacesSelector)sender;
            var nv = (Namespace)e.NewValue;
            s.refresh(nv);
        }

        private void refresh(Namespace value)
        {
            this.changing = true;
            foreach(Namespace item in this.gv.Items)
            {
                var n = value.HasFlag(item);
                var cb = ((CheckBox)((GridViewItem)this.gv.ContainerFromItem(item))?.ContentTemplateRoot);
                if(cb != null)
                    cb.IsChecked = n;
            }
            this.changing = false;
        }

        private bool changing;

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(this.changing)
                return;
            var v = (Namespace)((FrameworkElement)sender).DataContext;
            this.ExcludedTagNamespaces |= v;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if(this.changing)
                return;
            var v = (Namespace)((FrameworkElement)sender).DataContext;
            this.ExcludedTagNamespaces &= ~v;
        }

        private void CheckBox_Loaded(object sender, RoutedEventArgs e)
        {
            var cb = (CheckBox)sender;
            cb.IsChecked = this.ExcludedTagNamespaces.HasFlag((Namespace)cb.DataContext);
        }
    }
}
