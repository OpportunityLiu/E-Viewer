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

    public class GalleryImage : ObservableObject
    {

        private static string showKey
        {
            get;
            set;
        }
        private static int serverIndex;

        private IAsyncOperation<bool> loadShowKey(bool reload)
        {
            return Run(async token =>
            {
                try
                {
                    if(showKey == null)
                        reload = false;
                    var posturi = reload ? new Uri(this.PageUri, $"?nl={serverIndex.ToString()}-{showKey.Split('-')[0]}") : this.PageUri;
                    var res = this.Owner.Owner.PostStrAsync(this.PageUri, null);
                    token.Register(res.Cancel);
                    var ans = await res;
                    var regShowKey = Regex.Match(ans, @"var\s+showkey\s*=\s*""([-\w]+)""\s*;");
                    if(!regShowKey.Success)
                        return false;
                    var regServerIndex = Regex.Match(ans, @"var\s+si\s*=\s*([-\w]+)\s*;");
                    if(!regServerIndex.Success)
                        return false;
                    showKey = regShowKey.Groups[1].Value;
                    serverIndex = int.Parse(regServerIndex.Groups[1].Value, System.Globalization.NumberStyles.Integer);
                    return true;
                }
                catch(Exception)
                {
                    return false;
                }
            });
        }

        private class showImgApiResult
        {
            public int p;
            public string s;
            public string n;
            public string i;
            public string k;
            public string i3;
            public string i5;
            public string i6;
            public string i7;
            public long si;
            public string x;
            public string y;
            public string error;
        }

        private IAsyncAction loadImageUri(bool firstChance)
        {
            return Run(async token =>
            {
                IAsyncOperation<bool> lShowKey = null;
                IAsyncOperationWithProgress<string, HttpProgress> lApi = null;
                token.Register(() =>
                {
                    lShowKey?.Cancel();
                    lApi?.Cancel();
                });
                if(!firstChance || showKey == null)
                {
                    lShowKey = loadShowKey(!firstChance);
                    if(!await lShowKey)
                    {
                        if(!firstChance)
                            throw new InvalidOperationException("Can't load uri.");
                        await loadImageUri(false);
                    }
                }
                var req = new
                {
                    method = "showpage",
                    gid = Owner.Id,
                    page = Page,
                    imgkey = imageKey,
                    showkey = showKey
                };
                lApi = Owner.Owner.PostApiAsync(JsonConvert.SerializeObject(req));
                string api = null;
                try
                {
                    api = await lApi;
                }
                catch
                {
                    if(!firstChance)
                        throw;
                    await loadImageUri(false);
                }
                var result = JsonConvert.DeserializeObject<showImgApiResult>(api);
                if(result.error != null)
                {
                    if(!firstChance)
                        throw new InvalidOperationException("Can't load uri.");
                    await loadImageUri(false);
                }
                var doc = new HtmlDocument();
                doc.LoadHtml(result.i3);
                imageUri = new Uri(WebUtility.HtmlDecode(doc.GetElementbyId("img").GetAttributeValue("src", "")));
            });
        }

        private GalleryImage(Gallery owner, int page, string imageKey)
        {
            this.Owner = owner;
            this.Page = page;
            this.imageKey = imageKey;
            this.PageUri = new Uri(pageBaseUri, $"{imageKey}/{owner.Id.ToString()}-{page.ToString()}");
        }

        internal GalleryImage(Gallery owner, int page, string imageKey, ImageSource thumb)
            : this(owner, page, imageKey)
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
                using(var stream = await file.OpenReadAsync())
                {
                    await thumb.SetSourceAsync(stream);
                }
                image.imageFile = file;
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

        public int Page
        {
            get; private set;
        }

        public Uri PageUri
        {
            get; private set;
        }

        public IAsyncAction LoadImage(bool reload)
        {
            return Run(async token =>
            {
                IAsyncAction load;
                switch(state)
                {
                case ImageLoadingState.Waiting:
                    load = loadImageUri(true);
                    break;
                case ImageLoadingState.Failed:
                    load = loadImageUri(false);
                    break;
                case ImageLoadingState.Loaded:
                    if(reload)
                        load = loadImageUri(false);
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
                    var imageLoad = Owner.Owner.HttpClient.GetBufferAsync(imageUri);
                    this.State = ImageLoadingState.Loading;
                    imageLoad.Progress = async (sender, progress) =>
                    {
                        if(progress.TotalBytesToReceive == null || progress.TotalBytesToReceive == 0)
                            await DispatcherHelper.RunLowAsync(() => this.Progress = 0);
                        else
                        {
                            var pro = (int)(progress.BytesReceived * 100 / ((ulong)progress.TotalBytesToReceive));
                            await DispatcherHelper.RunLowAsync(() => this.Progress = pro);
                        }
                    };
                    var buffer = await imageLoad;
                    var save = CacheHelper.SaveFileAsync(Owner.Id.ToString(), Page.ToString(), buffer);
                    imageFile = await save;
                    this.State = ImageLoadingState.Loaded;
                }
                catch(Exception)
                {
                    this.Progress = 100;
                    State = ImageLoadingState.Failed;
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

        private StorageFile _imageFile;

        private StorageFile imageFile
        {
            get
            {
                return _imageFile;
            }
            set
            {
                _imageFile = value;
                RaisePropertyChanged(nameof(Image));
            }
        }

        private WeakReference<BitmapImage> image;

        public ImageSource Image
        {
            get
            {
                if(imageFile == null)
                    return null;
                BitmapImage image;
                if(this.image != null && this.image.TryGetTarget(out image))
                    return image;
                image = new BitmapImage();
                this.image = new WeakReference<BitmapImage>(image);
                var loadStream = imageFile.OpenReadAsync();
                loadStream.Completed = async (op, e) =>
                {
                    if(e != AsyncStatus.Completed)
                        return;
                    await DispatcherHelper.RunLowAsync(() =>
                    {
                        using(var stream = op.GetResults())
                        {
                            image.SetSource(stream);
                        }
                    });
                };
                return image;
            }
        }

        private static readonly Uri pageBaseUri = new Uri(Client.RootUri, "s/");

        private string imageKey;

        internal string ImageKey => imageKey;
    }
}
