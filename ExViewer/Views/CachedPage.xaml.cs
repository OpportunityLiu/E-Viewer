using ExClient.Galleries;
using ExViewer.Controls;
using ExViewer.ViewModels;
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
    public sealed partial class CachedPage : MyPage, IHasAppBar
    {
        public CachedPage()
        {
            this.InitializeComponent();
            this.VisibleBoundHandledByDesign = true;
            this.VM = CachedVM.Instance;
            this.cdg_ConfirmClear = new MyContentDialog()
            {
                Title = Strings.Resources.Views.ClearCachedDialog.Title,
                Content = Strings.Resources.Views.ClearCachedDialog.Content,
                PrimaryButtonText = Strings.Resources.General.OK,
                SecondaryButtonText = Strings.Resources.General.Cancel,
                PrimaryButtonCommand = this.VM.Clear
            };
        }

        public CachedVM VM
        {
            get => (CachedVM)GetValue(VMProperty);
            set => SetValue(VMProperty, value);
        }

        // Using a DependencyProperty as the backing store for VM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty =
            DependencyProperty.Register("VM", typeof(CachedVM), typeof(CachedPage), new PropertyMetadata(null));

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await Dispatcher.YieldIdle();
            if (e.NavigationMode != NavigationMode.Back)
            {
                if (this.VM.Galleries == null)
                {
                    this.VM.Refresh.Execute();
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
            if (this.VM.Open.Execute(item))
            {
                this.opened = item;
            }
        }

        private MyContentDialog cdg_ConfirmClear;

        private async void abb_DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            await this.cdg_ConfirmClear.ShowAsync();
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
