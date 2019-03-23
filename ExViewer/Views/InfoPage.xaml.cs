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
            InitializeComponent();
            HistoryIconConverter.Converter = new HistoryToIconConverter();
            ViewModel = new InfoVM();
            refreshAll();
        }

        private void refreshAll()
        {
            ViewModel.RefreshStatus.Execute();
            ViewModel.RefreshTaggingStatistics.Execute();
            ViewModel.RefreshHistory.Execute();
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
            setSplitViewButtonPlaceholderVisibility(null, RootControl.RootController.SplitViewButtonPlaceholderVisibility);
            RootControl.RootController.SplitViewButtonPlaceholderVisibilityChanged += setSplitViewButtonPlaceholderVisibility;
        }

        private void page_Unloaded(object sender, RoutedEventArgs e)
        {
            RootControl.RootController.SplitViewButtonPlaceholderVisibilityChanged -= setSplitViewButtonPlaceholderVisibility;
        }

        private void setSplitViewButtonPlaceholderVisibility(RootControl sender, bool visible)
        {
            bdSplitViewPlaceholder.Width = visible ? 48 : 0;
        }

        private void pv_root_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (pv_root.SelectedIndex)
            {
            case 0:
                abbRefresh.Command = ViewModel.RefreshStatus;
                break;
            case 1:
                abbRefresh.Command = ViewModel.RefreshTaggingStatistics;
                break;
            case 2:
                abbRefresh.Command = ViewModel.RefreshHistory;
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
                cb.IsOpen = true;
                cb.Focus(FocusState.Programmatic);
                break;
            default:
                e.Handled = false;
                break;
            }
        }

        public void CloseAll()
        {
            cb.IsOpen = false;
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
            setCommandParameter(mfTaggingRecord, e.ClickedItem);
            mfTaggingRecord.ShowAt(lvTagging.ContainerFromItem(e.ClickedItem).Cast<FrameworkElement>());
        }

        private void lvHistory_ItemClick(object sender, ItemClickEventArgs e)
        {
            setCommandParameter(mfHistoryRecord, e.ClickedItem);
            mfHistoryRecord.ShowAt(lvHistory.ContainerFromItem(e.ClickedItem).Cast<FrameworkElement>());
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
