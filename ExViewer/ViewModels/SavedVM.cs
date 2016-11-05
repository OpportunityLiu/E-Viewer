using ExClient;
using ExViewer.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExViewer.ViewModels
{
    public class SavedVM : GalleryListVM<SavedGallery>
    {
        public static SavedVM Instance
        {
            get;
        } = new SavedVM();

        private SavedVM()
        {
            Clear = new RelayCommand(() =>
            {
                RootControl.RootController.TrackAsyncAction(SavedGallery.ClearAllGalleriesAsync(), (s, e) =>
                {
                    CachedVM.Instance.Refresh.Execute(null);
                    Refresh.Execute(null);
                });
            });
            Refresh = new RelayCommand(async () =>
            {
                this.Galleries = null;
                this.Refresh.RaiseCanExecuteChanged();
                this.Galleries = await SavedGallery.LoadSavedGalleriesAsync();
                this.Refresh.RaiseCanExecuteChanged();
            });
            SaveTo = new RelayCommand<Gallery>(async g =>
            {
                var getTarget = savePicker.PickSingleFolderAsync();
                var sourceFolder = await g.GetFolderAsync();
                var files = await sourceFolder.GetFilesAsync();
                var target = await getTarget;
                if(target == null)
                    return;
                target = await target.CreateFolderAsync(toValidFolderName(g.GetDisplayTitle()), CreationCollisionOption.GenerateUniqueName);
                foreach(var file in files)
                {
                    await file.CopyAsync(target, file.Name, NameCollisionOption.ReplaceExisting);
                }
                RootControl.RootController.SendToast(LocalizedStrings.Resources.GallerySavedTo, typeof(SavedPage));
            });
        }

        private static Dictionary<char, char> alternateFolderChars = new Dictionary<char, char>()
        {
            ['?'] = '？',
            ['\\'] = '＼',
            ['/'] = '／',
            ['"'] = '＂',
            ['|'] = '｜',
            ['*'] = '＊',
            ['<'] = '＜',
            ['>'] = '＞',
            [':'] = '：'
        };

        private static string toValidFolderName(string raw)
        {
            var sb = new StringBuilder(raw);
            foreach(var item in alternateFolderChars)
            {
                sb.Replace(item.Key, item.Value);
            }
            var invalid = Path.GetInvalidFileNameChars();
            foreach(var item in invalid)
            {
                sb.Replace(item.ToString(), "");
            }
            return sb.ToString().Trim();
        }

        private static FolderPicker savePicker = initSavePicker();

        private static FolderPicker initSavePicker()
        {
            var p = new FolderPicker()
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                ViewMode = PickerViewMode.Thumbnail
            };
            p.FileTypeFilter.Add("*");
            return p;
        }

        public RelayCommand<Gallery> SaveTo
        {
            get;
        }

        public static IAsyncOperation<StorageFolder> GetCopyOf(Gallery gallery)
        {
            return Task.Run(async () =>
            {
                var p = new SaveGalleryProgress();
                var source = await gallery.GetFolderAsync();
                var files = await source.GetFilesAsync();
                var name = toValidFolderName(gallery.GetDisplayTitle());
                var temp = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync(DateTimeOffset.Now.Ticks.ToString());
                var target = await temp.CreateFolderAsync(name);
                foreach(var item in files)
                {
                    await item.CopyAsync(target);
                }
                return target;
            }).AsAsyncOperation();
        }
    }
}
