using ExClient.Galleries;
using ExClient.Internal;
using ExClient.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ExClient
{
    internal static class ThumbHelper
    {
        static ThumbHelper()
        {
            CoreApplication.MainView.Dispatcher.Begin(() =>
            {
                Display = DisplayInformation.GetForCurrentView();
                DefaultThumb = new BitmapImage(Config.DefaultThumbUri);
            });
        }

        public static DisplayInformation Display { get; private set; }

        public static BitmapImage DefaultThumb { get; private set; }

        /// <summary>
        /// Get thumb image of a gallery with local cache.
        /// </summary>
        /// <param name="exact"><see langword="true"/> if thumb of first image of the gallery must be used, otherwise, first cached image in the galley will be used.</param>
        /// <param name="bitmap">Bitmap to write thumb data into.</param>
        /// <returns>Local thumb found.</returns>
        private static async Task<bool> getThumbLocalilyAsync(Gallery gallery, bool exact, BitmapImage bitmap)
        {
            GalleryImageModel getImageModel(long gId, bool ex)
            {
                using (var db = new GalleryDb())
                {
                    db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                    if (ex)
                        return db.GalleryImageSet
                            .Include(gi => gi.Image)
                            .Where(gi => gi.GalleryId == gId && gi.PageId == 1)
                            .FirstOrDefault();
                    else
                        return db.GalleryImageSet
                            .Include(gi => gi.Image)
                            .Where(gi => gi.GalleryId == gId)
                            .OrderBy(gi => gi.PageId)
                            .FirstOrDefault();
                }
            }

            var imageModel = getImageModel(gallery.ID, exact);
            if (imageModel is null)
                return false;
            var file = await StorageHelper.ImageFolder.TryGetFileAsync(imageModel.Image.FileName);
            if (file is null)
                return false;
            try
            {
                using (var stream = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.SingleItem))
                {
                    await bitmap.SetSourceAsync(stream);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static async Task<bool> getThumbRemoteAsync(Gallery gallery, BitmapImage bitmap)
        {
            return await ThumbClient.FetchThumbAsync(gallery.ThumbUri, bitmap);
        }

        public static async Task<ImageSource> GetThumbAsync(this Gallery gallery)
        {
            await CoreApplication.MainView.Dispatcher.Yield();
            var image = new BitmapImage();
            if (await getThumbLocalilyAsync(gallery, true, image))
                return image;
            if (await getThumbRemoteAsync(gallery, image))
                return image;
            if (await getThumbLocalilyAsync(gallery, false, image))
                return image;
            return DefaultThumb;
        }
    }

}
