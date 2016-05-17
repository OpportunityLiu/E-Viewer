using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using System.Linq;
using Windows.Data.Html;

namespace ExClient
{
    public enum ImageLoadingState
    {
        Waiting,
        Preparing,
        Loading,
        Loaded,
        Failed
    }

    public enum ConnectionStrategy
    {
        AllLofi,
        LofiOnMetered,
        AllFull
    }

    public class GalleryImage : ObservableObject
    {
        private IAsyncAction loadImageUri()
        {
            return Run(async token =>
            {
                var loadPage = Owner.Owner.PostStrAsync(PageUri, null);
                token.Register(loadPage.Cancel);
                var pageResult = new HtmlDocument();
                pageResult.LoadHtml(await loadPage);

                imageUri = new Uri(HtmlUtilities.ConvertToText(pageResult.GetElementbyId("img").GetAttributeValue("src", "")));
                var originalNode = pageResult.GetElementbyId("i7").Descendants("a").FirstOrDefault();
                if(originalNode == null)
                {
                    originalImageUri = null;
                }
                else
                {
                    originalImageUri = new Uri(HtmlUtilities.ConvertToText(originalNode.GetAttributeValue("href", "")));
                }
                var loadFail = pageResult.GetElementbyId("loadfail").GetAttributeValue("onclick", "");
                var failToken = Regex.Match(loadFail, @"return\s+nl\(\s*'(.+?)'\s*\)").Groups[1].Value;
                PageUri = new Uri(pageBaseUri, $"{imageKey}/{Owner.Id.ToString()}-{PageId.ToString()}?nl={failToken}");
            });
        }

        private GalleryImage(Gallery owner, int pageId, string imageKey)
        {
            this.Owner = owner;
            this.PageId = pageId;
            this.imageKey = imageKey;
            this.PageUri = new Uri(pageBaseUri, $"{imageKey}/{owner.Id.ToString()}-{pageId.ToString()}");
        }

        internal GalleryImage(Gallery owner, int pageId, string imageKey, ImageSource thumb)
            : this(owner, pageId, imageKey)
        {
            this.Thumb = thumb;
        }

        internal static IAsyncOperation<GalleryImage> LoadCachedImageAsync(Gallery owner, int page, string imageKey)
        {
            return Run(async token =>
            {
                var file = await CacheHelper.LoadFileAsync(owner.Id.ToString(), page.ToString());
                if(file == null)
                    return null;
                var image = new GalleryImage(owner, page, imageKey);
                var thumb = new BitmapImage();
                thumb.DecodePixelWidth = 100;
                thumb.DecodePixelType = DecodePixelType.Logical;
                var loadThumb = file.OpenReadAsync();
                loadThumb.Completed = async (sender, e) =>
                {
                    if(e != AsyncStatus.Completed)
                        return;
                    await DispatcherHelper.RunLowAsync(async () =>
                    {
                        using(var stream = sender.GetResults())
                        {
                            await thumb.SetSourceAsync(stream);
                        }
                    });
                };
                image.ImageFile = file;
                image.Thumb = thumb;
                image.Progress = 100;
                image.State = ImageLoadingState.Loaded;
                return image;
            });
        }

        private ImageLoadingState state;

        public ImageLoadingState State
        {
            get
            {
                return state;
            }
            private set
            {
                Set(ref state, value);
            }
        }

        public ImageSource Thumb
        {
            get;
            private set;
        }

        public Gallery Owner
        {
            get; private set;
        }

        public int PageId
        {
            get; private set;
        }

        public Uri PageUri
        {
            get;
            private set;
        }

        public IAsyncAction LoadImage(bool reload, ConnectionStrategy strategy, bool throwIfFailed)
        {
            return Run(async token =>
            {
                IAsyncAction load;
                switch(state)
                {
                case ImageLoadingState.Waiting:
                case ImageLoadingState.Failed:
                    load = loadImageUri();
                    break;
                case ImageLoadingState.Loaded:
                    if(reload)
                    {
                        ImageFile = null;
                        load = loadImageUri();
                    }
                    else
                        return;
                    break;
                default:
                    return;
                }
                token.Register(load.Cancel);
                this.State = ImageLoadingState.Preparing;
                try
                {
                    await load;
                    Uri uri = null;
                    var loadFull = !ConnectionHelper.IsLofiRequired(strategy);
                    if(loadFull)
                    {
                        uri = originalImageUri ?? imageUri;
                        OriginalLoaded = true;
                    }
                    else
                    {
                        uri = imageUri;
                        OriginalLoaded = (originalImageUri == null);
                    }
                    var imageLoad = Owner.Owner.HttpClient.GetBufferAsync(uri);
                    this.State = ImageLoadingState.Loading;
                    imageLoad.Progress = (sender, progress) =>
                    {
                        if(progress.TotalBytesToReceive == null || progress.TotalBytesToReceive == 0)
                            this.Progress = 0;
                        else
                        {
                            var pro = (int)(progress.BytesReceived * 100 / ((ulong)progress.TotalBytesToReceive));
                            this.Progress = pro;
                        }
                    };
                    var buffer = await imageLoad;
                    var save = CacheHelper.SaveFileAsync(Owner.Id.ToString(), PageId.ToString(), buffer);
                    ImageFile = await save;
                    this.State = ImageLoadingState.Loaded;
                }
                catch(Exception)
                {
                    this.Progress = 100;
                    State = ImageLoadingState.Failed;
                    if(throwIfFailed)
                        throw;
                }
            });
        }

        private int progress;

        public int Progress
        {
            get
            {
                return progress;
            }
            private set
            {
                Set(ref progress, value);
            }
        }

        private Uri imageUri;
        private Uri originalImageUri;

        private StorageFile _imageFile;

        public StorageFile ImageFile
        {
            get
            {
                return _imageFile;
            }
            protected set
            {
                _imageFile = value;
                image = null;
                RaisePropertyChanged(nameof(Image));
            }
        }

        private WeakReference<BitmapImage> image;

        public BitmapImage Image
        {
            get
            {
                if(ImageFile == null)
                    return null;
                BitmapImage image;
                if(this.image != null && this.image.TryGetTarget(out image))
                    return image;
                image = new BitmapImage();
                this.image = new WeakReference<BitmapImage>(image);
                var loadStream = ImageFile.OpenReadAsync();
                loadStream.Completed = async (op, e) =>
                {
                    if(e != AsyncStatus.Completed)
                        return;
                    await DispatcherHelper.RunLowAsync(async () =>
                    {
                        using(var stream = op.GetResults())
                        {
                            await image.SetSourceAsync(stream);
                        }
                    });
                };
                return image;
            }
        }

        private static readonly Uri pageBaseUri = new Uri(Client.RootUri, "s/");

        private string imageKey;

        internal string ImageKey => imageKey;

        public bool OriginalLoaded
        {
            get
            {
                return originalLoaded;
            }
            private set
            {
                Set(ref originalLoaded, value);
            }
        }

        private bool originalLoaded;
    }
}
