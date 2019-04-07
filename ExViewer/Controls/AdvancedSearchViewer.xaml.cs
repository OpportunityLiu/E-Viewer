using ExClient.Search;
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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace ExViewer.Controls
{
    public sealed partial class AdvancedSearchViewer : UserControl
    {
        public AdvancedSearchViewer()
        {
            InitializeComponent();
        }

        public AdvancedSearchOptions Data
        {
            get => (AdvancedSearchOptions)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        /// <summary>
        /// Indentify <see cref="Data"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(AdvancedSearchOptions), typeof(AdvancedSearchViewer), new PropertyMetadata(null));
    }
}
