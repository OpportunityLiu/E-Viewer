using ExClient.Galleries;
using ExViewer.Controls;
using ExViewer.ViewModels;
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
    public sealed partial class SearchPage : MyPage, IHasAppBar
    {
        public SearchPage()
        {
            this.InitializeComponent();
            this.VisibleBoundHandledByDesign = true;
        }

        private Button _btnExpandButton;
        private Button btnExpandButton => System.Threading.LazyInitializer.EnsureInitialized(ref this._btnExpandButton, () => this.ab.Descendants<Button>("ExpandButton").FirstOrDefault());

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            TagSuggestionService.IncreaseStateCode(this.asb);
            base.OnNavigatedTo(e);
            this.VM = SearchVM.GetVM(e.Parameter?.ToString());
            this.VM.SetQueryWithSearchResult();
            this.VM.Search.Executed += this.Search_Executed;
            await Dispatcher.YieldIdle();
            if (e.NavigationMode == NavigationMode.New)
            {
                if (e.Parameter != null) // for the pre-load page
                    this.VM.SearchResult.Reset();
                this.btnExpandButton?.Focus(FocusState.Programmatic);
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                if (!await ViewHelper.ScrollAndFocus(this.lv, this.VM.SelectedGallery))
                    this.btnExpandButton?.Focus(FocusState.Programmatic);
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
            this.VM.Open.Execute((Gallery)e.ClickedItem);
        }

        public SearchVM VM
        {
            get => (SearchVM)GetValue(VMProperty);
            set => SetValue(VMProperty, value);
        }

        // Using a DependencyProperty as the backing store for VM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty =
            DependencyProperty.Register("VM", typeof(SearchVM), typeof(SearchPage), new PropertyMetadata(null));

        private void ab_Opening(object sender, object e)
        {
            this.sv_AdvancedSearch.IsEnabled = true;
            Grid.SetColumn(this.asb, 0);
        }

        private void ab_Closed(object sender, object e)
        {
            this.sv_AdvancedSearch.IsEnabled = false;
            Grid.SetColumn(this.asb, 1);
        }

        private void lv_RefreshRequested(object sender, EventArgs e)
        {
            this.VM?.SearchResult.Reset();
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = true;
            switch (e.Key)
            {
            case Windows.System.VirtualKey.GamepadY:
                this.asb.Focus(FocusState.Keyboard);
                break;
            case Windows.System.VirtualKey.GamepadMenu:
            case Windows.System.VirtualKey.Application:
                this.ab.IsOpen = !this.ab.IsOpen;
                break;
            default:
                e.Handled = false;
                break;
            }
        }

        public void CloseAll()
        {
            this.asb.IsSuggestionListOpen = false;
            this.ab.IsOpen = false;
        }

        public void SetSplitViewButtonPlaceholderVisibility(RootControl sender, bool visible)
        {
            if (visible)
                this.cdSplitViewPlaceholder.Width = new GridLength(48);
            else
                this.cdSplitViewPlaceholder.Width = new GridLength(0);
        }

        private void root_Loading(FrameworkElement sender, object args)
        {
            this.SetSplitViewButtonPlaceholderVisibility(null, RootControl.RootController.SplitViewButtonPlaceholderVisibility);
            RootControl.RootController.SplitViewButtonPlaceholderVisibilityChanged += this.SetSplitViewButtonPlaceholderVisibility;
        }

        private void root_Unloaded(object sender, RoutedEventArgs e)
        {
            RootControl.RootController.SplitViewButtonPlaceholderVisibilityChanged -= this.SetSplitViewButtonPlaceholderVisibility;
        }

        private FileSearchDialog dlgFIleSearch;

        private async void btnFileSearch_Click(object sender, RoutedEventArgs e)
        {
            if (this.dlgFIleSearch == null)
                this.dlgFIleSearch = new FileSearchDialog();
            CloseAll();
            await this.dlgFIleSearch.ShowAsync();
        }

        private double caculateGdAbMaxHeight(Thickness visibleBounds, double rootHeight)
        {
            if (rootHeight <= 50)
                return double.PositiveInfinity;
            var r = rootHeight - visibleBounds.Top - visibleBounds.Bottom;
            if (r <= 0)
                return double.PositiveInfinity;
            return r;
        }
    }
}
