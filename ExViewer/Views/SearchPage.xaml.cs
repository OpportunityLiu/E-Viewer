using ExClient.Galleries;
using ExViewer.Controls;
using ExViewer.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
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

        private int navId;
        private Button btnExpandButton;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.navId++;
            this.VM = SearchVM.GetVM(e.Parameter?.ToString());
            if (e.NavigationMode == NavigationMode.New)
            {
                if (e.Parameter != null)
                    this.VM?.SearchResult.Reset();
                if (this.btnExpandButton == null)
                    this.btnExpandButton = this.ab.Descendants<Button>("ExpandButton").FirstOrDefault();
                await Dispatcher.YieldIdle();
                this.btnExpandButton?.Focus(FocusState.Programmatic);
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                var selectedGallery = this.VM.SelectedGallery;
                if (selectedGallery != null)
                {
                    await Dispatcher.YieldIdle();
                    this.lv.ScrollIntoView(selectedGallery);
                    ((Control)this.lv.ContainerFromItem(selectedGallery))?.Focus(FocusState.Programmatic);
                }
            }
            this.VM.SetQueryWithSearchResult();
            await ExClient.Client.Current.UserStatus.RefreshAsync();
        }

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

        private async void asb_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var needAutoComplete = args.Reason == AutoSuggestionBoxTextChangeReason.UserInput;
            var currentId = this.navId;
            if (needAutoComplete)
            {
                var r = await this.VM.LoadSuggestion(sender.Text);
                if (args.CheckCurrent() && currentId == this.navId)
                {
                    this.asb.ItemsSource = r;
                }
            }
        }

        private async void asb_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            sender.ItemsSource = null;
            if (args.ChosenSuggestion == null || this.VM.AutoCompleteFinished(args.ChosenSuggestion))
            {
                CloseAll();
                this.VM.Search.Execute(args.QueryText);
            }
            else
            {
                this.asb.Focus(FocusState.Keyboard);
                // workaround for IME candidates, which will clean input.
                await Dispatcher.YieldIdle();
                this.asb.Text = args.ChosenSuggestion.ToString();
            }
        }

        private void asb_LostFocus(object sender, RoutedEventArgs e)
        {
            this.asb.ItemsSource = null;
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
    }
}
