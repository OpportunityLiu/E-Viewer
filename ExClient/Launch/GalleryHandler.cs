using ExClient.Api;
using Opportunity.MvvmUniverse.Helpers;
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
            return AsyncWarpper.Create((LaunchResult)new GalleryLaunchResult(info, -1, GalleryLaunchStatus.Default));
        }
    }
}
