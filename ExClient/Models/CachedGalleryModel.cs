using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient.Models
{
    internal class CachedGalleryModel
    {
        public CachedGalleryModel Update(Gallery gallery, byte[] thumbData)
        {
            GalleryId = gallery.Id;
            ThumbData = thumbData;
            return this;
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
