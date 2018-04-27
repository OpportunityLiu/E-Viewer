using ExClient;
using ExClient.Galleries;
using ExViewer.Controls;
using ExViewer.ViewModels;
using Opportunity.Helpers.Universal.AsyncHelpers;
using Opportunity.MvvmUniverse.Services;
using Opportunity.MvvmUniverse.Services.Navigation;
using Opportunity.MvvmUniverse.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
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
    public sealed partial class FavoritesPage : MvvmPage, IHasAppBar, INavigationHandler
    {
        public FavoritesPage()
        {
            this.InitializeComponent();
            var l = new List<FavoriteCategory>(11)
            {
                FavoriteCategory.All
            };
            l.AddRange(Client.Current.Favorites);
            this.cbCategory.ItemsSource = l;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            TagSuggestionService.IncreaseStateCode(this.asb);
            Navigator.GetForCurrentView().Handlers.Add(this);
            base.OnNavigatedTo(e);
            this.ViewModel = FavoritesVM.GetVM(e.Parameter?.ToString());
            this.ViewModel.SetQueryWithSearchResult();
            this.ViewModel.Search.Executed += this.Search_Executed;
            await Dispatcher.YieldIdle();
            if (e.NavigationMode == NavigationMode.New)
            {
                if (e.Parameter != null)
                    this.ViewModel.SearchResult.Reset();
                this.cbCategory.Focus(FocusState.Programmatic);
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                if (!await ViewHelper.ScrollAndFocus(this.lv, this.ViewModel.SelectedGallery))
                    this.cbCategory.Focus(FocusState.Programmatic);
            }
        }

        private void Search_Executed(Opportunity.MvvmUniverse.Commands.ICommand<string> sender, Opportunity.MvvmUniverse.Commands.ExecutedEventArgs<string> e) => CloseAll();

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            CloseAll();
            this.GetNavigator().Handlers.Remove(this);
        }

        private void lv_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.ViewModel.Open.Execute((Gallery)e.ClickedItem);
        }

        public new FavoritesVM ViewModel
        {
            get => (FavoritesVM)base.ViewModel;
            set => base.ViewModel = value;
        }

        private void lv_RefreshRequested(object sender, EventArgs e)
        {
            this.ViewModel?.SearchResult.Reset();
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
            if (this.cbActions is null)
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
            this.GetNavigator().UpdateProperties();
            ElementSoundPlayer.Play(ElementSoundKind.Invoke);
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
            this.GetNavigator().UpdateProperties();
            ElementSoundPlayer.Play(ElementSoundKind.GoBack);
            return true;
        }


        void IServiceHandler<Navigator>.OnAdd(Navigator service) { }
        void IServiceHandler<Navigator>.OnRemove(Navigator service) { }

        public bool CanGoBack => (this.lv.SelectionMode != ListViewSelectionMode.None);
        public IAsyncOperation<bool> GoBackAsync()
        {
            if (!CanGoBack)
                return AsyncOperation<bool>.CreateCompleted(false);
            exitSelectMode();
            return AsyncOperation<bool>.CreateCompleted(true);
        }

        bool INavigationHandler.CanGoForward => false;

        IAsyncOperation<bool> INavigationHandler.GoForwardAsync()
            => AsyncOperation<bool>.CreateCompleted(false);

        IAsyncOperation<bool> INavigationHandler.NavigateAsync(Type sourcePageType, object parameter)
            => AsyncOperation<bool>.CreateCompleted(false);

        private void lv_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            var r = startSelectMode();
            if (!r)
                return;

            args.Handled = true;
            var item = ((DependencyObject)args.OriginalSource).AncestorsAndSelf<ListViewItem>().FirstOrDefault();
            if (item != null)
            {
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
            var task = this.ViewModel.SearchResult.AddToCategoryAsync(this.lv.SelectedRanges.ToList(), cat);
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
    }
}
