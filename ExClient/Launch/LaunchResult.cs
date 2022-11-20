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
            GalleryInfo = gInfo;
            CurrentIndex = index;
            Status = status;
        }
    }

    public sealed class SearchLaunchResult : LaunchResult
    {
        internal SearchLaunchResult(SearchResult data)
        {
            Data = data;
        }

        public SearchResult Data { get; }
    }

    public sealed class PopularLaunchResult : LaunchResult
    {
        private PopularLaunchResult() { }

        internal static PopularLaunchResult Instance { get; } = new PopularLaunchResult();
    }
}
