using ExClient;
using ExViewer.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace ExViewer.Views
{
    public sealed partial class AddToFavoritesDialog : MyContentDialog
    {
        public AddToFavoritesDialog()
        {
            this.InitializeComponent();
            this.PrimaryButtonText = Strings.Resources.General.OK;
            this.SecondaryButtonText = Strings.Resources.General.Cancel;
            foreach(var item in Client.Current.Favorites)
            {
                this.categories.Add(item);
            }
        }


        public Gallery Gallery
        {
            get => (Gallery)GetValue(GalleryProperty); set => SetValue(GalleryProperty, value);
        }

        // Using a DependencyProperty as the backing store for Gallery.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GalleryProperty =
            DependencyProperty.Register("Gallery", typeof(Gallery), typeof(AddToFavoritesDialog), new PropertyMetadata(null));

        private ObservableCollection<FavoriteCategory> categories = new ObservableCollection<FavoriteCategory>();

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.pbLoading.IsIndeterminate = true;
            this.tbInfo.Text = "";
            var def = args.GetDeferral();
            try
            {
                if(this.cbCategory.SelectedItem is FavoriteCategory cat)
                {
                    await cat.AddAsync(this.Gallery, this.tbNote.Text);
                }
                else
                {
                    throw new Exception(strings.FavoriteCategoryUnselected);
                }
            }
            catch(Exception ex)
            {
                this.tbInfo.Text = ex.GetMessage();
                args.Cancel = true;
            }
            finally
            {
                def.Complete();
                this.pbLoading.IsIndeterminate = false;
            }
        }

        private async void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if(this.Gallery == null)
                throw new InvalidOperationException("Property AddToFavoritesDialog.Gallery is null.");
            var galleryInFavorites = this.Gallery.FavoriteCategory == null ? null : (bool?)(this.Gallery.FavoriteCategory.Index >= 0);
            this.tbInfo.Text = "";
            this.tbNote.Text = this.Gallery.FavoriteNote ?? "";
            this.cbCategory.SelectedItem = this.cbCategory.Items.FirstOrDefault();
            this.Title = galleryInFavorites == true ? strings.ModifyTitle : strings.AddTitle;
            if(galleryInFavorites == null)
            {
                if(!await loadNotesAsync())
                    return;
                galleryInFavorites = this.Gallery.FavoriteCategory.Index >= 0;
                this.Title = galleryInFavorites == true ? strings.ModifyTitle : strings.AddTitle;
            }
            else
                await Task.Delay(50);
            if(galleryInFavorites == true)
            {
                if(this.categories.Count == 10)
                    this.categories.Add(FavoriteCategory.Removed);
                this.cbCategory.SelectedIndex = this.Gallery.FavoriteCategory.Index;
                if(this.Gallery.FavoriteNote == null)
                    await loadNotesAsync();
            }
            else
            {
                if(this.categories.Count == 11)
                    this.categories.RemoveAt(10);
                this.cbCategory.SelectedIndex = 0;
            }
        }

        private async Task<bool> loadNotesAsync()
        {
            var success = false;
            try
            {
                this.pbLoading.IsIndeterminate = true;
                this.tbNote.Text = await this.Gallery.FetchFavoriteNoteAsync();
                this.cbCategory.SelectedIndex = this.Gallery.FavoriteCategory.Index;
                success = true;
            }
            catch(Exception ex)
            {
                this.tbInfo.Text = ex.GetMessage();
            }
            finally
            {
                this.pbLoading.IsIndeterminate = false;
            }
            return success;
        }

        private static readonly ExViewer_ResourceInfo.Resources.Views.IAddToFavoritesDialog strings
            = Strings.Resources.Views.AddToFavoritesDialog;

        private void tbNote_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.tbInfo.Text = "";
        }

        private void cbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.tbInfo.Text = "";
        }
    }
}
