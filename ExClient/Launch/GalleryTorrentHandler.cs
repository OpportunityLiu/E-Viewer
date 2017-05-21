using ExClient.Api;
using Opportunity.MvvmUniverse.AsyncHelpers;
using Windows.Foundation;

namespace ExClient.Launch
{
    internal sealed class GalleryTorrentHandler : GalleryHandler
    {
        public override bool CanHandle(UriHandlerData data)
        {
            return GalleryInfo.TryParseGalleryTorrent(data, out var info);
        }

        public override IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data)
        {
            GalleryInfo.TryParseGalleryTorrent(data, out var info);
            return AsyncWrapper.CreateCompleted((LaunchResult)new GalleryLaunchResult(info, -1, GalleryLaunchStatus.Torrent));
        }
    }
}
