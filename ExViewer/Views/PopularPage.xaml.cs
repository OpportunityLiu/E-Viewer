using ExClient.Galleries;
using ExViewer.Controls;
using ExViewer.ViewModels;
using Opportunity.MvvmUniverse.Views;
using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PopularPage : MvvmPage, IHasAppBar
    {
        public PopularPage()
        {
            this.InitializeComponent();
            this.ViewModel = new PopularVM();
        }

        public new PopularVM ViewModel
        {
            get => (PopularVM)base.ViewModel;
            set => base.ViewModel = value;
        }

        public static readonly DependencyProperty VMProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(PopularVM), typeof(PopularPage), new PropertyMetadata(null));

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                if (await ViewHelper.ScrollAndFocus(this.lv, this.opened))
                    return;
            }
            await Dispatcher.YieldIdle();
            if (this.lv.Items.Count != 0)
                this.lv.Focus(FocusState.Programmatic);
            else
                this.cb_top.Focus(FocusState.Programmatic);
        }

        private Gallery opened;

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

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
