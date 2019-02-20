using ExViewer.Controls;
using ExViewer.ViewModels;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace ExViewer.Views
{
    public sealed partial class FileSearchDialog : MyContentDialog
    {
        public FileSearchDialog()
        {
            InitializeComponent();
            picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                CommitButtonText = Strings.Resources.Views.FileSearchDialog.FileOpenPicker.CommitButtonText,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary,
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SettingsIdentifier = nameof(FileSearchDialog)
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpe");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".jfif");
            picker.FileTypeFilter.Add(".gif");
            picker.FileTypeFilter.Add(".png");
        }

        private void MyContentDialog_Loading(FrameworkElement sender, object args)
        {
            SearchFile = null;
            cbCover.IsChecked = false;
            cbExp.IsChecked = false;
            cbSimilar.IsChecked = true;
        }

        private async void MyContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            await Dispatcher.YieldIdle();
            btnBrowse.Focus(FocusState.Programmatic);
        }

        public StorageFile SearchFile
        {
            get => (StorageFile)GetValue(SearchFileProperty); set => SetValue(SearchFileProperty, value);
        }

        // Using a DependencyProperty as the backing store for SearchFile.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchFileProperty =
            DependencyProperty.Register("SearchFile", typeof(StorageFile), typeof(FileSearchDialog), new PropertyMetadata(null));

        private readonly Windows.Storage.Pickers.FileOpenPicker picker;

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var search = ExClient.Client.Current.SearchAsync(SearchFile, cbSimilar.IsChecked ?? true, cbCover.IsChecked ?? false, cbExp.IsChecked ?? false);
            SearchFile = null;
            await Dispatcher.YieldIdle();
            RootControl.RootController.TrackAsyncAction(search, p => double.NaN, async (s, e) =>
            {
                switch (e)
                {
                case AsyncStatus.Completed:
                    var vm = SearchVM.GetVM(s.GetResults());
                    await RootControl.RootController.Navigator.NavigateAsync(typeof(SearchPage), vm.SearchQuery);
                    break;
                case AsyncStatus.Error:
                    RootControl.RootController.SendToast(s.ErrorCode, typeof(SearchPage));
                    break;
                }
                s.Close();
            });
        }

        private async void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var file = await picker.PickSingleFileAsync();
            if (file is null)
            {
                return;
            }

            SearchFile = file;
        }

        private async Task<StorageFile> check(DragEventArgs e)
        {
            if (!e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                return null;
            }
            var deferral = e.GetDeferral();
            Debug.Assert(e.DragUIOverride != null, "e.DragUIOverride != null");
            try
            {
                var info = Strings.Resources.Views.FileSearchDialog;
                var storageitems = await e.DataView.GetStorageItemsAsync();
                if (storageitems.Count != 1 || !(storageitems[0] is StorageFile file))
                {
                    e.DragUIOverride.Caption = info.DropWrongFileNumber;
                    return null;
                }
                if (!picker.FileTypeFilter.Contains(file.FileType.ToLowerInvariant()))
                {
                    e.DragUIOverride.Caption = info.DropWrongFileType;
                    return null;
                }
                e.AcceptedOperation = DataPackageOperation.Move | DataPackageOperation.Copy | DataPackageOperation.Link;
                e.DragUIOverride.Caption = info.DropHint;
                e.Handled = true;
                return file;
            }
            finally
            {
                deferral.Complete();
            }
        }

        private async void tbFileName_DragEnter(object sender, DragEventArgs e)
        {
            await check(e);
        }

        private async void tbFileName_Drop(object sender, DragEventArgs e)
        {
            SearchFile = await check(e);
        }
    }
}
