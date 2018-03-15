using ExClient.Galleries;
using ExViewer.Controls;
using ExViewer.Services;
using ExViewer.ViewModels;
using Opportunity.MvvmUniverse.Services.Notification;
using Opportunity.MvvmUniverse.Views;
using System;
using System.Linq;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ExViewer.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CachedPage : MvvmPage, IHasAppBar
    {
        public CachedPage()
        {
            this.InitializeComponent();
            this.ViewModel = CachedVM.Instance;
        }

        public new CachedVM ViewModel
        {
            get => (CachedVM)base.ViewModel;
            set => base.ViewModel = value;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await Dispatcher.YieldIdle();
            if (e.NavigationMode != NavigationMode.Back)
            {
                if (this.ViewModel.Galleries == null)
                {
                    this.ViewModel.Refresh.Execute();
                    this.abb_Refresh.Focus(FocusState.Programmatic);
                }
                else
                    this.lv.Focus(FocusState.Programmatic);
            }
            else
            {
                if (!await ViewHelper.ScrollAndFocus(this.lv, this.opened))
                    this.lv.Focus(FocusState.Programmatic);
            }
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
            case VirtualKey.GamepadY:
                this.cb_top.Focus(FocusState.Keyboard);
                break;
            case VirtualKey.GamepadMenu:
            case VirtualKey.Application:
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

        private static ContentDialogQuestionData confirmClear = new ContentDialogQuestionData
        {
            Title = Strings.Resources.Views.ClearCachedDialog.Title,
            Content = Strings.Resources.Views.ClearCachedDialog.Content,
            PrimaryButtonText = Strings.Resources.General.OK,
            CloseButtonText = Strings.Resources.General.Cancel,
        };

        private async void abb_DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if ((await Notificator.GetForCurrentView().NotifyAsync(ContentDialogNotification.Question, confirmClear))
                == NotificationResult.Positive)
            {
                this.ViewModel.Clear.Execute();
            }
        }

        public void CloseAll()
        {
            this.cb_top.IsOpen = false;
        }

        private void lv_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            var lvi = (args.OriginalSource as DependencyObject)?.AncestorsAndSelf<ListViewItem>()?.FirstOrDefault();
            if (lvi == null)
                return;
            var dc = lvi.Content;
            this.mfi_DeleteGallery.CommandParameter = dc;
            this.mf_Gallery.ShowAt(lvi);
            args.Handled = true;
        }

        private void lv_ContextCanceled(UIElement sender, RoutedEventArgs args)
        {
            this.mf_Gallery.Hide();
        }
    }
}
