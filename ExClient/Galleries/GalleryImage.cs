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
using Windows.Storage.FileProperties;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Galleries
{
    [System.Diagnostics.DebuggerDisplay(@"\{PageID = {PageID} State = {State} File = {ImageFile?.Name}\}")]
    public sealed class GalleryImage : ObservableObject
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
                {
                    return;
                }

                await b.Dispatcher.YieldIdle();
                using (var stream = await StorageHelper.GetIconOfExtensionAsync("jpg"))
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

        public static StorageFolder ImageFolder { get; set; }

        internal static IAsyncOperation<StorageFolder> GetImageFolderAsync()
        {
            var temp = ImageFolder;
            if (temp != null)
                return AsyncOperation<StorageFolder>.CreateCompleted(temp);
            return Run(async token =>
            {
                var temp2 = ImageFolder;
                if (temp2 is null)
                {
                    temp2 = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);
                    ImageFolder = temp2;
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
            this.ThumbUri = thumb;
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
                if (oft is null)
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
            private set => Set(ref this.state, value);
        }

        private Uri thumbUri;
        public Uri ThumbUri { get => this.thumbUri; private set => ForceSet(nameof(Thumb), ref this.thumbUri, value); }

        private readonly WeakReference<ImageSource> thumb = new WeakReference<ImageSource>(null);
        private async void loadThumb()
        {
            var file = this.imageFile;
            var uri = this.thumbUri;
            if (file is null && uri is null)
                return;

            this.thumb.TryGetTarget(out var current);
            if (!CoreApplication.MainView.Dispatcher.HasThreadAccess)
                await CoreApplication.MainView.Dispatcher.YieldIdle();
            var img = new BitmapImage();
            try
            {
                if (file != null)
                {
                    var size = (uint)(180 * display.RawPixelsPerViewPixel);
                    using (var stream = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, size, ThumbnailOptions.ResizeThumbnail))
                    {
                        await img.SetSourceAsync(stream);
                    }
                }
                else if (!await ThumbClient.FetchThumbAsync(uri, img))
                    img = null;
            }
            catch
            {
                img = null;
            }
            this.thumb.SetTarget(img);
            if (img != current)
                OnPropertyChanged(nameof(Thumb));
        }

        public ImageSource Thumb
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
            private set => Set(ref this.imageHash, value);
        }

        private IAsyncAction loadImageAction;

        public IAsyncAction LoadImageAsync(bool reload, ConnectionStrategy strategy, bool throwIfFailed)
        {
            var previousAction = this.loadImageAction;
            var previousEnded = previousAction is null || previousAction.Status != AsyncStatus.Started;
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
                    if (this.PageUri is null)
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
                            if (file is null)
                            {
                                // Failed
                                break;
                            }
                            this.ImageFile = file;
                            this.OriginalLoaded = imageModel.OriginalLoaded;

                            var giModel = db.GalleryImageSet
                                .SingleOrDefault(model => model.GalleryId == this.Owner.ID && model.PageId == this.PageID);
                            if (giModel is null)
                            {
                                db.GalleryImageSet.Add(new GalleryImageModel().Update(this));
                            }
                            else
                            {
                                giModel.Update(this);
                            }

                            db.SaveChanges();
                            this.Progress = 100;
                            this.State = ImageLoadingState.Loaded;
                            return;
                        }

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
                            this.OriginalLoaded = (this.originalImageUri is null);
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
                            if (httpProgress.TotalBytesToReceive is null || httpProgress.TotalBytesToReceive == 0)
                            {
                                this.Progress = 0;
                            }
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
                            {
                                throw new InvalidOperationException(LocalizedStrings.Resources.ExceedLimits);
                            }
                            else
                            {
                                throw new InvalidOperationException(error);
                            }
                        }

                        await this.deleteImageFileAsync();
                        var buffer = await imageLoadResponse.Content.ReadAsBufferAsync();
                        var ext = Path.GetExtension(imageLoadResponse.RequestMessage.RequestUri.LocalPath);
                        this.ImageFile = await folder.SaveFileAsync($"{this.imageHash}{ext}", CreationCollisionOption.ReplaceExisting, buffer);
                        var myModel = db.GalleryImageSet
                            .Include(model => model.Image)
                            .SingleOrDefault(model => model.GalleryId == this.Owner.ID && model.PageId == this.PageID);
                        if (myModel is null)
                        {
                            if (imageModel is null)
                            {
                                db.ImageSet.Add(new ImageModel().Update(this));
                            }
                            else
                            {
                                imageModel.Update(this);
                            }

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
            private set
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
