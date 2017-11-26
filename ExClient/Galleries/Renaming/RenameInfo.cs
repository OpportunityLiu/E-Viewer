using Opportunity.MvvmUniverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExClient.Api;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;
using ExClient.Internal;

namespace ExClient.Galleries.Renaming
{
    public class RenameInfo : ObservableObject
    {
        public RenameInfo(GalleryInfo galleryInfo) => this.GalleryInfo = galleryInfo;

        public GalleryInfo GalleryInfo { get; }

        public static IAsyncOperation<RenameInfo> FetchAsync(GalleryInfo galleryInfo)
        {
            //TODO:
            return AsyncInfo.Run(async token =>
            {
                var uri = new Uri($"https://e-hentai.org/gallerypopups.php?gid={galleryInfo.ID}&t={galleryInfo.Token.ToTokenString()}8&act=rename");
                var r = new RenameInfo(galleryInfo);
            });
        }
    }
}
