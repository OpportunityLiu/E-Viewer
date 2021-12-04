using ExClient.Tagging;
using Opportunity.Helpers;
using Opportunity.Helpers.ObjectModel;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Controls
{
    public sealed partial class ExcludedTagNamespacesSelector : UserControl
    {
        public ExcludedTagNamespacesSelector()
        {
            InitializeComponent();
            foreach (var item in EnumExtension.GetDefinedValues<Namespace>())
            {
                if (item.Value == Namespace.Unknown || item.Value >= Namespace.Temp)
                {
                    continue;
                }

                gv.Items.Add(new Box<Namespace> { Value = item.Value });
            }
        }

        public Namespace ExcludedTagNamespaces
        {
            get => (Namespace)GetValue(ExcludedTagNamespacesProperty);
            set => SetValue(ExcludedTagNamespacesProperty, value);
        }

        // Using a DependencyProperty as the backing store for ExcludedTagNamespaces.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExcludedTagNamespacesProperty =
            DependencyProperty.Register("ExcludedTagNamespaces", typeof(Namespace), typeof(ExcludedTagNamespacesSelector), new PropertyMetadata(Namespace.Temp, ExcludedTagNamespacesChanged));

        private static void ExcludedTagNamespacesChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var s = (ExcludedTagNamespacesSelector)sender;
            var nv = (Namespace)e.NewValue;
            s.refresh(nv);
        }

        private void refresh(Namespace value)
        {
            changing = true;
            foreach (var item in gv.Items.Cast<IBox<Namespace>>())
            {
                var n = value.HasFlag(item.Value);
                var cb = ((CheckBox)((GridViewItem)gv.ContainerFromItem(item))?.ContentTemplateRoot);
                if (cb != null)
                {
                    cb.IsChecked = n;
                }
            }
            changing = false;
        }

        private bool changing;

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (changing)
            {
                return;
            }

            var v = ((IBox<Namespace>)((FrameworkElement)sender).DataContext).Value;
            ExcludedTagNamespaces |= v;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (changing)
            {
                return;
            }

            var v = ((IBox<Namespace>)((FrameworkElement)sender).DataContext).Value;
            ExcludedTagNamespaces &= ~v;
        }

        private void CheckBox_Loaded(object sender, RoutedEventArgs e)
        {
            var cb = (CheckBox)sender;
            cb.IsChecked = ExcludedTagNamespaces.HasFlag(((IBox<Namespace>)cb.DataContext).Value);
        }
    }
}
