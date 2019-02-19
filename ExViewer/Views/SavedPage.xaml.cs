using ExClient.Galleries;
using ExViewer.Controls;
using ExViewer.Services;
using ExViewer.ViewModels;
using Opportunity.MvvmUniverse.Services.Notification;
using Opportunity.MvvmUniverse.Views;
using System;
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
    public sealed partial class SavedPage : MvvmPage, IHasAppBar
    {
        public SavedPage()
        {
            InitializeComponent();
            ViewModel = SavedVM.Instance;
        }

        public new SavedVM ViewModel
        {
            get => (SavedVM)base.ViewModel;
            set => base.ViewModel = value;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back)
            {
                await Task.Delay(33);
                if (ViewModel.Galleries.IsNullOrEmpty())
                {
                    ViewModel.Refresh.Execute();
                    abb_Refresh.Focus(FocusState.Programmatic);
                }
                else
                {
                    lv.Focus(FocusState.Programmatic);
                }
            }
            else
            {
                if (!await ViewHelper.ScrollAndFocus(lv, opened))
                {
                    lv.Focus(FocusState.Programmatic);
                }
            }
        }

        private Gallery opened;

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = true;
            switch (e.Key)
            {
            case Windows.System.VirtualKey.GamepadY:
                cb_top.Focus(FocusState.Keyboard);
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
            if (ViewModel.Open.Execute(item))
            {
                opened = item;
            }
        }

        private readonly ContentDialogNotificationData confirmClear = new ContentDialogNotificationData
        {
            Title = Strings.Resources.Views.ClearSavedDialog.Title,
            Content = Strings.Resources.Views.ClearSavedDialog.Content,
            PrimaryButtonText = Strings.Resources.General.OK,
            CloseButtonText = Strings.Resources.General.Cancel,
        };

        private async void abb_DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            confirmClear.PrimaryButtonCommand = ViewModel.Clear;
            await Notificator.GetForCurrentView().NotifyAsync(confirmClear);
        }

        public void CloseAll()
        {
            cb_top.IsOpen = false;
        }

        private void lv_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            var lvi = (args.OriginalSource as DependencyObject)?.AncestorsAndSelf<ListViewItem>()?.FirstOrDefault();
            if (lvi is null)
            {
                return;
            }

            var dc = lvi.Content;
            mfi_DeleteGallery.CommandParameter = dc;
            mf_Gallery.ShowAt(lvi);
            args.Handled = true;
        }

        private void lv_ContextCanceled(UIElement sender, RoutedEventArgs args)
        {
            mf_Gallery.Hide();
        }
    }
}
