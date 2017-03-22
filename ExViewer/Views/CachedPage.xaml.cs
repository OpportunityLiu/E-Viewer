using ExClient;
using ExViewer.Controls;
using ExViewer.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
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
                PrimaryButtonText = Strings.Resources.OK,
                SecondaryButtonText = Strings.Resources.Cancel,
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
            if(e.NavigationMode != NavigationMode.Back || this.VM.Galleries == null)
            {
                this.VM.Refresh.Execute(null);
            }
            else if(e.NavigationMode == NavigationMode.Back)
            {
                await Task.Delay(50);
                ((ListViewItem)this.lv.ContainerFromItem(this.opened))?.Focus(FocusState.Programmatic);
            }
        }

        private Gallery opened;

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void lv_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(this.VM.Open.CanExecute(e.ClickedItem))
            {
                this.VM.Open.Execute(e.ClickedItem);
                this.opened = (Gallery)e.ClickedItem;
            }
        }

        private MyContentDialog cdg_ConfirmClear;

        private async void abb_DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            await this.cdg_ConfirmClear.ShowAsync();
        }

        private void lv_RefreshRequested(object sender, EventArgs args)
        {
            this.VM.Refresh.Execute(null);
        }

        public void CloseAll()
        {
            this.cb_top.IsOpen = false;
        }

        private void lv_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            var lvi = (args.OriginalSource as DependencyObject)?.FirstAncestorOrSelf<ListViewItem>();
            if(lvi == null)
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
