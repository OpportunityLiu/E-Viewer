using ExClient.Api;
using Opportunity.Helpers.Universal.AsyncHelpers;
using Windows.Foundation;

namespace ExClient.Launch
{
    internal class GalleryHandler : UriHandler
    {

        public override bool CanHandle(UriHandlerData data)
        {
            return GalleryInfo.TryParseGallery(data, out var info);
        }

        public override IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data)
        {
            GalleryInfo.TryParseGallery(data, out var info);
            return AsyncOperation<LaunchResult>.CreateCompleted(new GalleryLaunchResult(info, -1, GalleryLaunchStatus.Default));
        }
    }
}
