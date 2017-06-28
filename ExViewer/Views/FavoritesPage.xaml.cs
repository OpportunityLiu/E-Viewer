using ExClient;
using ExClient.Galleries;
using ExViewer.Controls;
using ExViewer.ViewModels;
using Opportunity.MvvmUniverse.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FavoritesPage : MyPage, IHasAppBar, INavigationHandler
    {
        public FavoritesPage()
        {
            this.InitializeComponent();
            this.VisibleBoundHandledByDesign = true;
            var l = new List<FavoriteCategory>(11)
            {
                FavoriteCategory.All
            };
            l.AddRange(Client.Current.Favorites);
            this.cbCategory.ItemsSource = l;
            this.submitSearchCmd = new Opportunity.MvvmUniverse.Commands.Command<string>(submitSearch);
        }

        private Opportunity.MvvmUniverse.Commands.Command<string> submitSearchCmd;

        private void submitSearch(string text)
        {
            CloseAll();
            this.VM.Search.Execute(text);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var id = TagSuggestionService.GetStateCode(this.asb);
            TagSuggestionService.SetStateCode(this.asb, id + 1);
            Navigator.GetForCurrentView().Handlers.Add(this);
            base.OnNavigatedTo(e);
            this.VM = FavoritesVM.GetVM(e.Parameter?.ToString());
            this.VM.SetQueryWithSearchResult();
            if (e.NavigationMode == NavigationMode.New || this.VM.SelectedGallery == null)
            {
                if (e.Parameter != null)
                    this.VM.SearchResult.Reset();
                await Dispatcher.YieldIdle();
                this.cbCategory.Focus(FocusState.Programmatic);
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                var selectedGallery = this.VM.SelectedGallery;
                this.lv.ScrollIntoView(selectedGallery);
                await Dispatcher.YieldIdle();
                this.lv.ScrollIntoView(selectedGallery);
                var con = (Control)this.lv.ContainerFromItem(selectedGallery);
                if (con == null)
                    this.cbCategory.Focus(FocusState.Programmatic);
                else
                    con.Focus(FocusState.Programmatic);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            CloseAll();
            Navigator.GetForCurrentView().Handlers.Remove(this);
        }

        private void lv_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.VM.Open.Execute((Gallery)e.ClickedItem);
        }

        public FavoritesVM VM
        {
            get => (FavoritesVM)GetValue(VMProperty);
            set => SetValue(VMProperty, value);
        }


        // Using a DependencyProperty as the backing store for VM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty =
            DependencyProperty.Register("VM", typeof(FavoritesVM), typeof(FavoritesPage), new PropertyMetadata(null));

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
                if (this.lv.SelectionMode == ListViewSelectionMode.None)
                    this.cbCategory.Focus(FocusState.Keyboard);
                else
                    this.cbCategory2.Focus(FocusState.Keyboard);
                break;
            case Windows.System.VirtualKey.GamepadMenu:
            case Windows.System.VirtualKey.Application:
                e.Handled = false;
                break;
            default:
                e.Handled = false;
                break;
            }
        }

        public void CloseAll()
        {
            exitSelectMode();
            this.asb.IsSuggestionListOpen = false;
            this.cbCategory.IsDropDownOpen = false;
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

        private bool startSelectMode()
        {
            if (!this.lv.IsItemClickEnabled)
                return false;
            this.lv.SelectionMode = ListViewSelectionMode.Multiple;
            this.lv.IsItemClickEnabled = false;
            if (this.cbActions == null)
            {
                this.FindName(nameof(this.cbActions));
                var l = new List<FavoriteCategory>(11)
                {
                    FavoriteCategory.Removed
                };
                l.AddRange(Client.Current.Favorites);
                this.cbCategory2.ItemsSource = l;
            }
            this.cbCategory2.SelectedIndex = this.cbCategory.SelectedIndex;
            this.cbActions.Visibility = Visibility.Visible;
            this.cbCategory.Visibility = Visibility.Collapsed;
            this.asb.Visibility = Visibility.Collapsed;
            this.RaiseCanGoBackChanged();
            return true;
        }

        private bool exitSelectMode()
        {
            if (this.lv.IsItemClickEnabled)
                return false;
            this.lv.SelectionMode = ListViewSelectionMode.None;
            this.lv.IsItemClickEnabled = true;
            this.cbActions.Visibility = Visibility.Collapsed;
            this.cbCategory.Visibility = Visibility.Visible;
            this.asb.Visibility = Visibility.Visible;
            this.RaiseCanGoBackChanged();
            return true;
        }

        public bool CanGoBack()
        {
            return (this.lv.SelectionMode != ListViewSelectionMode.None);
        }

        public void GoBack()
        {
            exitSelectMode();
        }

        private void lv_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            args.Handled = startSelectMode();
            var item = ((DependencyObject)args.OriginalSource).AncestorsAndSelf<ListViewItem>().FirstOrDefault();
            if (item != null)
            {
                args.Handled = true;
                var i = this.lv.ItemFromContainer(item);
                this.lv.SelectedItem = i;
            }
        }

        private void lv_ContextCanceled(UIElement sender, RoutedEventArgs args)
        {
        }

        private void abbAll_Click(object sender, RoutedEventArgs e)
        {
            this.lv.SelectRange(new ItemIndexRange(0, uint.MaxValue));
        }

        private void abbClear_Click(object sender, RoutedEventArgs e)
        {
            this.lv.DeselectRange(new ItemIndexRange(0, uint.MaxValue));
        }

        private void abbApply_Click(object sender, RoutedEventArgs e)
        {
            var cat = (FavoriteCategory)this.cbCategory2.SelectedItem ?? FavoriteCategory.Removed;
            var task = this.VM.SearchResult.AddToCategoryAsync(this.lv.SelectedRanges.ToList(), cat);
            this.lv.SelectionMode = ListViewSelectionMode.None;
            RootControl.RootController.TrackAsyncAction(task, (s, args) =>
            {
                if (args == Windows.Foundation.AsyncStatus.Completed)
                    exitSelectMode();
                else
                {
                    RootControl.RootController.SendToast(s.ErrorCode, this.GetType());
                    this.lv.SelectionMode = ListViewSelectionMode.Multiple;
                }
            });
        }

        private void cbActions_Opening(object sender, object e)
        {
            Grid.SetColumn(this.cbCategory2, 0);
        }

        private void cbActions_Closed(object sender, object e)
        {
            Grid.SetColumn(this.cbCategory2, 1);
        }

        Navigator INavigationHandler.Parent { get; set; }
    }
}
