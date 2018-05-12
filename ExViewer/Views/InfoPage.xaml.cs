using ExViewer.Controls;
using ExViewer.ViewModels;
using Opportunity.MvvmUniverse.Views;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;


// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class InfoPage : MvvmPage, IHasAppBar
    {
        public InfoPage()
        {
            this.InitializeComponent();
            this.ViewModel = new InfoVM();
            this.ViewModel.RefreshStatus.Execute();
            this.ViewModel.RefreshTaggingStatistics.Execute();
        }

        private double percent(double value)
        {
            if (double.IsNaN(value))
            {
                return 0;
            }

            return value;
        }

        public new InfoVM ViewModel
        {
            get => (InfoVM)base.ViewModel;
            set => base.ViewModel = value;
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

        private void pv_root_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (this.pv_root.SelectedIndex)
            {
            case 0:
                this.abbRefresh.Command = ViewModel.RefreshStatus;
                break;
            case 1:
                this.abbRefresh.Command = ViewModel.RefreshTaggingStatistics;
                break;
            }
        }

        private async void abbChangeUser_Click(object sender, RoutedEventArgs e)
        {
            if (await RootControl.RootController.RequestLogOn())
            {
                this.ViewModel.RefreshStatus.Execute();
                this.ViewModel.RefreshTaggingStatistics.Execute();
            }
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = true;
            switch (e.Key)
            {
            case Windows.System.VirtualKey.GamepadY:
                e.Handled = false;
                break;
            case Windows.System.VirtualKey.GamepadMenu:
            case Windows.System.VirtualKey.Application:
                this.cb.IsOpen = true;
                this.cb.Focus(FocusState.Programmatic);
                break;
            default:
                e.Handled = false;
                break;
            }
        }

        public void CloseAll()
        {
            this.cb.IsOpen = false;
        }

        private void lvTagging_ItemClick(object sender, ItemClickEventArgs e)
        {
            foreach (var item in this.mfTaggingRecord.Items)
            {
                if (item is MenuFlyoutItem mfi)
                {
                    mfi.CommandParameter = e.ClickedItem;
                }
            }
            this.mfTaggingRecord.ShowAt(this.lvTagging.ContainerFromItem(e.ClickedItem).Cast<FrameworkElement>());
        }
    }
}
