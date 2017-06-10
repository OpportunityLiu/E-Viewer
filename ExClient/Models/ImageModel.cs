using ExClient.Galleries;
using System.Collections.Generic;

namespace ExClient.Models
{
    class ImageModel
    {
        /// <summary>
        /// SHA1 hash of the original image file.
        /// </summary>
        public string ImageId { get; set; }

        public bool OriginalLoaded { get; set; }

        public string FileName { get; set; }

        public IList<GalleryImageModel> UsingBy { get; set; }

        public ImageModel Update(GalleryImage image)
        {
            this.ImageId = image.ImageHash.ToString();
            this.FileName = image.ImageFile.Name;
            this.OriginalLoaded = image.OriginalLoaded;
            return this;
        }
    }
}
