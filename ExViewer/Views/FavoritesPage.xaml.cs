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
            InitializeComponent();
            var l = new List<FavoriteCategory>(11)
            {
                Client.Current.Favorites.All
            };
            l.AddRange(Client.Current.Favorites);
            cbCategory.ItemsSource = l;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            TagSuggestionService.IncreaseStateCode(asb);
            Navigator.GetForCurrentView().Handlers.Add(this);
            base.OnNavigatedTo(e);
            ViewModel = FavoritesVM.GetVM(e.Parameter?.ToString());
            ViewModel.SetQueryWithSearchResult();
            ViewModel.Search.Executed += Search_Executed;
            await Dispatcher.YieldIdle();
            if (e.NavigationMode == NavigationMode.New)
            {
                if (e.Parameter != null)
                {
                    ViewModel.SearchResult.Reset();
                }

                cbCategory.Focus(FocusState.Programmatic);
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                if (!await ViewHelper.ScrollAndFocus(lv, ViewModel.SelectedGallery))
                {
                    cbCategory.Focus(FocusState.Programmatic);
                }
            }
        }

        private void Search_Executed(Opportunity.MvvmUniverse.Commands.ICommand<string> sender, Opportunity.MvvmUniverse.Commands.ExecutedEventArgs<string> e) => CloseAll();

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            CloseAll();
            Navigator.GetForCurrentView().Handlers.Remove(this);
        }

        private void lv_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.Open.Execute((Gallery)e.ClickedItem);
        }

        public new FavoritesVM ViewModel
        {
            get => (FavoritesVM)base.ViewModel;
            set => base.ViewModel = value;
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
                if (lv.SelectionMode == ListViewSelectionMode.None)
                {
                    cbCategory.Focus(FocusState.Keyboard);
                }
                else
                {
                    cbCategory2.Focus(FocusState.Keyboard);
                }

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
            asb.IsSuggestionListOpen = false;
            cbCategory.IsDropDownOpen = false;
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

        private bool startSelectMode()
        {
            if (!lv.IsItemClickEnabled)
            {
                return false;
            }

            lv.SelectionMode = ListViewSelectionMode.Multiple;
            lv.IsItemClickEnabled = false;
            if (cbActions is null)
            {
                FindName(nameof(cbActions));
                var l = new List<FavoriteCategory>(11)
                {
                    Client.Current.Favorites.Removed
                };
                l.AddRange(Client.Current.Favorites);
                cbCategory2.ItemsSource = l;
            }
            cbCategory2.SelectedIndex = cbCategory.SelectedIndex;
            cbActions.Visibility = Visibility.Visible;
            cbCategory.Visibility = Visibility.Collapsed;
            asb.Visibility = Visibility.Collapsed;
            Navigator.GetForCurrentView().UpdateProperties();
            ElementSoundPlayer.Play(ElementSoundKind.Invoke);
            return true;
        }

        private bool exitSelectMode()
        {
            if (lv.IsItemClickEnabled)
            {
                return false;
            }

            lv.SelectionMode = ListViewSelectionMode.None;
            lv.IsItemClickEnabled = true;
            cbActions.Visibility = Visibility.Collapsed;
            cbCategory.Visibility = Visibility.Visible;
            asb.Visibility = Visibility.Visible;
            Navigator.GetForCurrentView().UpdateProperties();
            ElementSoundPlayer.Play(ElementSoundKind.GoBack);
            return true;
        }


        void IServiceHandler<Navigator>.OnAdd(Navigator service) { }
        void IServiceHandler<Navigator>.OnRemove(Navigator service) { }

        public bool CanGoBack => (lv.SelectionMode != ListViewSelectionMode.None);
        public IAsyncOperation<bool> GoBackAsync()
        {
            if (!CanGoBack)
            {
                return AsyncOperation<bool>.CreateCompleted(false);
            }

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
            {
                return;
            }

            args.Handled = true;
            var item = ((DependencyObject)args.OriginalSource).AncestorsAndSelf<ListViewItem>().FirstOrDefault();
            if (item != null)
            {
                var i = lv.ItemFromContainer(item);
                lv.SelectedItem = i;
            }
        }

        private void lv_ContextCanceled(UIElement sender, RoutedEventArgs args)
        {
        }

        private void abbAll_Click(object sender, RoutedEventArgs e)
        {
            lv.SelectRange(new ItemIndexRange(0, uint.MaxValue));
        }

        private void abbClear_Click(object sender, RoutedEventArgs e)
        {
            lv.DeselectRange(new ItemIndexRange(0, uint.MaxValue));
        }

        private void abbApply_Click(object sender, RoutedEventArgs e)
        {
            var cat = (FavoriteCategory)cbCategory2.SelectedItem ?? Client.Current.Favorites.Removed;
            var task = ViewModel.SearchResult.AddToCategoryAsync(lv.SelectedRanges.ToList(), cat);
            lv.SelectionMode = ListViewSelectionMode.None;
            RootControl.RootController.TrackAsyncAction(task.AsAsyncAction(), (s, args) =>
            {
                if (args == AsyncStatus.Completed)
                {
                    exitSelectMode();
                }
                else
                {
                    RootControl.RootController.SendToast(s.ErrorCode, GetType());
                    lv.SelectionMode = ListViewSelectionMode.Multiple;
                }
            });
        }

        private void cbActions_Opening(object sender, object e)
        {
            Grid.SetColumn(cbCategory2, 0);
        }

        private void cbActions_Closed(object sender, object e)
        {
            Grid.SetColumn(cbCategory2, 1);
        }
    }
}
