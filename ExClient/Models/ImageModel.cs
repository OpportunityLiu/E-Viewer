using ExClient.Galleries;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExClient.Models
{
    class ImageModel
    {
        public static Expression<Func<ImageModel, bool>> PKEquals(SHA1Value value)
        {
            var (d0, d1, d2) = ToStorage(value);
            return m => m.Data0 == d0 && m.Data1 == d1 && m.Data2 == d2;
        }

        public static (ulong d0, ulong d1, uint d2) ToStorage(SHA1Value value)
        {
            var d = value.Data;
            var d0 = BitConverter.ToUInt64(d, 0);
            var d1 = BitConverter.ToUInt64(d, 8);
            var d2 = BitConverter.ToUInt32(d, 16);
            return (d0, d1, d2);
        }

        public static SHA1Value FromStorage(ulong d0, ulong d1, uint d2)
        {
            var d = new byte[20];
            var b0 = BitConverter.GetBytes(d0);
            var b1 = BitConverter.GetBytes(d1);
            var b2 = BitConverter.GetBytes(d2);
            Buffer.BlockCopy(b0, 0, d, 0, 8);
            Buffer.BlockCopy(b1, 0, d, 8, 8);
            Buffer.BlockCopy(b2, 0, d, 16, 4);
            return new SHA1Value(d);
        }

        public ulong Data0;
        public ulong Data1;
        public uint Data2;


        /// <summary>
        /// SHA1 hash of the original image file.
        /// </summary>
        public SHA1Value ImageId
        {
            get => FromStorage(Data0, Data1, Data2);
            set => (Data0, Data1, Data2) = ToStorage(value);
        }

        public bool OriginalLoaded { get; set; }

        public string FileName { get; set; }

        public IList<GalleryImageModel> UsingBy { get; set; }

        public ImageModel Update(GalleryImage image)
        {
            ImageId = image.ImageHash;
            FileName = image.ImageFile.Name;
            OriginalLoaded = image.OriginalLoaded;
            return this;
        }
    }
}
