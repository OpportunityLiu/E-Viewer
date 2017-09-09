using ExViewer.Controls;
using ExViewer.ViewModels;
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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class InfoPage : MyPage
    {
        public InfoPage()
        {
            this.InitializeComponent();
            this.VisibleBoundHandledByDesign = true;
            this.VM = new InfoVM();
            this.VM.RefreshStatus.Execute();
        }

        public InfoVM VM
        {
            get => (InfoVM)GetValue(VMProperty);
            set => SetValue(VMProperty, value);
        }

        /// <summary>
        /// Indentify <see cref="VM"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VMProperty =
            DependencyProperty.Register(nameof(VM), typeof(InfoVM), typeof(InfoPage), new PropertyMetadata(null));

        private void page_Loading(FrameworkElement sender, object args)
        {
            this.setSplitViewButtonPlaceholderVisibility(null, RootControl.RootController.SplitViewButtonPlaceholderVisibility);
            RootControl.RootController.SplitViewButtonPlaceholderVisibilityChanged += this.setSplitViewButtonPlaceholderVisibility;

        }

        private void page_Unloaded(object sender, RoutedEventArgs e)
        {
            RootControl.RootController.SplitViewButtonPlaceholderVisibilityChanged -= this.setSplitViewButtonPlaceholderVisibility;
        }

        private void setSplitViewButtonPlaceholderVisibility(RootControl sender, bool visible)
        {
            if (visible)
                this.bdSplitViewPlaceholder.Width = 48;
            else
                this.bdSplitViewPlaceholder.Width = 0;
        }
    }
}
