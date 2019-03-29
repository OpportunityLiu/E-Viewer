using ExClient.Api;
using Opportunity.Helpers.Universal.AsyncHelpers;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ExClient.Launch
{
    internal sealed class GalleryPopupHandler : GalleryHandler
    {
        public override bool CanHandle(UriHandlerData data)
        {
            return GalleryInfo.TryParseGalleryPopup(data, out var info, out _);
        }

        public override Task<LaunchResult> HandleAsync(UriHandlerData data)
        {
            GalleryInfo.TryParseGalleryPopup(data, out var info, out var type);
            return Task.FromResult<LaunchResult>(new GalleryLaunchResult(info, -1, type));
        }
    }
}
