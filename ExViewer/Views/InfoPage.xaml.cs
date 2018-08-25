using ExViewer.Controls;
using ExViewer.Database;
using ExViewer.ViewModels;
using Opportunity.MvvmUniverse.Views;
using Opportunity.UWP.Converters;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;


// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    internal sealed partial class InfoPage : MvvmPage, IHasAppBar
    {
        public InfoPage()
        {
            this.InitializeComponent();
            this.HistoryIconConverter.Converter = new HistoryToIconConverter();
            this.ViewModel = new InfoVM();
            refreshAll();
        }

        private void refreshAll()
        {
            this.ViewModel.RefreshStatus.Execute();
            this.ViewModel.RefreshTaggingStatistics.Execute();
            this.ViewModel.RefreshHistory.Execute();
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
            this.bdSplitViewPlaceholder.Width = visible ? 48 : 0;
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
            case 2:
                this.abbRefresh.Command = ViewModel.RefreshHistory;
                break;
            }
        }

        private async void abbChangeUser_Click(object sender, RoutedEventArgs e)
        {
            if (await RootControl.RootController.RequestLogOn())
            {
                refreshAll();
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

        private static void setCommandParameter(MenuFlyout target, object parameter)
        {
            set(target.Items);

            void set(IEnumerable<MenuFlyoutItemBase> collection)
            {
                foreach (var item in collection)
                {
                    switch (item)
                    {
                    case MenuFlyoutItem mfi:
                        mfi.CommandParameter = parameter;
                        break;
                    case MenuFlyoutSubItem msi:
                        set(msi.Items);
                        break;
                    }
                }
            }
        }

        private void lvTagging_ItemClick(object sender, ItemClickEventArgs e)
        {
            setCommandParameter(this.mfTaggingRecord, e.ClickedItem);
            this.mfTaggingRecord.ShowAt(this.lvTagging.ContainerFromItem(e.ClickedItem).Cast<FrameworkElement>());
        }

        private void lvHistory_ItemClick(object sender, ItemClickEventArgs e)
        {
            setCommandParameter(this.mfHistoryRecord, e.ClickedItem);
            this.mfHistoryRecord.ShowAt(this.lvHistory.ContainerFromItem(e.ClickedItem).Cast<FrameworkElement>());
        }

        private sealed class HistoryToIconConverter : ValueConverter<HistoryRecordType, Uri>
        {
            public override Uri Convert(HistoryRecordType value, object parameter, string language)
            {
                return new Uri($"ms-appx:///Assets/Jumplist/{value}.png");
            }

            public override HistoryRecordType ConvertBack(Uri value, object parameter, string language) => throw new NotImplementedException();
        }
    }
}
