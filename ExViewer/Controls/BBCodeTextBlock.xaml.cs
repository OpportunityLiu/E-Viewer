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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Controls
{
    public sealed partial class BBCodeTextBlock : UserControl
    {
        public BBCodeTextBlock()
        {
            this.InitializeComponent();
        }

        public Style TextBlockStyle
        {
            get
            {
                return (Style)GetValue(TextBlockStyleProperty);
            }
            set
            {
                SetValue(TextBlockStyleProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for TextBlockStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextBlockStyleProperty =
            DependencyProperty.Register("TextBlockStyle", typeof(Style), typeof(BBCodeTextBlock), new PropertyMetadata(null));

        public string BBCode
        {
            get
            {
                return (string)GetValue(BBCodeProperty);
            }
            set
            {
                SetValue(BBCodeProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for BBCode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BBCodeProperty =
            DependencyProperty.Register("BBCode", typeof(string), typeof(BBCodeTextBlock), new PropertyMetadata("", BBCodePropertyChanged));

        public static void BBCodePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue == null)
                throw new ArgumentNullException(nameof(BBCode));
            ((BBCodeTextBlock)sender).loadBBCode(e.NewValue.ToString());
        }

        private void loadBBCode(string bbCode)
        {
            Content.Text = bbCode;
        }
    }
}
