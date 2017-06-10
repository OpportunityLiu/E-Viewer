using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExClient.Galleries;

namespace ExClient.Models
{
    class GalleryImageModel
    {
        public long GalleryId { get; set; }
        public GalleryModel Gallery { get; set; }

        /// <summary>
        /// 1-based Id for image.
        /// </summary>
        public int PageId { get; set; }

        public string ImageId { get; set; }
        public ImageModel Image { get; set; }

        internal GalleryImageModel Update(GalleryImage galleryImage)
        {
            this.GalleryId = galleryImage.Owner.Id;
            this.PageId = galleryImage.PageId;
            this.ImageId = galleryImage.ImageHash.ToString();
            return this;
        }
    }
}
