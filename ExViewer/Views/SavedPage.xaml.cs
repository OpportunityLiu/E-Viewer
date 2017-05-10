using ExClient;
using ExViewer.Controls;
using ExViewer.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
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
    public sealed partial class SavedPage : MyPage, IHasAppBar
    {
        public SavedPage()
        {
            this.InitializeComponent();
            this.VisibleBoundHandledByDesign = true;
            this.VM = SavedVM.Instance;
            this.cdg_ConfirmClear = new MyContentDialog()
            {
                Title = Strings.Resources.Views.ClearSavedDialog.Title,
                Content = Strings.Resources.Views.ClearSavedDialog.Content,
                PrimaryButtonText = Strings.Resources.General.OK,
                SecondaryButtonText = Strings.Resources.General.Cancel,
                PrimaryButtonCommand = this.VM.Clear
            };
        }

        public SavedVM VM
        {
            get => (SavedVM)GetValue(VMProperty);
            set => SetValue(VMProperty, value);
        }

        // Using a DependencyProperty as the backing store for VM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty =
            DependencyProperty.Register("VM", typeof(SavedVM), typeof(SavedPage), new PropertyMetadata(null));

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode != NavigationMode.Back || this.VM.Galleries == null)
            {
                this.VM.Refresh.Execute();
            }
            else if (e.NavigationMode == NavigationMode.Back)
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
            if (this.VM.Open.Execute(item))
            {
                this.opened = item;
            }
        }

        private readonly MyContentDialog cdg_ConfirmClear;

        private async void abb_DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            await this.cdg_ConfirmClear.ShowAsync();
        }

        private void lv_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            var item = e.Items.FirstOrDefault() as Gallery;
            if (item == null)
            {
                e.Cancel = true;
                return;
            }
            this.mf_Gallery.Hide();
            e.Data.SetDataProvider(StandardDataFormats.StorageItems, async request =>
            {
                var d = request.GetDeferral();
                try
                {
                    var makeCopy = SavedVM.GetCopyOf(item);
                    request.SetData(new IStorageItem[] { await makeCopy });
                }
                finally { d.Complete(); }
            });
            e.Data.Properties.ApplicationName = Package.Current.DisplayName;
            e.Data.Properties.PackageFamilyName = Package.Current.Id.FamilyName;
            e.Data.Properties.Description = item.Title;
            e.Data.Properties.Title = item.Title;
            e.Data.Properties.Thumbnail = RandomAccessStreamReference.CreateFromUri(item.ThumbUri);
            e.Data.RequestedOperation = DataPackageOperation.Move;
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
            this.mfi_SaveTo.CommandParameter = dc;
            this.mf_Gallery.ShowAt(lvi);
            args.Handled = true;
        }

        private void lv_ContextCanceled(UIElement sender, RoutedEventArgs args)
        {
            this.mf_Gallery.Hide();
        }
    }
}
