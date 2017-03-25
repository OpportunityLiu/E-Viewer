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
            this.Clear = new RelayCommand(() =>
            {
                RootControl.RootController.TrackAsyncAction(SavedGallery.ClearAllGalleriesAsync(), (s, e) =>
                {
                    CachedVM.Instance.Refresh.Execute(null);
                    this.Refresh.Execute(null);
                });
            });
            this.Refresh = new RelayCommand(async () =>
            {
                this.Galleries = null;
                this.Refresh.RaiseCanExecuteChanged();
                this.Galleries = await SavedGallery.LoadSavedGalleriesAsync();
                this.Refresh.RaiseCanExecuteChanged();
            });
            this.SaveTo = new RelayCommand<Gallery>(async g =>
            {
                var getTarget = savePicker.PickSingleFolderAsync();
                var sourceFolder = await g.GetFolderAsync();
                var files = await sourceFolder.GetFilesAsync();
                var target = await getTarget;
                if(target == null)
                    return;
                target = await target.CreateFolderAsync(StorageHelper.ToValidFolderName(g.GetDisplayTitle()), CreationCollisionOption.GenerateUniqueName);
                foreach(var file in files)
                {
                    await file.CopyAsync(target, file.Name, NameCollisionOption.ReplaceExisting);
                }
                RootControl.RootController.SendToast(Strings.Resources.GallerySavedTo, typeof(SavedPage));
            });
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

        public static IAsyncOperationWithProgress<StorageFolder, double> GetCopyOf(Gallery gallery)
        {
            return Run<StorageFolder, double>(async (token, progress) =>
            {
                progress.Report(double.NaN);
                var source = await gallery.GetFolderAsync();
                var temp = await StorageHelper.CreateTempFolderAsync();
                var name = StorageHelper.ToValidFolderName(gallery.GetDisplayTitle());
                var target = await temp.CreateFolderAsync(name);
                var files = await source.GetFilesAsync();
                for(var i = 0; i < files.Count; i++)
                {
                    progress.Report((double)i / files.Count);
                    await files[i].CopyAsync(target);
                }
                return target;
            });
        }
    }
}
