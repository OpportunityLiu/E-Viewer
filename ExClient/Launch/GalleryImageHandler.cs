using ExClient.Api;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Launch
{
    internal sealed class GalleryImageHandler : GalleryHandler
    {
        public override bool CanHandle(UriHandlerData data)
        {
            return ImageInfo.TryParse(data, out var info);
        }

        public override async Task<LaunchResult> HandleAsync(UriHandlerData data)
        {
            ImageInfo.TryParse(data, out var info);
            var gInfo = await info.FetchGalleryInfoAsync();
            return new GalleryLaunchResult(gInfo, info.PageId - 1, GalleryLaunchStatus.Image);
        }
    }
}
