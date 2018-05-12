using ExClient.Galleries;
using ExClient.Search;
using ExViewer.Controls;
using ExViewer.Services;
using ExViewer.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Opportunity.MvvmUniverse.Services.Notification;
using Opportunity.MvvmUniverse.Views;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ToplistPage : MvvmPage, IHasAppBar
    {
        public ToplistPage()
        {
            this.InitializeComponent();
            this.ViewModel = new ToplistVM();
        }

        public new ToplistVM ViewModel
        {
            get => (ToplistVM)base.ViewModel;
            set => base.ViewModel = value;
        }

        private void page_Loading(FrameworkElement sender, object args)
        {
            this.setSplitViewButtonPlaceholderVisibility(null, RootControl.RootController.SplitViewButtonPlaceholderVisibility);
            RootControl.RootController.SplitViewButtonPlaceholderVisibilityChanged += this.setSplitViewButtonPlaceholderVisibility;
        }

        private void page_Unloaded(object sender, RoutedEventArgs e)
        {
            RootControl.RootController.SplitViewButtonPlaceholderVisibilityChanged -= this.setSplitViewButtonPlaceholderVisibility;
        }

        private void setSplitViewButtonPlaceholderVisibility(RootControl sender, bool visible)
        {
            if (visible)
                this.bdSplitViewPlaceholder.Width = 48;
            else
                this.bdSplitViewPlaceholder.Width = 0;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await Dispatcher.YieldIdle();
            if (e.NavigationMode != NavigationMode.Back)
            {
                this.pvRoot.Focus(FocusState.Programmatic);
            }
            else
            {
                if (!await ViewHelper.ScrollAndFocus(this.openedLv, this.opened))
                {
                    this.pvRoot.Focus(FocusState.Programmatic);
                }
            }
        }

        private Gallery opened;
        private ListView openedLv;

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = true;
            switch (e.Key)
            {
            case Windows.System.VirtualKey.GamepadY:
                this.cb_top.Focus(FocusState.Keyboard);
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

        private void lv_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (Gallery)e.ClickedItem;
            if (this.ViewModel.Open.Execute(item))
            {
                this.opened = item;
                this.openedLv = (ListView)sender;
            }
        }

        public void CloseAll()
        {
            this.cb_top.IsOpen = false;
        }

        private void lv_RefreshRequested(object sender, EventArgs e)
        {
            var s = (ListView)sender;
            var g = (GalleryToplist)s.ItemsSource;
            ViewModel.Refresh.Execute(g);
        }
    }
}
