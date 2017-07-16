namespace ExClient.Galleries
{
    public struct SaveGalleryProgress
    {
        internal int ImageLoadedInternal;

        public int ImageLoaded => this.ImageLoadedInternal;

        public int ImageCount
        {
            get; internal set;
        }
    }
}