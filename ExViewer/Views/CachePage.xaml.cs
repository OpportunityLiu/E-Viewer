using ExClient;
using ExViewer.ViewModels;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CachePage : Page
    {
        public CachePage()
        {
            this.InitializeComponent();
            VM = SimpleIoc.Default.GetInstance<CacheVM>();
            cdg_ConfirmClear = new ContentDialog()
            {
                Title = "ARE YOU SURE",
                Content = "All saved galleries will be deleted.",
                PrimaryButtonText = "Ok",
                SecondaryButtonText = "Cancel",
                PrimaryButtonCommand = VM.Clear
            };
        }

        public CacheVM VM
        {
            get
            {
                return (CacheVM)GetValue(VMProperty);
            }
            set
            {
                SetValue(VMProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for VM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty =
            DependencyProperty.Register("VM", typeof(CacheVM), typeof(CachePage), new PropertyMetadata(null));

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.NavigationMode != NavigationMode.Back)
            {
                VM.Refresh.Execute(null);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void btn_Pane_Click(object sender, RoutedEventArgs e)
        {
            cb_top.IsOpen = false;
            RootControl.RootController.SwitchSplitView();
        }

        private void lv_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(GalleryPage), e.ClickedItem);
        }

        private ContentDialog cdg_ConfirmClear;

        private async void abb_ClearCache_Click(object sender, RoutedEventArgs e)
        {
            await cdg_ConfirmClear.ShowAsync();
        }

        private void gv_Gallery_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void gv_Gallery_Holding(object sender, HoldingRoutedEventArgs e)
        {
            var s = (FrameworkElement)sender;
            switch(e.HoldingState)
            {
            case Windows.UI.Input.HoldingState.Started:
                FlyoutBase.ShowAttachedFlyout(s);
                break;
            case Windows.UI.Input.HoldingState.Canceled:
                FlyoutBase.GetAttachedFlyout(s).Hide();
                break;
            default:
                break;
            }
        }
    }
}
