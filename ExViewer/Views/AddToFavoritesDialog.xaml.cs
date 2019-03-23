using ExClient;
using ExClient.Galleries;
using ExViewer.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace ExViewer.Views
{
    public sealed partial class AddToFavoritesDialog : MyContentDialog
    {
        public AddToFavoritesDialog()
        {
            InitializeComponent();
            PrimaryButtonText = Strings.Resources.General.OK;
            CloseButtonText = Strings.Resources.General.Cancel;
            foreach (var item in Client.Current.Favorites)
            {
                categories.Add(item);
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
            pbLoading.IsIndeterminate = true;
            tbInfo.Text = "";
            var def = args.GetDeferral();
            try
            {
                if (cbCategory.SelectedItem is FavoriteCategory cat)
                {
                    await cat.AddAsync(Gallery, tbNote.Text);
                }
                else
                {
                    throw new Exception(strings.FavoriteCategoryUnselected);
                }
            }
            catch (Exception ex)
            {
                tbInfo.Text = ex.GetMessage();
                Telemetry.LogException(ex);
                args.Cancel = true;
            }
            finally
            {
                def.Complete();
                pbLoading.IsIndeterminate = false;
            }
        }

        private async void MyContentDialog_Loading(FrameworkElement sender, object args)
        {
            if (Gallery is null)
            {
                throw new InvalidOperationException("Property AddToFavoritesDialog.Gallery is null.");
            }

            var galleryInFavorites = Gallery.FavoriteCategory is null ? null : (bool?)(Gallery.FavoriteCategory.Index >= 0);
            tbInfo.Text = "";
            tbNote.Text = Gallery.FavoriteNote ?? "";
            cbCategory.SelectedItem = cbCategory.Items.FirstOrDefault();
            Title = galleryInFavorites == true ? strings.ModifyTitle : strings.AddTitle;
            await Dispatcher.YieldIdle();
            if (galleryInFavorites is null)
            {
                if (!await loadNotesAsync())
                {
                    return;
                }

                galleryInFavorites = Gallery.FavoriteCategory.Index >= 0;
                Title = galleryInFavorites == true ? strings.ModifyTitle : strings.AddTitle;
            }
            if (galleryInFavorites == true)
            {
                if (categories.Count == 10)
                {
                    categories.Add(Client.Current.Favorites.Removed);
                }

                cbCategory.SelectedIndex = Gallery.FavoriteCategory.Index;
                if (Gallery.FavoriteNote is null)
                {
                    await loadNotesAsync();
                }
            }
            else
            {
                if (categories.Count == 11)
                {
                    categories.RemoveAt(10);
                }

                cbCategory.SelectedIndex = 0;
            }
        }

        private async Task<bool> loadNotesAsync()
        {
            var success = false;
            try
            {
                pbLoading.IsIndeterminate = true;
                tbNote.Text = await Gallery.FetchFavoriteNoteAsync();
                cbCategory.SelectedIndex = Gallery.FavoriteCategory.Index;
                success = true;
            }
            catch (Exception ex)
            {
                tbInfo.Text = ex.GetMessage();
                Telemetry.LogException(ex);
            }
            finally
            {
                pbLoading.IsIndeterminate = false;
            }
            return success;
        }

        private static readonly ResourceInfo.Resources.Views.IAddToFavoritesDialog strings
            = Strings.Resources.Views.AddToFavoritesDialog;

        private void tbNote_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbInfo.Text = "";
        }

        private void cbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tbInfo.Text = "";
        }

        private async void MyContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // resize the dialog.
            var d = args.GetDeferral();
            await Task.Delay(33);
            d.Complete();
        }
    }
}
