namespace ExClient.Galleries
{
    public struct SaveGalleryProgress
    {
        internal SaveGalleryProgress(int imageLoaded, int imageCount)
        {
            this.ImageCount = imageCount;
            this.ImageLoaded = imageLoaded;
        }

        public int ImageLoaded { get; }

        public int ImageCount { get; }
    }
}