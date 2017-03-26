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
    public sealed partial class RootControl : UserControl
    {
        public RootControl()
        {
            this.InitializeComponent();
            this.manager = SystemNavigationManager.GetForCurrentView();

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
#if DEBUG
            this.GotFocus += this.OnGotFocus;
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            if(FocusManager.GetFocusedElement() is FrameworkElement focus)
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

        public Thickness VisibleBoundsThickness
        {
            get => (Thickness)GetValue(VisibleBoundsThicknessProperty);
            set => SetValue(VisibleBoundsThicknessProperty, value);
        }

        public static readonly DependencyProperty VisibleBoundsThicknessProperty =
            DependencyProperty.Register(nameof(VisibleBoundsThickness), typeof(Thickness), typeof(RootControl), new PropertyMetadata(new Thickness(0)));

        public Thickness ContentVisibleBoundsThickness
        {
            get
            {
                if(this.sv_root.DisplayMode == SplitViewDisplayMode.Overlay)
                {
                    return VisibleBoundsThickness;
                }
                var v = VisibleBoundsThickness;
                v.Left = 0;
                return v;
            }
        }

        private readonly SystemNavigationManager manager;

        private void Control_Loading(FrameworkElement sender, object args)
        {
            if(this.HomePageType == null)
            {
                RootController.SetRoot(this);
            }
            else
            {
                this.fm_inner.Navigate(this.HomePageType);
                this.tbtPane.Focus(FocusState.Pointer);
            }
            this.manager.BackRequested += this.Manager_BackRequested;
        }

        private async void Control_Loaded(object sender, RoutedEventArgs e)
        {
            if(this.HomePageType == null)
            {
                this.UserInfo = await UserInfo.LoadFromCache();
            }
            else
            {
                Themes.ThemeExtention.SetTitleBar();
                RootController.UpdateUserInfo(false);
                RootController.HandleUriLaunch();
            }
        }

        private void Control_Unloaded(object sender, RoutedEventArgs e)
        {
            this.manager.BackRequested -= this.Manager_BackRequested;
        }

        private Task goBackTask = Task.CompletedTask;

        private void Manager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if(this.fm_inner.CanGoBack && !RootController.ViewDisabled)
            {
                e.Handled = true;
                if(this.goBackTask.IsCompleted)
                {
                    // prevent double click
                    this.goBackTask = Task.Delay(200);
                    this.fm_inner.GoBack();
                }
            }
        }

        private void fm_inner_Navigated(object sender, NavigationEventArgs e)
        {
            if(this.fm_inner.CanGoBack)
                this.manager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            else
                this.manager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            var pageType = this.fm_inner.Content.GetType();
            JYAnalyticsUniversal.JYAnalytics.TrackPageStart(pageType.Name);
            if(this.pages.TryGetValue(pageType, out var tab))
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
            if(this.pages.TryGetValue(pageType, out var tab))
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

        private void tbtPaneBindBack(bool? value)
        {
            RootController.SwitchSplitView(value);
        }

        private bool? tbtPaneBind(bool value)
        {
            return value;
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = true;
            switch(e.OriginalKey)
            {
            case Windows.System.VirtualKey.GamepadView:
                RootController.SwitchSplitView(null);
                break;
            default:
                e.Handled = false;
                break;
            }
        }
#if DEBUG_BOUNDS
        private const double NARROW_WIDE_WIDTH = 620;
#else
        private const double NARROW_WIDE_WIDTH = 720;
#endif

        protected override Size MeasureOverride(Size availableSize)
        {
            if(RootController.IsFullScreen || availableSize.Width < NARROW_WIDE_WIDTH)
            {
                if(this.sv_root.DisplayMode != SplitViewDisplayMode.Overlay)
                {
                    this.sv_root.DisplayMode = SplitViewDisplayMode.Overlay;
                    RootController.SetSplitViewButtonPlaceholderVisibility(true);
                }
            }
            else
            {
                if(this.sv_root.DisplayMode != SplitViewDisplayMode.CompactOverlay)
                {
                    this.sv_root.DisplayMode = SplitViewDisplayMode.CompactOverlay;
                    RootController.SetSplitViewButtonPlaceholderVisibility(false);
                }
            }
            if(RootController.ApplicationView.DesiredBoundsMode == ApplicationViewBoundsMode.UseCoreWindow)
            {
                ClearValue(VisibleBoundsThicknessProperty);
            }
            else
            {
                var vb = RootController.ApplicationView.VisibleBounds;
                var wb = Window.Current.Bounds;
                var tbh = RootController.TitleBarHeight;
                this.VisibleBoundsThickness = new Thickness(vb.Left - wb.Left, vb.Top + tbh - wb.Top, wb.Right - vb.Right, wb.Bottom - vb.Bottom);
            }
            return base.MeasureOverride(availableSize);
        }

        private double getPaneLength(Thickness visibleBounds, double offset)
        {
            return visibleBounds.Left + offset;
        }

        private void elementAccessKeyDisplayRequested(UIElement sender, AccessKeyDisplayRequestedEventArgs args)
        {
            var tooltip = ToolTipService.GetToolTip(sender);
            if(!(tooltip is ToolTip tt))
            {
                tt = new ToolTip { Content = tooltip };
                ToolTipService.SetToolTip(sender, tt);
            }
            tt.IsOpen = true;
        }

        private void elementAccessKeyDisplayDismissed(UIElement sender, AccessKeyDisplayDismissedEventArgs args)
        {
            if(ToolTipService.GetToolTip(sender) is ToolTip tt)
            {
                tt.IsOpen = false;
            }
        }
    }
}
