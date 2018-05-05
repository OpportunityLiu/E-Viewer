using ExClient.Galleries;
using ExViewer.Controls;
using ExViewer.Services;
using ExViewer.ViewModels;
using Opportunity.MvvmUniverse.Services.Notification;
using Opportunity.MvvmUniverse.Views;
using System;
using System.Linq;
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

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await Dispatcher.YieldIdle();
            //if (e.NavigationMode != NavigationMode.Back)
            //{
            //    if (this.ViewModel.Galleries is null)
            //    {
            //        this.ViewModel.Refresh.Execute();
            //        this.abb_Refresh.Focus(FocusState.Programmatic);
            //    }
            //    else
            //    {
            //        this.lv.Focus(FocusState.Programmatic);
            //    }
            //}
            //else
            //{
            //    if (!await ViewHelper.ScrollAndFocus(this.lv, this.opened))
            //    {
            //        this.lv.Focus(FocusState.Programmatic);
            //    }
            //}
        }

        private Gallery opened;

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
            }
        }

        public void CloseAll()
        {
            this.cb_top.IsOpen = false;
        }
    }
}
