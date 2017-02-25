using System;

namespace ExClient.Models
{
    internal class SavedGalleryModel
    {
        public SavedGalleryModel Update(Gallery gallery, byte[] thumbData)
        {
            GalleryId = gallery.Id;
            ThumbData = thumbData;
            Saved = DateTimeOffset.UtcNow;
            return this;
        }

        public DateTimeOffset Saved
        {
            get; set;
        }

        public GalleryModel Gallery
        {
            get; set;
        }

        public long GalleryId
        {
            get; set;
        }

        public byte[] ThumbData
        {
            get; set;
        }
    }
}
