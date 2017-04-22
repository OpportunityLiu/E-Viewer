using ExClient;
using ExViewer.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Security.Credentials;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using ExViewer.ViewModels;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI;
using ExViewer.Controls;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FavoritesPage : MyPage, IHasAppBar
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
        }

        private int navId;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationManager.GetForCurrentView().BackRequested += this.FavoritesPage_BackRequested;
            this.navId++;
            this.VM = FavoritesVM.GetVM(e.Parameter?.ToString());
            if (e.NavigationMode == NavigationMode.New)
            {
                if (e.Parameter != null)
                    this.VM?.SearchResult.Reset();
                await Task.Delay(100);
                this.cbCategory.Focus(FocusState.Programmatic);
            }
            if (e.NavigationMode == NavigationMode.Back)
            {
                var selectedGallery = this.VM.SelectedGallery;
                if (selectedGallery != null)
                {
                    await Task.Delay(100);
                    this.lv.ScrollIntoView(selectedGallery);
                    ((Control)this.lv.ContainerFromItem(selectedGallery))?.Focus(FocusState.Programmatic);
                }
            }
            this.VM.SetQueryWithSearchResult();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            CloseAll();
            NavigationManager.GetForCurrentView().BackRequested -= this.FavoritesPage_BackRequested;
        }

        private void lv_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (this.VM.Open.CanExecute(e.ClickedItem))
                this.VM.Open.Execute(e.ClickedItem);
        }

        public FavoritesVM VM
        {
            get => (FavoritesVM)GetValue(VMProperty);
            set => SetValue(VMProperty, value);
        }

        // Using a DependencyProperty as the backing store for VM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty =
            DependencyProperty.Register("VM", typeof(FavoritesVM), typeof(FavoritesPage), new PropertyMetadata(null));

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
                await Dispatcher.RunIdleAsync(a => this.asb.Text = args.ChosenSuggestion.ToString());
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
                this.cbCategory.Focus(FocusState.Keyboard);
                break;
            case Windows.System.VirtualKey.GamepadMenu:
            case Windows.System.VirtualKey.Application:
                startSelectMode();
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
            InputPane.GetForCurrentView().TryHide();
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
            if (this.lv.SelectionMode == ListViewSelectionMode.Multiple)
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
            this.abbApply.IsEnabled = true;
            return true;
        }

        private bool exitSelectMode()
        {
            if (this.lv.SelectionMode == ListViewSelectionMode.None)
                return false;
            this.lv.SelectionMode = ListViewSelectionMode.None;
            this.lv.IsItemClickEnabled = true;
            this.cbActions.Visibility = Visibility.Collapsed;
            return true;
        }

        private void FavoritesPage_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            e.Handled = exitSelectMode();
        }

        private void lv_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            args.Handled = startSelectMode();
            if (!args.Handled)
                return;
            var item = ((DependencyObject)args.OriginalSource).FirstAncestorOrSelf<ListViewItem>();
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

        private async void abbApply_Click(object sender, RoutedEventArgs e)
        {
            this.abbApply.IsEnabled = false;
            var cat = (FavoriteCategory)this.cbCategory2.SelectedItem ?? FavoriteCategory.Removed;
            try
            {
                await this.VM.SearchResult.AddToCategoryAsync(this.lv.SelectedRanges, cat);
                exitSelectMode();
            }
            catch (Exception ex)
            {
                RootControl.RootController.SendToast(ex, this.GetType());
            }
            finally
            {
                this.abbApply.IsEnabled = true;
            }
        }
    }
}
