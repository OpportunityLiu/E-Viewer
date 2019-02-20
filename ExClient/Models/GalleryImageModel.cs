using ExClient.Galleries;
using System;
using System.Linq.Expressions;

namespace ExClient.Models
{
    class GalleryImageModel
    {
        public static Expression<Func<GalleryImageModel, bool>> FKEquals(SHA1Value value)
        {
            var (d0, d1, d2) = ImageModel.ToStorage(value);
            return m => m.Data0 == d0 && m.Data1 == d1 && m.Data2 == d2;
        }

        public long GalleryId { get; set; }
        public GalleryModel Gallery { get; set; }

        /// <summary>
        /// 1-based Id for image.
        /// </summary>
        public int PageId { get; set; }


        public ulong Data0, Data1;
        public uint Data2;
        /// <summary>
        /// SHA1 hash of the original image file.
        /// </summary>
        public SHA1Value ImageId
        {
            get => ImageModel.FromStorage(this.Data0, this.Data1, this.Data2);
            set => (this.Data0, this.Data1, this.Data2) = ImageModel.ToStorage(value);
        }

        public ImageModel Image { get; set; }

        internal GalleryImageModel Update(GalleryImage galleryImage)
        {
            this.GalleryId = galleryImage.Owner.Id;
            this.PageId = galleryImage.PageId;
            this.ImageId = galleryImage.ImageHash;
            return this;
        }
    }
}
