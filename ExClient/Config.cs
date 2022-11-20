using System;
using System.Threading.Tasks;

using Windows.Storage;

namespace ExClient
{
    public static class Config
    {
        private static Uri defaultThumbUri;
        public static Uri DefaultThumbUri
        {
            get => defaultThumbUri;
            set
            {
                defaultThumbUri = value;
                if (ThumbHelper.DefaultThumb != null)
                    ThumbHelper.DefaultThumb.UriSource = value;
            }
        }

        public static Task SetImageFolderAsync(StorageFolder folder)
        {
            return StorageHelper.SetImageFolderAsync(folder);
        }
    }
}
