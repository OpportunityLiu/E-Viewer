using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Data.Json;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using ExClient;
using Windows.ApplicationModel.Activation;
using Windows.UI.ViewManagement;
using ExViewer.ViewModels;
using System.Diagnostics;
using System.Threading.Tasks;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RootControl : Page
    {
        public RootControl()
        {
            this.InitializeComponent();
            this.tabs = new Dictionary<Controls.SplitViewTab, Type>()
            {
                [this.svt_Saved] = typeof(SavedPage),
                [this.svt_Cached] = typeof(CachedPage),
                [this.svt_Search] = typeof(SearchPage),
                [this.svt_Favorites] = typeof(FavoritesPage),
                [this.svt_Settings] = typeof(SettingsPage)
            };

            this.pages = new Dictionary<Type, Controls.SplitViewTab>()
            {
                [typeof(CachedPage)] = this.svt_Cached,
                [typeof(SavedPage)] = this.svt_Saved,
                [typeof(SearchPage)] = this.svt_Search,
                [typeof(FavoritesPage)] = this.svt_Favorites,
                [typeof(SettingsPage)] = this.svt_Settings
            };
            this.sv_root.IsPaneOpen = false;
#if DEBUG
            this.GotFocus += this.OnGotFocus;
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            var focus = FocusManager.GetFocusedElement() as FrameworkElement;
            if(focus != null)
            {
                var c = focus as Control;
                Debug.WriteLine($"{focus.Name}({focus.GetType()}) {c?.FocusState}", "Focus state");
            }
#endif
        }

        public Type HomePageType
        {
            get; set;
        }

        public ApplicationExecutionState PreviousState
        {
            get; set;
        }

        private readonly Dictionary<Controls.SplitViewTab, Type> tabs;
        private readonly Dictionary<Type, Controls.SplitViewTab> pages;

        public UserInfo UserInfo
        {
            get => (UserInfo)GetValue(UserInfoProperty);
            set => SetValue(UserInfoProperty, value);
        }

        // Using a DependencyProperty as the backing store for UserInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserInfoProperty =
            DependencyProperty.Register("UserInfo", typeof(UserInfo), typeof(RootControl), new PropertyMetadata(null));

        SystemNavigationManager manager;

        private async void Control_Loading(FrameworkElement sender, object args)
        {
            RootController.SetRoot(this);
            this.manager = SystemNavigationManager.GetForCurrentView();
            this.manager.BackRequested += this.Manager_BackRequested;
            this.fm_inner.Navigate(this.HomePageType ?? typeof(SearchPage));
            await Task.Yield();
            this.btnPane.Focus(FocusState.Pointer);
        }

        private async void Control_Loaded(object sender, RoutedEventArgs e)
        {
            this.UserInfo = await UserInfo.LoadFromCache();
            RootController.UpdateUserInfo(false);
            RootController.HandleUriLaunch();
            vsg_CurrentStateChanging(null, null);
        }

        private void Control_Unloaded(object sender, RoutedEventArgs e)
        {
            this.manager.BackRequested -= this.Manager_BackRequested;
        }

        private void Manager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if(this.fm_inner.CanGoBack && !RootController.ViewDisabled)
            {
                this.fm_inner.GoBack();
                e.Handled = true;
            }
        }

        private void fm_inner_Navigated(object sender, NavigationEventArgs e)
        {
            if(this.fm_inner.CanGoBack)
                this.manager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            else
                this.manager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            Controls.SplitViewTab tab;
            var pageType = this.fm_inner.Content.GetType();
            JYAnalyticsUniversal.JYAnalytics.TrackPageStart(pageType.Name);
            if(this.pages.TryGetValue(pageType, out tab))
            {
                tab.IsChecked = true;
            }
        }

        private void fm_inner_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            var content = this.fm_inner.Content;
            if(content == null)
                return;
            var pageType = content.GetType();
            JYAnalyticsUniversal.JYAnalytics.TrackPageEnd(pageType.Name);
            Controls.SplitViewTab tab;
            if(this.pages.TryGetValue(pageType, out tab))
            {
                tab.IsChecked = false;
            }
        }

        private void svt_Click(object sender, RoutedEventArgs e)
        {
            var s = (Controls.SplitViewTab)sender;
            if(s.IsChecked)
                return;
            this.fm_inner.Navigate(this.tabs[s]);
            RootController.SwitchSplitView(false);
        }

        private async void btn_ChangeUser_Click(object sender, RoutedEventArgs e)
        {
            await RootController.RequestLogOn();
        }

        private void btn_pane_Click(object sender, RoutedEventArgs e)
        {
            RootController.SwitchSplitView(null);
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = true;
            switch(e.OriginalKey)
            {
            case Windows.System.VirtualKey.Control:
            case Windows.System.VirtualKey.LeftControl:
            case Windows.System.VirtualKey.RightControl:
            case Windows.System.VirtualKey.GamepadView:
                RootController.SwitchSplitView(null);
                break;
            default:
                e.Handled = false;
                break;
            }
        }

        private void vsg_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            switch(this.sv_root.DisplayMode)
            {
            case SplitViewDisplayMode.Overlay:
                RootController.SetSplitViewButtonPlaceholderVisibility(true);
                break;
            case SplitViewDisplayMode.CompactOverlay:
                RootController.SetSplitViewButtonPlaceholderVisibility(false);
                break;
            }
        }

        private void page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if(RootController.IsFullScreen || e.NewSize.Width < 720)
            {
                this.sv_root.DisplayMode = SplitViewDisplayMode.Overlay;
                RootController.SetSplitViewButtonPlaceholderVisibility(true);
            }
            else
            {
                this.sv_root.DisplayMode = SplitViewDisplayMode.CompactOverlay;
                RootController.SetSplitViewButtonPlaceholderVisibility(false);
            }
        }
    }
}
