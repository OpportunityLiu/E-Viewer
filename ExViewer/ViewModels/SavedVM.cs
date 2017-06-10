using ExClient;
using ExClient.Galleries;
using ExViewer.Views;
using Opportunity.MvvmUniverse.Commands;
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
            this.Clear = new Command(() =>
            {
                RootControl.RootController.TrackAsyncAction(SavedGallery.ClearAllGalleriesAsync(), (s, e) =>
                {
                    CachedVM.Instance.Refresh.Execute();
                    this.Refresh.Execute();
                });
            });
            this.Refresh = new AsyncCommand(async () =>
            {
                this.Galleries = null;
                this.Galleries = await SavedGallery.LoadSavedGalleriesAsync();
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
    }
}
