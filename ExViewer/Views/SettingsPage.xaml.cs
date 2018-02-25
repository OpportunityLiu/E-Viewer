using ExViewer.Controls;
using ExViewer.Settings;
using System;
using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : MyPage
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            this.VisibleBoundHandledByDesign = true;
            this.pv_root.ItemsSource = SettingCollection.Current.GroupedSettings;
        }

        private Stack<int> navigateStack = new Stack<int>();

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await Dispatcher.YieldIdle();
            switch (e.NavigationMode)
            {
            case NavigationMode.New:
                this.pv_root.SelectedIndex = 0;
                break;
            case NavigationMode.Back:
                this.pv_root.SelectedIndex = this.navigateStack.Pop();
                break;
            }
            this.pv_root.Focus(FocusState.Programmatic);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            switch (e.NavigationMode)
            {
            case NavigationMode.New:
            case NavigationMode.Forward:
                this.navigateStack.Push(this.pv_root.SelectedIndex);
                break;
            }
        }

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
