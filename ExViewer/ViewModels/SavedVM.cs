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
            Commands.Add(nameof(Clear), Command.Create(sender =>
            {
                RootControl.RootController.TrackAsyncAction(SavedGallery.ClearAllGalleriesAsync(), (s, e) =>
                {
                    CachedVM.Instance.Refresh.Execute();
                    Instance.Refresh.Execute();
                });
            }));
            Commands.Add(nameof(Refresh), AsyncCommand.Create(async (sender) =>
            {
                var that = (SavedVM)sender.Tag;
                that.Galleries = null;
                that.Galleries = await SavedGallery.LoadSavedGalleriesAsync();
            }));
        }

        private static readonly FolderPicker savePicker = initSavePicker();

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
