namespace ExClient.Launch
{
    public abstract class LaunchResult
    {
        internal LaunchResult() { }
    }

    public enum GalleryLaunchStatus
    {
        Default,
        Image,
        Torrent
    }

    public sealed class GalleryLaunchResult : LaunchResult
    {
        public Gallery Gallery
        {
            get;
        }

        public int CurrentIndex
        {
            get;
        }

        public GalleryLaunchStatus Status
        {
            get;
        }

        internal GalleryLaunchResult(Gallery g, int index, GalleryLaunchStatus status)
        {
            this.Gallery = g;
            this.CurrentIndex = index;
            this.Status = status;
        }
    }

    public sealed class SearchLaunchResult : LaunchResult
    {
        internal SearchLaunchResult(SearchResult data)
        {
            this.Data = data;
        }

        public SearchResult Data
        {
            get;
        }
    }

    public sealed class FavoritesSearchLaunchResult : LaunchResult
    {
        internal FavoritesSearchLaunchResult(FavoritesSearchResult data)
        {
            this.Data = data;
        }

        public FavoritesSearchResult Data
        {
            get;
        }
    }
}
