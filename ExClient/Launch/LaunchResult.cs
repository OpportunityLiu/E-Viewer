using ExClient.Search;

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
        Torrent,
        Archive,
        Rename,
        Expunge,
        Favorite,
        Stats
    }

    public sealed class GalleryLaunchResult : LaunchResult
    {
        public Api.GalleryInfo GalleryInfo { get; }

        public int CurrentIndex { get; }

        public GalleryLaunchStatus Status { get; }

        internal GalleryLaunchResult(Api.GalleryInfo gInfo, int index, GalleryLaunchStatus status)
        {
            this.GalleryInfo = gInfo;
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

        public SearchResult Data { get; }
    }
}
