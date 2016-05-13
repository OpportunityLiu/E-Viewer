using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace ExViewer.Controls
{
    class SplitViewTab : SplitViewButton
    {
        public SplitViewTab()
        {
            this.DefaultStyleKey = typeof(SplitViewTab);
        }

        public bool IsChecked
        {
            get
            {
                return (bool)GetValue(IsCheckedProperty);
            }
            set
            {
                SetValue(IsCheckedProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for IsChecked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(SplitViewTab), new PropertyMetadata(false, OnIsCheckedChangedStatic));

        private static void OnIsCheckedChangedStatic(object sender, DependencyPropertyChangedEventArgs args)
        {
            if(args.OldValue == args.NewValue)
                return;
            var s = (SplitViewTab)sender;
            s.updateState(true);
            s.OnIsCheckedChanged(args);
        }

        protected virtual void OnIsCheckedChanged(DependencyPropertyChangedEventArgs args)
        {
        }

        private void updateState(bool animate)
        {
            VisualStateManager.GoToState(this, this.IsChecked ? "Checked" : "Unchecked", animate);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            updateState(false);
        }
    }
}
