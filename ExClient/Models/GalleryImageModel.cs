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
            get => ImageModel.FromStorage(Data0, Data1, Data2);
            set => (Data0, Data1, Data2) = ImageModel.ToStorage(value);
        }

        public ImageModel Image { get; set; }

        internal GalleryImageModel Update(GalleryImage galleryImage)
        {
            GalleryId = galleryImage.Owner.ID;
            PageId = galleryImage.PageID;
            ImageId = galleryImage.ImageHash;
            return this;
        }
    }
}
