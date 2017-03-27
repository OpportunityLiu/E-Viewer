namespace ExClient.Models
{
    class ImageModel
    {
        public ulong ImageKey
        {
            get;
            set;
        }

        public bool OriginalLoaded
        {
            get;
            set;
        }
        
        public long OwnerId
        {
            get;
            set;
        }
        
        public GalleryModel Owner
        {
            get;
            set;
        }

        /// <summary>
        /// 1-based Id for image.
        /// </summary>
        public int PageId
        {
            get;
            set;
        }

        public string FileName
        {
            get;
            set;
        }

        public ImageModel Update(GalleryImage galleryImage)
        {
            this.ImageKey = galleryImage.ImageKey;
            this.FileName = galleryImage.ImageFile.Name;
            this.OriginalLoaded = galleryImage.OriginalLoaded;
            this.OwnerId = galleryImage.Owner.Id;
            this.PageId = galleryImage.PageId;
            return this;
        }
    }
}
