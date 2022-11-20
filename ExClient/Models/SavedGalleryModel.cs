﻿using ExClient.Galleries;

using System;

namespace ExClient.Models
{
    internal class SavedGalleryModel
    {
        public SavedGalleryModel Update(Gallery gallery)
        {
            GalleryId = gallery.Id;
            Saved = DateTimeOffset.UtcNow;
            return this;
        }

        public long saved;

        public DateTimeOffset Saved
        {
            get => DateTimeOffset.FromUnixTimeMilliseconds(saved);
            set => saved = value.ToUnixTimeMilliseconds();
        }

        public GalleryModel Gallery { get; set; }

        public long GalleryId { get; set; }
    }
}
