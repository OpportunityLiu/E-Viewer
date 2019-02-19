using ExViewer.Controls;
using ExViewer.Settings;
using Opportunity.MvvmUniverse.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : MvvmPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            pv_root.ItemsSource = SettingCollection.Current.GroupedSettings;
        }

        private async void SettingsPage_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            if (!(FocusManager.GetFocusedElement() is FrameworkElement dp))
                return;
            await Dispatcher.YieldIdle();
            dp.StartBringIntoView(new BringIntoViewOptions
            {
                AnimationDesired = true,
                TargetRect = new Windows.Foundation.Rect(0, 0, dp.ActualWidth, dp.ActualHeight + VisibleBounds.Bottom)
            });
        }

        private Stack<int> navigateStack = new Stack<int>();

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await Dispatcher.YieldIdle();
            InputPane.GetForCurrentView().Showing += SettingsPage_Showing;
            switch (e.NavigationMode)
            {
            case NavigationMode.New:
                pv_root.SelectedIndex = 0;
                break;
            case NavigationMode.Back:
                pv_root.SelectedIndex = navigateStack.Pop();
                break;
            }
            pv_root.Focus(FocusState.Programmatic);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            InputPane.GetForCurrentView().Showing -= SettingsPage_Showing;
            switch (e.NavigationMode)
            {
            case NavigationMode.New:
            case NavigationMode.Forward:
                navigateStack.Push(pv_root.SelectedIndex);
                break;
            }
        }

        private void page_Loading(FrameworkElement sender, object args)
        {
            setSplitViewButtonPlaceholderVisibility(null, RootControl.RootController.SplitViewButtonPlaceholderVisibility);
            RootControl.RootController.SplitViewButtonPlaceholderVisibilityChanged += setSplitViewButtonPlaceholderVisibility;

        }

        private void page_Unloaded(object sender, RoutedEventArgs e)
        {
            RootControl.RootController.SplitViewButtonPlaceholderVisibilityChanged -= setSplitViewButtonPlaceholderVisibility;
        }

        private void setSplitViewButtonPlaceholderVisibility(RootControl sender, bool visible)
        {
            if (visible)
            {
                bdSplitViewPlaceholder.Width = 48;
            }
            else
            {
                bdSplitViewPlaceholder.Width = 0;
            }
        }
    }
}
