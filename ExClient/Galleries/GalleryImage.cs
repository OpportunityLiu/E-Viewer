using ExClient.Internal;
using ExClient.Models;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Opportunity.Helpers.Universal.AsyncHelpers;
using Opportunity.MvvmUniverse;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Data.Html;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Galleries
{
    [System.Diagnostics.DebuggerDisplay(@"\{PageID = {PageID} State = {State} File = {ImageFile?.Name}\}")]
    public class GalleryImage : ObservableObject
    {
        static GalleryImage()
        {
            CoreApplication.MainView.Dispatcher.Begin(() =>
            {
                display = DisplayInformation.GetForCurrentView();
                createDefaultThumb();
            });
        }

        private static void createDefaultThumb()
        {
            CoreApplication.MainView.Dispatcher.Begin(async () =>
            {
                var b = new BitmapImage();
                var old = Interlocked.CompareExchange(ref defaultThumb, b, null);
                if (old != null)
                    return;
                await b.Dispatcher.YieldIdle();
                using (var stream = await StorageHelper.GetIconOfExtension("jpg"))
                {
                    await b.SetSourceAsync(stream);
                }
            });
        }

        private static ImageSource defaultThumb;
        public static ImageSource DefaultThumb
        {
            get => defaultThumb;
            set
            {
                if (value is null)
                    createDefaultThumb();
                else
                    defaultThumb = value;
            }
        }

        private static DisplayInformation display;

        private static StorageFolder imageFolder;
        public static StorageFolder ImageFolder
        {
            get => imageFolder;
            set => imageFolder = value;
        }

        protected internal static IAsyncOperation<StorageFolder> GetImageFolderAsync()
        {
            var temp = imageFolder;
            if (temp != null)
                return AsyncOperation<StorageFolder>.CreateCompleted(temp);
            return Run(async token =>
            {
                var temp2 = imageFolder;
                if (temp2 == null)
                {
                    temp2 = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);
                    imageFolder = temp2;
                }
                return temp2;
            });
        }

        internal IAsyncAction PopulateCachedImageAsync(GalleryImageModel galleryImageModel, ImageModel imageModel)
        {
            return Run(async token =>
            {
                var folder = ImageFolder ?? await GetImageFolderAsync();
                var hash = galleryImageModel.ImageId;
                this.ImageHash = hash;
                var imageFile = await folder.TryGetFileAsync(imageModel.FileName);
                if (imageFile != null)
                {
                    ImageFile = imageFile;
                    OriginalLoaded = imageModel.OriginalLoaded;
                    Progress = 100;
                    State = ImageLoadingState.Loaded;
                }
                this.Init(hash.ToToken(), null);
            });
        }

        internal GalleryImage(Gallery owner, int pageID)
        {
            this.Owner = owner;
            this.PageID = pageID;
        }

        internal void Init(ulong imageKey, Uri thumb)
        {
            this.ImageKey = imageKey;
            this.thumbUri = thumb;
            OnPropertyChanged(nameof(Thumb));
        }

        private static readonly Regex failTokenMatcher = new Regex(@"return\s+nl\(\s*'(.+?)'\s*\)", RegexOptions.Compiled);
        private static readonly Regex hashMatcher = new Regex(@"f_shash=([A-Fa-f0-9]{40})(&|\s|$)", RegexOptions.Compiled);

        private IAsyncAction loadImageUriAndHash()
        {
            return Run(async token =>
            {
                var loadPageUri = this.failToken is null
                    ? this.PageUri
                    : new Uri(this.PageUri, $"?{this.failToken}");

                var doc = await Client.Current.HttpClient.GetDocumentAsync(loadPageUri);

                this.imageUri = doc.GetElementbyId("img").GetAttribute("src", default(Uri));
                this.originalImageUri = doc.GetElementbyId("i7").Element("a")?.GetAttribute("href", default(Uri));
                var hashNode = doc.GetElementbyId("i6").Element("a");
                this.ImageHash = SHA1Value.Parse(hashMatcher.Match(hashNode.GetAttribute("href", "")).Groups[1].Value);
                var loadFail = doc.GetElementbyId("loadfail").GetAttribute("onclick", "");
                var oft = this.failToken;
                var nft = failTokenMatcher.Match(loadFail).Groups[1].Value;
                if (oft == null)
                    this.failToken = "nl=" + nft;
                else
                    this.failToken = oft + "&nl=" + nft;
            });
        }

        private string failToken;

        private ImageLoadingState state;
        public ImageLoadingState State
        {
            get => this.state;
            protected set => Set(ref this.state, value);
        }

        private Uri thumbUri;

        protected static HttpClient ThumbClient { get; } = new HttpClient();

        private void loadThumb()
        {
            CoreApplication.MainView.Dispatcher.BeginIdle(async d =>
            {
                var img = new BitmapImage();
                try
                {
                    if (this.imageFile != null)
                    {
                        using (var stream = await this.imageFile.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.SingleItem, (uint)(180 * display.RawPixelsPerViewPixel), Windows.Storage.FileProperties.ThumbnailOptions.ResizeThumbnail))
                        {
                            await img.SetSourceAsync(stream);
                        }
                    }
                    else if (this.thumbUri != null)
                    {
                        var buffer = await ThumbClient.GetBufferAsync(this.thumbUri);
                        using (var stream = buffer.AsRandomAccessStream())
                        {
                            await img.SetSourceAsync(stream);
                        }
                    }
                    else
                    {
                        img = null;
                    }
                }
                catch (Exception)
                {
                    img = null;
                }
                this.thumb.SetTarget(img);
                if (img != null)
                    OnPropertyChanged(nameof(Thumb));
            });
        }

        private readonly WeakReference<ImageSource> thumb = new WeakReference<ImageSource>(null);
        public virtual ImageSource Thumb
        {
            get
            {
                if (this.thumb.TryGetTarget(out var thb))
                    return thb;
                loadThumb();
                return DefaultThumb;
            }
        }

        public Gallery Owner { get; }

        /// <summary>
        /// 1-based ID for image.
        /// </summary>
        public int PageID { get; }

        public Uri PageUri
            => this.imageKey == 0 ? null : new Uri(Client.Current.Uris.RootUri, $"s/{this.imageKey.ToTokenString()}/{Owner.ID}-{PageID}");

        private ulong imageKey;
        public ulong ImageKey { get => this.imageKey; private set => Set(nameof(PageUri), ref this.imageKey, value); }

        private SHA1Value imageHash;
        /// <summary>
        /// SHA-1 value for original image file.
        /// </summary>
        public SHA1Value ImageHash
        {
            get => this.imageHash;
            protected set => Set(ref this.imageHash, value);
        }

        private IAsyncAction loadImageAction;

        public virtual IAsyncAction LoadImageAsync(bool reload, ConnectionStrategy strategy, bool throwIfFailed)
        {
            var previousAction = this.loadImageAction;
            var previousEnded = previousAction == null || previousAction.Status != AsyncStatus.Started;
            switch (this.state)
            {
            case ImageLoadingState.Loading:
            case ImageLoadingState.Loaded:
                if (!reload)
                {
                    if (previousEnded)
                        return AsyncAction.CreateCompleted();
                    return PollingAsyncWrapper.Wrap(previousAction, 1500);
                }
                else
                {
                    if (!previousEnded)
                        previousAction?.Cancel();
                }
                break;
            case ImageLoadingState.Preparing:
                if (previousEnded)
                    return AsyncAction.CreateCompleted();
                return PollingAsyncWrapper.Wrap(previousAction, 1500);
            }
            return this.loadImageAction = startLoadImageAsync(reload, strategy, throwIfFailed);
        }

        private IAsyncAction startLoadImageAsync(bool reload, ConnectionStrategy strategy, bool throwIfFailed)
        {
            this.State = ImageLoadingState.Preparing;
            return Run(async token =>
            {
                try
                {
                    if (this.PageUri == null)
                        await Owner.LoadItemsAsync(this.PageID - 1, 1);
                    var loadFull = !ConnectionHelper.IsLofiRequired(strategy);
                    var folder = ImageFolder ?? await GetImageFolderAsync();
                    this.Progress = 0;

                    var loadImgInfo = this.loadImageUriAndHash();
                    token.Register(loadImgInfo.Cancel);
                    await loadImgInfo;
                    token.ThrowIfCancellationRequested();

                    using (var db = new GalleryDb())
                    {
                        var imageModel = db.ImageSet.SingleOrDefault(ImageModel.PKEquals(this.imageHash));
                        while (!reload && imageModel != null && (imageModel.OriginalLoaded || imageModel.OriginalLoaded == loadFull))
                        {
                            // Try load local file
                            var file = await folder.TryGetFileAsync(imageModel.FileName);
                            if (file == null)
                            {
                                // Failed
                                break;
                            }
                            this.ImageFile = file;
                            this.OriginalLoaded = imageModel.OriginalLoaded;

                            var giModel = db.GalleryImageSet
                                .SingleOrDefault(model => model.GalleryId == this.Owner.ID && model.PageId == this.PageID);
                            if (giModel == null)
                                db.GalleryImageSet.Add(new GalleryImageModel().Update(this));
                            else
                                giModel.Update(this);
                            db.SaveChanges();
                            this.Progress = 100;
                            this.State = ImageLoadingState.Loaded;
                            return;
                        }
                        token.ThrowIfCancellationRequested();
                        if (this.imageUri.LocalPath.EndsWith("/509.gif"))
                            throw new InvalidOperationException(LocalizedStrings.Resources.ExceedLimits);

                        var imgUri = default(Uri);
                        if (loadFull)
                        {
                            imgUri = this.originalImageUri ?? this.imageUri;
                            this.OriginalLoaded = true;
                        }
                        else
                        {
                            imgUri = this.imageUri;
                            this.OriginalLoaded = (this.originalImageUri == null);
                        }
                        this.State = ImageLoadingState.Loading;

                        var loadImg = Client.Current.HttpClient.GetAsync(imgUri);
                        loadImg.Progress = (httpAsyncInfo, httpProgress) =>
                        {
                            if (token.IsCancellationRequested)
                            {
                                httpAsyncInfo.Cancel();
                                return;
                            }
                            if (httpProgress.TotalBytesToReceive == null || httpProgress.TotalBytesToReceive == 0)
                                this.Progress = 0;
                            else
                            {
                                var pro = (int)(httpProgress.BytesReceived * 100 / ((ulong)httpProgress.TotalBytesToReceive));
                                this.Progress = pro;
                            }
                        };
                        var imageLoadResponse = await loadImg;
                        token.ThrowIfCancellationRequested();

                        if (imageLoadResponse.Content.Headers.ContentType.MediaType == "text/html")
                        {
                            var error = HtmlUtilities.ConvertToText(imageLoadResponse.Content.ToString());
                            if (error.StartsWith("You have exceeded your image viewing limits."))
                                throw new InvalidOperationException(LocalizedStrings.Resources.ExceedLimits);
                            else
                                throw new InvalidOperationException(error);
                        }

                        await this.deleteImageFileAsync();
                        var buffer = await imageLoadResponse.Content.ReadAsBufferAsync();
                        var ext = Path.GetExtension(imageLoadResponse.RequestMessage.RequestUri.LocalPath);
                        this.ImageFile = await folder.SaveFileAsync($"{this.imageHash}{ext}", CreationCollisionOption.ReplaceExisting, buffer);
                        var myModel = db.GalleryImageSet
                            .Include(model => model.Image)
                            .SingleOrDefault(model => model.GalleryId == this.Owner.ID && model.PageId == this.PageID);
                        if (myModel == null)
                        {
                            if (imageModel == null)
                                db.ImageSet.Add(new ImageModel().Update(this));
                            else
                                imageModel.Update(this);
                            db.GalleryImageSet.Add(new GalleryImageModel().Update(this));
                        }
                        else
                        {
                            myModel.Image.Update(this);
                            myModel.Update(this);
                        }
                        db.SaveChanges();
                        this.State = ImageLoadingState.Loaded;
                    }
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception)
                {
                    this.Progress = 100;
                    this.State = ImageLoadingState.Failed;
                    if (throwIfFailed)
                        throw;
                }
            });
        }

        private async Task deleteImageFileAsync()
        {
            var file = this.ImageFile;
            if (file != null)
            {
                this.ImageFile = null;
                try
                {
                    await file.DeleteAsync();
                }
                catch (FileNotFoundException)
                {
                }
            }
        }

        private int progress;

        public int Progress
        {
            get => this.progress;
            private set => Set(ref this.progress, value);
        }

        private Uri imageUri;
        private Uri originalImageUri;

        private StorageFile imageFile;
        public StorageFile ImageFile
        {
            get => this.imageFile;
            protected set
            {
                Set(ref this.imageFile, value);
                if (value != null)
                {
                    loadThumb();
                }
            }
        }

        private bool originalLoaded;
        public bool OriginalLoaded
        {
            get => this.originalLoaded;
            private set => Set(ref this.originalLoaded, value);
        }
    }
}
