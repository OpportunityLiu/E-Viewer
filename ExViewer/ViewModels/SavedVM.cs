using ExClient.Galleries;
using ExViewer.Views;
using Opportunity.MvvmUniverse.Commands;
using System;
using System.Collections.Generic;
using Windows.Storage.Pickers;
using ICommand = System.Windows.Input.ICommand;

namespace ExViewer.ViewModels
{
    public class SavedVM : GalleryListVM<SavedGallery>
    {
        public static SavedVM Instance { get; } = new SavedVM();

        private SavedVM()
        {
            this.Clear.Tag = this;
            this.Refresh.Tag = this;
        }

        public override AsyncCommand Refresh { get; } = AsyncCommand.Create(async sender =>
        {
            var that = (SavedVM)sender.Tag;
            that.Galleries = null;
            that.Galleries = await SavedGallery.LoadSavedGalleriesAsync();
        });

        public override Command Clear { get; } = Command.Create(sender =>
        {
            RootControl.RootController.TrackAsyncAction(SavedGallery.ClearAllGalleriesAsync(), (s, e) =>
            {
                var that = (SavedVM)sender.Tag;
                CachedVM.Instance.Refresh.Execute();
                that.Refresh.Execute();
            });
        });

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
