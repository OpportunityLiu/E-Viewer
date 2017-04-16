using ExClient.Api;
using System;
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

        public override IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data)
        {
            ImageInfo.TryParse(data, out var info);
            return Run(async token =>
            {
                var gInfo = await info.FetchGalleryInfoAsync();
                return (LaunchResult)new GalleryLaunchResult(gInfo, info.PageId, GalleryLaunchStatus.Image);
            });
        }
    }
}
