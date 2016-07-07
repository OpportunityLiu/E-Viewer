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

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RootControl : Page
    {
        public RootControl(Type homePageType, ApplicationExecutionState previousState)
        {
            this.InitializeComponent();
            tabs = new Dictionary<Controls.SplitViewTab, Type>()
            {
                [this.svt_Cache] = typeof(CachePage),
                [this.svt_Search] = typeof(SearchPage),
                [this.svt_Settings] = typeof(SettingsPage)
            };

            pages = new Dictionary<Type, Controls.SplitViewTab>()
            {
                [typeof(CachePage)] = this.svt_Cache,
                [typeof(SearchPage)] = this.svt_Search,
                [typeof(SettingsPage)] = this.svt_Settings
            };
            RootController.SetRoot(this);
            this.homePageType = homePageType;
            this.previousState = previousState;
        }

        private Type homePageType;
        private ApplicationExecutionState previousState;

        private readonly Dictionary<Controls.SplitViewTab, Type> tabs;
        private readonly Dictionary<Type, Controls.SplitViewTab> pages;

        SystemNavigationManager manager;

        private void btn_pane_Click(object sender, RoutedEventArgs e)
        {
            sv_root.IsPaneOpen = !sv_root.IsPaneOpen;
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            if(Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                var ignore = statusBar.ShowAsync();
            }
            manager = SystemNavigationManager.GetForCurrentView();
            manager.BackRequested += Manager_BackRequested;
            fm_inner.Navigate(homePageType);
        }

        private void Control_Unloaded(object sender, RoutedEventArgs e)
        {
            manager.BackRequested -= Manager_BackRequested;
        }

        private void Manager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if(fm_inner.CanGoBack && !RootController.ViewDisabled)
            {
                fm_inner.GoBack();
                e.Handled = true;
            }
        }

        private void fm_inner_Navigated(object sender, NavigationEventArgs e)
        {
            if(fm_inner.CanGoBack)
                manager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            else
                manager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            Controls.SplitViewTab tab;
            if(pages.TryGetValue(fm_inner.Content.GetType(), out tab))
            {
                tab.IsChecked = true;
            }
        }

        private void fm_inner_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            var content = fm_inner.Content;
            if(content == null)
                return;
            Controls.SplitViewTab tab;
            if(this.pages.TryGetValue(content.GetType(), out tab))
            {
                tab.IsChecked = false;
            }
        }

        private void svt_Click(object sender, RoutedEventArgs e)
        {
            var s = (Controls.SplitViewTab)sender;
            if(s.IsChecked)
                return;
            fm_inner.Navigate(tabs[s]);
            sv_root.IsPaneOpen = !sv_root.IsPaneOpen;
        }

        private void fm_inner_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
#if DEBUG && !DISABLE_XAML_GENERATED_BREAK_ON_UNHANDLED_EXCEPTION
            if(global::System.Diagnostics.Debugger.IsAttached)
                global::System.Diagnostics.Debugger.Break();
#endif
        }

        private void Grid_GotFocus(object sender, RoutedEventArgs e)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(e.OriginalSource);
#endif
        }

        //private async void svb_LogOn_Click(object sender, RoutedEventArgs e)
        //{
        //    await RootController.RequireLogOn();
        //}
    }
}
