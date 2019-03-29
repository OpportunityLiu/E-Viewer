using ExClient.Api;
using Opportunity.Helpers.Universal.AsyncHelpers;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ExClient.Launch
{
    internal class GalleryHandler : UriHandler
    {
        public override bool CanHandle(UriHandlerData data)
        {
            return GalleryInfo.TryParseGallery(data, out var info);
        }

        public override Task<LaunchResult> HandleAsync(UriHandlerData data)
        {
            GalleryInfo.TryParseGallery(data, out var info);
            return Task.FromResult<LaunchResult>(new GalleryLaunchResult(info, -1, GalleryLaunchStatus.Default));
        }
    }
}
