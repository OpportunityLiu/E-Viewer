using ExClient.Internal;
using ExClient.Models;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Opportunity.MvvmUniverse;
using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
            DispatcherHelper.BeginInvokeOnUIThread(() =>
            {
                display = DisplayInformation.GetForCurrentView();
                DefaultThumb = new BitmapImage();
                DefaultThumb.Dispatcher.BeginIdle(async a =>
                {
                    using (var stream = await StorageHelper.GetIconOfExtension("jpg"))
                    {
                        await DefaultThumb.SetSourceAsync(stream);
                    }
                });
            });
        }

        private static DisplayInformation display;

        protected internal static StorageFolder ImageFolder { get; private set; }

        protected internal static IAsyncOperation<StorageFolder> GetImageFolderAsync()
        {
            var temp = ImageFolder;
            if (temp != null)
                return AsyncWrapper.CreateCompleted(temp);
            return Run(async token =>
            {
                var temp2 = ImageFolder;
                if (temp2 == null)
                {
                    temp2 = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);
                    ImageFolder = temp2;
                }
                return temp2;
            });
        }

        protected static BitmapImage DefaultThumb { get; private set; }

        internal static IAsyncOperation<GalleryImage> LoadCachedImageAsync(Gallery owner, GalleryImageModel galleryImageModel, ImageModel imageModel)
        {
            return Run(async token =>
            {
                var folder = ImageFolder ?? await GetImageFolderAsync();
                var hash = galleryImageModel.ImageId;
                var img = new GalleryImage(owner, galleryImageModel.PageId, hash.ToToken(), null)
                {
                    imageHash = hash
                };
                var imageFile = await folder.TryGetFileAsync(imageModel.FileName);
                if (imageFile == null)
                {
                    img.originalLoaded = false;
                    img.state = ImageLoadingState.Waiting;
                }
                else
                {
                    img.imageFile = imageFile;
                    img.originalLoaded = imageModel.OriginalLoaded;
                    img.progress = 100;
                    img.state = ImageLoadingState.Loaded;
                }
                return img;
            });
        }

        internal GalleryImage(Gallery owner, int pageID, ulong imageKey, Uri thumb)
        {
            this.Owner = owner;
            this.PageID = pageID;
            this.ImageKey = imageKey;
            this.thumbUri = thumb;
        }

        private static readonly Regex failTokenMatcher = new Regex(@"return\s+nl\(\s*'(.+?)'\s*\)", RegexOptions.Compiled);
        private static readonly Regex hashMatcher = new Regex(@"f_shash=([A-Fa-f0-9]{40})(&|\s|$)", RegexOptions.Compiled);

        private IAsyncAction loadImageUriAndHash()
        {
            return Run(async token =>
            {
                var loadPageUri = default(Uri);
                if (this.failToken != null)
                    loadPageUri = new Uri(this.PageUri, $"?{this.failToken}");
                else
                    loadPageUri = this.PageUri;
                var doc = await Client.Current.HttpClient.GetDocumentAsync(loadPageUri);

                this.imageUri = new Uri(doc.GetElementbyId("img").GetAttributeValue("src", "").DeEntitize());
                var originalNode = doc.GetElementbyId("i7").Element("a");
                if (originalNode == null)
                {
                    this.originalImageUri = null;
                }
                else
                {
                    this.originalImageUri = new Uri(originalNode.GetAttributeValue("href", "").DeEntitize());
                }
                var hashNode = doc.GetElementbyId("i6").Element("a");
                this.ImageHash = SHA1Value.Parse(hashMatcher.Match(hashNode.GetAttributeValue("href", "").DeEntitize()).Groups[1].Value);
                var loadFail = doc.GetElementbyId("loadfail").GetAttributeValue("onclick", "").DeEntitize();
                var oft = this.failToken;
                var nft = failTokenMatcher.Match(loadFail).Groups[1].Value;
                if (oft == null)
                    this.failToken = $"nl={nft}";
                else
                    this.failToken = $"{oft}&nl={nft}";
            });
        }

        private string failToken;

        private ImageLoadingState state;
        public ImageLoadingState State
        {
            get => state;
            protected set => Set(ref state, value);
        }

        private Uri thumbUri;

        private readonly WeakReference<ImageSource> thumb = new WeakReference<ImageSource>(null);

        protected static HttpClient ThumbClient { get; } = new HttpClient();

        private void loadThumb()
        {
            DispatcherHelper.BeginInvokeOnUIThread(async () =>
            {
                var img = new BitmapImage();
                await img.Dispatcher.YieldIdle();
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

        public Uri PageUri => new Uri(Client.Current.Uris.RootUri, $"s/{ImageKey.ToTokenString()}/{Owner.ID}-{PageID}");

        public ulong ImageKey { get; }

        private SHA1Value imageHash;
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
                        return AsyncWrapper.CreateCompleted();
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
                    return AsyncWrapper.CreateCompleted();
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
                    var loadFull = !ConnectionHelper.IsLofiRequired(strategy);
                    var folder = ImageFolder ?? await GetImageFolderAsync();
                    using (var db = new GalleryDb())
                    {
                        this.Progress = 0;
                        var loadImgInfo = this.loadImageUriAndHash();
                        IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> loadImg = null;
                        token.Register(() =>
                        {
                            loadImgInfo.Cancel();
                            loadImg?.Cancel();
                        });
                        await loadImgInfo;
                        var imageModel = db.ImageSet.SingleOrDefault(ImageModel.PKEquals(this.imageHash));
                        if (!reload && imageModel != null && (imageModel.OriginalLoaded || imageModel.OriginalLoaded == loadFull))
                        {
                            var file = await folder.TryGetFileAsync(imageModel.FileName);
                            if (file != null)
                            {
                                this.ImageFile = file;
                            }
                            var giModel = db.GalleryImageSet
                                .SingleOrDefault(model => model.GalleryId == this.Owner.ID && model.PageId == this.PageID);
                            if (giModel == null)
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
                        Uri imgUri = null;
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
                        token.ThrowIfCancellationRequested();
                        loadImg = Client.Current.HttpClient.GetAsync(imgUri);
                        loadImg.Progress = loadImgProgress;
                        var imageLoadResponse = await loadImg;
                        if (imageLoadResponse.Content.Headers.ContentType.MediaType == "text/html")
                        {
                            var error = HtmlUtilities.ConvertToText(imageLoadResponse.Content.ToString());
                            if (error.StartsWith("You have exceeded your image viewing limits."))
                            {
                                throw new InvalidOperationException(LocalizedStrings.Resources.ExceedLimits);
                            }
                            throw new InvalidOperationException(error);
                        }
                        token.ThrowIfCancellationRequested();
                        await this.deleteImageFileAsync();
                        var buffer = await imageLoadResponse.Content.ReadAsBufferAsync();
                        var ext = Path.GetExtension(imageLoadResponse.RequestMessage.RequestUri.LocalPath);
                        this.ImageFile = await folder.SaveFileAsync($"{this.imageHash}{ext}", CreationCollisionOption.ReplaceExisting, buffer);
                        var myModel = db.GalleryImageSet
                            .Include(model => model.Image)
                            .SingleOrDefault(model => model.GalleryId == this.Owner.ID && model.PageId == this.PageID);
                        imageModel?.Update(this);
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

        private void loadImgProgress(IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> asyncInfo, HttpProgress progress)
        {
            if (progress.TotalBytesToReceive == null || progress.TotalBytesToReceive == 0)
                this.Progress = 0;
            else
            {
                var pro = (int)(progress.BytesReceived * 100 / ((ulong)progress.TotalBytesToReceive));
                this.Progress = pro;
            }
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
            get => progress;
            private set => Set(ref progress, value);
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
