using ExClient.Status;
using Opportunity.MvvmUniverse.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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

            this.manager.Handlers.Add(this.fm_inner.AsNavigationHandler());

            this.tabs = new Dictionary<Controls.SplitViewTab, Type>()
            {
                [this.svt_Saved] = typeof(SavedPage),
                [this.svt_Cached] = typeof(CachedPage),
                [this.svt_Search] = typeof(SearchPage),
                [this.svt_Favorites] = typeof(FavoritesPage),
                [this.svt_Popular] = typeof(PopularPage),
                [this.svt_Settings] = typeof(SettingsPage)
            };

            this.pages = new Dictionary<Type, Controls.SplitViewTab>()
            {
                [typeof(CachedPage)] = this.svt_Cached,
                [typeof(SavedPage)] = this.svt_Saved,
                [typeof(SearchPage)] = this.svt_Search,
                [typeof(FavoritesPage)] = this.svt_Favorites,
                [typeof(PopularPage)] = this.svt_Popular,
                [typeof(SettingsPage)] = this.svt_Settings
            };

            vistualizeFocus();
        }

        [Conditional("DEBUG")]
        private void vistualizeFocus()
        {
            this.GotFocus += (s, e) =>
            {
                var focus = FocusManager.GetFocusedElement();
                if (focus == null)
                    return;
                var fe = focus as FrameworkElement;
                var con = fe as Control;
                Debug.WriteLine($"{(fe?.Name ?? focus.ToString())}({focus.GetType()}) {con?.FocusState}", "Focus state");
            };
        }

        private readonly Dictionary<Controls.SplitViewTab, Type> tabs;
        private readonly Dictionary<Type, Controls.SplitViewTab> pages;

        private bool layoutLoaded;

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
                if (this.sv_root.DisplayMode == SplitViewDisplayMode.Overlay)
                {
                    return VisibleBoundsThickness;
                }
                var v = VisibleBoundsThickness;
                v.Left = 0;
                return v;
            }
        }

        private readonly Navigator manager = Navigator.GetOrCreateForCurrentView();

        private async void Control_Loading(FrameworkElement sender, object args)
        {
            if (!this.layoutLoaded)
            {
                RootController.SetRoot(this);
            }
            else
            {
                await this.manager.NavigateAsync(typeof(SearchPage));
            }
        }

        private async void Control_Loaded(object sender, RoutedEventArgs e)
        {
            var temp = this.layoutLoaded;
            this.layoutLoaded = true;
            if (!temp)
            {
                this.UserInfo = await UserInfo.LoadFromCache();
                RootController.UpdateUserInfo(false);
            }
            else
            {
                RootController.HandleUriLaunch();
                this.tbtPane.Focus(FocusState.Pointer);
            }
        }

        private void Control_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void fm_inner_Navigated(object sender, NavigationEventArgs e)
        {
            var pageType = this.fm_inner.Content.GetType();
            if (this.pages.TryGetValue(pageType, out var tab))
            {
                tab.IsChecked = true;
            }
        }

        private void fm_inner_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            var content = this.fm_inner.Content;
            if (content == null)
                return;
            var pageType = content.GetType();
            if (this.pages.TryGetValue(pageType, out var tab))
            {
                tab.IsChecked = false;
            }
        }

        private async void svt_Click(object sender, RoutedEventArgs e)
        {
            var s = (Controls.SplitViewTab)sender;
            if (s.IsChecked)
                return;
            RootController.SwitchSplitView(false);
            await this.manager.NavigateAsync(this.tabs[s]);
        }

        private async void btn_UserInfo_Click(object sender, RoutedEventArgs e)
        {
            if (!(this.fm_inner.Content is InfoPage))
            {
                RootController.SwitchSplitView(false);
                await this.manager.NavigateAsync(typeof(InfoPage));
            }
        }

        private void tbtPaneBindBack(bool? value)
        {
            RootController.SwitchSplitView((bool)value);
        }

        private bool? tbtPaneBind(bool value)
        {
            return value;
        }

        private AcrylicBackgroundSource abPaneBackgroundBind(bool value, SplitViewDisplayMode mode)
        {
            if (mode == SplitViewDisplayMode.Overlay)
                return AcrylicBackgroundSource.Backdrop;
            return value ? AcrylicBackgroundSource.Backdrop : AcrylicBackgroundSource.HostBackdrop;
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = true;
            switch (e.OriginalKey)
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
            if (RootController.IsFullScreen || availableSize.Width < NARROW_WIDE_WIDTH)
            {
                if (this.sv_root.DisplayMode != SplitViewDisplayMode.Overlay)
                {
                    this.sv_root.DisplayMode = SplitViewDisplayMode.Overlay;
                    RootController.SetSplitViewButtonPlaceholderVisibility(true);
                }
            }
            else
            {
                if (this.sv_root.DisplayMode != SplitViewDisplayMode.CompactOverlay)
                {
                    this.sv_root.DisplayMode = SplitViewDisplayMode.CompactOverlay;
                    RootController.SetSplitViewButtonPlaceholderVisibility(false);
                }
            }
            var paneHeight = RootController.InputPane.OccludedRect.Height;
            if (RootController.ApplicationView.DesiredBoundsMode == ApplicationViewBoundsMode.UseCoreWindow)
            {
                if (paneHeight == 0)
                    setVisibleBoundsThickness(new Thickness());
                else
                    setVisibleBoundsThickness(new Thickness(0, 0, 0, paneHeight));
            }
            else
            {
                var vb = RootController.ApplicationView.VisibleBounds;
                var wb = Window.Current.Bounds;
                var tbh = RootController.TitleBarHeight;
                setVisibleBoundsThickness(new Thickness(
                    bound(vb.Left - wb.Left),
                    bound(vb.Top + tbh - wb.Top),
                    bound(wb.Right - vb.Right),
                    bound(wb.Bottom - vb.Bottom + paneHeight)));

                double bound(double value) => value < 0 ? 0 : value;
            }
            return base.MeasureOverride(availableSize);
        }

        private void setVisibleBoundsThickness(Thickness value)
        {
            var old = this.VisibleBoundsThickness;
            if (old == value)
                return;
            this.VisibleBoundsThickness = value;
        }

        private double getPaneLength(Thickness visibleBounds, double offset)
        {
            return visibleBounds.Left + offset;
        }
    }
}
