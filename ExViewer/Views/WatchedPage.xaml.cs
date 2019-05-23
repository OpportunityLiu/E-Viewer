using ExClient.Galleries;
using ExViewer.Controls;
using ExViewer.ViewModels;
using Opportunity.MvvmUniverse.Views;
using System;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class WatchedPage : MvvmPage, IHasAppBar
    {
        public WatchedPage()
        {
            InitializeComponent();
        }

        private Button _btnExpandButton;
        private Button btnExpandButton => System.Threading.LazyInitializer.EnsureInitialized(ref _btnExpandButton, () => ab.Descendants<Button>("ExpandButton").FirstOrDefault());

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            TagSuggestionService.IncreaseStateCode(asb);
            base.OnNavigatedTo(e);
            ViewModel = WatchedVM.GetVM(e.Parameter?.ToString());
            ViewModel.SetQueryWithSearchResult();
            ViewModel.Search.Executed += Search_Executed;
            await Dispatcher.YieldIdle();
            if (e.NavigationMode == NavigationMode.New)
            {
                if (e.Parameter != null) // for the pre-load page
                {
                    ViewModel.SearchResult.Reset();
                }

                btnExpandButton?.Focus(FocusState.Programmatic);
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                if (!await ViewHelper.ScrollAndFocus(lv, ViewModel.SelectedGallery))
                {
                    btnExpandButton?.Focus(FocusState.Programmatic);
                }
            }
        }

        private void Search_Executed(Opportunity.MvvmUniverse.Commands.ICommand<string> sender, Opportunity.MvvmUniverse.Commands.ExecutedEventArgs<string> e) =>
            CloseAll();

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        private void lv_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.Open.Execute((Gallery)e.ClickedItem);
        }

        public new WatchedVM ViewModel
        {
            get => (WatchedVM)base.ViewModel;
            set => base.ViewModel = value;
        }

        private void ab_Opening(object sender, object e)
        {
            sv_AdvancedSearch.IsEnabled = true;
            Grid.SetColumn(asb, 0);
        }

        private void ab_Closed(object sender, object e)
        {
            sv_AdvancedSearch.IsEnabled = false;
            Grid.SetColumn(asb, 1);
        }

        private void lv_RefreshRequested(object sender, EventArgs e)
        {
            ViewModel?.SearchResult.Reset();
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = true;
            switch (e.Key)
            {
            case Windows.System.VirtualKey.GamepadY:
                asb.Focus(FocusState.Keyboard);
                break;
            case Windows.System.VirtualKey.GamepadMenu:
            case Windows.System.VirtualKey.Application:
                ab.IsOpen = !ab.IsOpen;
                break;
            default:
                e.Handled = false;
                break;
            }
        }

        public void CloseAll()
        {
            asb.IsSuggestionListOpen = false;
            ab.IsOpen = false;
        }

        public void SetSplitViewButtonPlaceholderVisibility(RootControl sender, bool visible)
        {
            if (visible)
            {
                cdSplitViewPlaceholder.Width = new GridLength(48);
            }
            else
            {
                cdSplitViewPlaceholder.Width = new GridLength(0);
            }
        }

        private void root_Loading(FrameworkElement sender, object args)
        {
            SetSplitViewButtonPlaceholderVisibility(null, RootControl.RootController.SplitViewButtonPlaceholderVisibility);
            RootControl.RootController.SplitViewButtonPlaceholderVisibilityChanged += SetSplitViewButtonPlaceholderVisibility;
        }

        private void root_Unloaded(object sender, RoutedEventArgs e)
        {
            RootControl.RootController.SplitViewButtonPlaceholderVisibilityChanged -= SetSplitViewButtonPlaceholderVisibility;
        }

        private FileSearchDialog dlgFileSearch;

        private async void btnFileSearch_Click(object sender, RoutedEventArgs e)
        {
            if (dlgFileSearch is null)
            {
                dlgFileSearch = new FileSearchDialog();
            }

            CloseAll();
            await dlgFileSearch.ShowAsync();
        }

        private double caculateGdAbMaxHeight(Thickness visibleBounds, double rootHeight)
        {
            if (rootHeight <= 50)
            {
                return double.PositiveInfinity;
            }

            var r = rootHeight - visibleBounds.Top - visibleBounds.Bottom;
            if (r <= 0)
            {
                return double.PositiveInfinity;
            }

            return r;
        }
    }
}
