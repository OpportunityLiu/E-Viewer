using ExClient.Galleries;
using System;

namespace ExClient.Models
{
    internal class SavedGalleryModel
    {
        public SavedGalleryModel Update(Gallery gallery)
        {
            GalleryId = gallery.ID;
            Saved = DateTimeOffset.UtcNow;
            return this;
        }

        public long saved;

        public DateTimeOffset Saved
        {
            get => DateTimeOffset.FromUnixTimeMilliseconds(saved);
            set => this.saved = value.ToUnixTimeMilliseconds();
        }

        public GalleryModel Gallery { get; set; }

        public long GalleryId { get; set; }
    }
}
