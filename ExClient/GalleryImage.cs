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
using System.IO;
using GalaSoft.MvvmLight.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using ExClient.Internal;

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

    [System.Diagnostics.DebuggerDisplay(@"\{PageId = {PageId} State = {State} File = {ImageFile?.Name}\}")]
    public class GalleryImage : ObservableObject
    {
        static GalleryImage()
        {
            DispatcherHelper.CheckBeginInvokeOnUI(async () =>
            {
                var info = Windows.Graphics.Display.DisplayInformation.GetForCurrentView();
                thumbWidth = (uint)(100 * info.RawPixelsPerViewPixel);
                defaultThumb = new BitmapImage();
                using(var stream = await StorageHelper.GetIconOfExtension("jpg"))
                {
                    await defaultThumb.SetSourceAsync(stream);
                }
            });
        }

        private static uint thumbWidth = 100;
        private static BitmapImage defaultThumb;

        internal static IAsyncOperation<GalleryImage> LoadCachedImageAsync(Gallery owner, Models.ImageModel model)
        {
            return Run(async token =>
            {
                var imageFile = await owner.GalleryFolder.TryGetFileAsync(model.FileName);
                if(imageFile == null)
                    return null;
                var img = new GalleryImage(owner, model.PageId, model.ImageKey, null)
                {
                    ImageFile = imageFile,
                    OriginalLoaded = model.OriginalLoaded,
                    Progress = 100,
                    State = ImageLoadingState.Loaded
                };
                return img;
            });
        }

        internal GalleryImage(Gallery owner, int pageId, string imageKey, ImageSource thumb)
        {
            this.Owner = owner;
            this.PageId = pageId;
            this.imageKey = imageKey;
            this.PageUri = new Uri(pageBaseUri, $"{imageKey}/{owner.Id.ToString()}-{pageId.ToString()}");
            this.image = new ImageHandle(img =>
            {
                return Run(async token =>
                {
                    try
                    {
                        using(var stream = await ImageFile.OpenReadAsync())
                        {
                            await img.SetSourceAsync(stream);
                        }
                    }
                    catch(FileNotFoundException)
                    {
                        ImageFile = null;
                        State = ImageLoadingState.Waiting;
                    }
                    catch(Exception)
                    {
                        this.State = ImageLoadingState.Failed;
                    }
                });
            });
            this.thumbSource = thumb;
            this.thumb = new ImageHandle(img =>
            {
                return Run(async token =>
                {
                    img.DecodePixelType = DecodePixelType.Logical;
                    img.DecodePixelWidth = 100;
                    try
                    {
                        using(var stream = await this.imageFile.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.SingleItem, thumbWidth * 18 / 10))
                        {
                            await img.SetSourceAsync(stream);
                        }
                    }
                    catch
                    {
                        using(var stream = await StorageHelper.GetIconOfExtension("jpg"))
                        {
                            await img.SetSourceAsync(stream);
                        }
                    }
                });
            });
            this.thumb.ImageLoaded += Thumb_ImageLoaded;
        }

        private void Thumb_ImageLoaded(object sender, EventArgs e)
        {
            thumbSource = null;
            RaisePropertyChanged(nameof(Thumb));
        }

        private static readonly Regex failTokenMatcher = new Regex(@"return\s+nl\(\s*'(.+?)'\s*\)", RegexOptions.Compiled);

        private IAsyncAction loadImageUri()
        {
            return Task.Run(async () =>
            {
                var loadPageUri = PageUri;
                if(failToken != null)
                    loadPageUri = new Uri(pageBaseUri, $"{imageKey}/{Owner.Id.ToString()}-{PageId.ToString()}?nl={failToken}");
                var loadPage = Owner.Owner.PostStrAsync(loadPageUri, null);
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
                failToken = failTokenMatcher.Match(loadFail).Groups[1].Value;
            }).AsAsyncAction();
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

        private readonly ImageHandle thumb;
        private ImageSource thumbSource;

        public ImageSource Thumb
        {
            get
            {
                if(thumbSource != null)
                    return thumbSource;
                if(thumb.Loaded)
                    return thumb.Image;
                thumb.StartLoading();
                return defaultThumb;
            }
        }

        public Gallery Owner
        {
            get;
        }

        public int PageId
        {
            get;
        }

        public Uri PageUri
        {
            get;
        }

        private IAsyncAction loadImageAction;

        public IAsyncAction LoadImageAsync(bool reload, ConnectionStrategy strategy, bool throwIfFailed)
        {
            var previousAction = loadImageAction;
            return loadImageAction = Run(async token =>
            {
                IAsyncAction load;
                IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> imageLoad = null;
                switch(state)
                {
                case ImageLoadingState.Waiting:
                case ImageLoadingState.Failed:
                    load = loadImageUri();
                    break;
                case ImageLoadingState.Loading:
                case ImageLoadingState.Loaded:
                    if(reload)
                    {
                        if(previousAction?.Status == AsyncStatus.Started)
                            previousAction.Cancel();
                        await deleteImageFile();
                        load = loadImageUri();
                    }
                    else
                        return;
                    break;
                default:
                    return;
                }
                token.Register(() =>
                {
                    load.Cancel();
                    imageLoad?.Cancel();
                });
                this.State = ImageLoadingState.Preparing;
                this.Progress = 0;
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
                    imageLoad = Owner.Owner.HttpClient.GetAsync(uri);
                    this.State = ImageLoadingState.Loading;
                    imageLoad.Progress = (sender, progress) =>
                    {
                        if(this.State == ImageLoadingState.Loaded)
                        {
                            sender.Cancel();
                            return;
                        }
                        if(progress.TotalBytesToReceive == null || progress.TotalBytesToReceive == 0)
                            this.Progress = 0;
                        else
                        {
                            var pro = (int)(progress.BytesReceived * 100 / ((ulong)progress.TotalBytesToReceive));
                            this.Progress = pro;
                        }
                    };
                    token.ThrowIfCancellationRequested();
                    await deleteImageFile();
                    var imageLoadResponse = await imageLoad;
                    if(imageLoadResponse.Content.Headers.ContentType.MediaType == "text/html")
                        throw new InvalidOperationException(HtmlUtilities.ConvertToText(imageLoadResponse.Content.ToString()));
                    token.ThrowIfCancellationRequested();
                    var buffer = await imageLoadResponse.Content.ReadAsBufferAsync();
                    var ext = Path.GetExtension(imageLoadResponse.RequestMessage.RequestUri.LocalPath);
                    var save = Owner.GalleryFolder.SaveFileAsync($"{PageId}{ext}", buffer);
                    ImageFile = await save;
                    using(var db = new Models.GalleryDb())
                    {
                        var myModel = db.ImageSet.SingleOrDefault(model => model.OwnerId == this.Owner.Id && model.PageId == this.PageId);
                        if(myModel == null)
                        {
                            db.ImageSet.Add(new Models.ImageModel().Update(this));
                        }
                        else
                        {
                            db.ImageSet.Update(myModel.Update(this));
                        }
                        db.SaveChanges();
                    }
                    this.State = ImageLoadingState.Loaded;
                }
                catch(TaskCanceledException) { }
                catch(Exception)
                {
                    this.Progress = 100;
                    State = ImageLoadingState.Failed;
                    if(throwIfFailed)
                        throw;
                }
            });
        }

        private IAsyncAction deleteImageFile()
        {
            return Run(async token =>
            {
                if(ImageFile != null)
                {
                    var file = ImageFile;
                    ImageFile = null;
                    await file.DeleteAsync();
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

        private StorageFile imageFile;

        public StorageFile ImageFile
        {
            get
            {
                return imageFile;
            }
            protected set
            {
                Set(ref imageFile, value);
                image.Reset();
                RaisePropertyChanged(nameof(Image));
                RaisePropertyChanged(nameof(ImageFileUri));
                if(value != null)
                {
                    thumb.Reset();
                    thumb.StartLoading();
                }
            }
        }

        private static Uri ImageBaseUri = new Uri("ms-appdata:///localCache/");

        public Uri ImageFileUri
        {
            get
            {
                if(imageFile == null)
                    return null;
                return new Uri(ImageBaseUri, $"{Owner.Id}/{imageFile.Name}");
            }
        }

        private readonly ImageHandle image;

        public BitmapImage Image
        {
            get
            {
                if(ImageFile == null)
                    return null;
                return this.image.Image;
            }
        }

        private static readonly Uri pageBaseUri = new Uri(Client.RootUri, "s/");

        private string imageKey;

        internal string ImageKey => imageKey;

        private string failToken;

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
