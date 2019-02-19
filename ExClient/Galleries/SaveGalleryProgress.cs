namespace ExClient.Galleries
{
    public readonly struct SaveGalleryProgress
    {
        internal SaveGalleryProgress(int imageLoaded, int imageCount)
        {
            ImageCount = imageCount;
            ImageLoaded = imageLoaded;
        }

        public int ImageLoaded { get; }

        public int ImageCount { get; }
    }
}