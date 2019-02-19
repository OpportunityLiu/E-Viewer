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
        internal async Task PopulateCachedImageAsync(GalleryImageModel galleryImageModel, ImageModel imageModel)
        {
            var hash = galleryImageModel.ImageId;
            ImageHash = hash;
            var imageFile = await StorageHelper.ImageFolder.TryGetFileAsync(imageModel.FileName);
            if (imageFile != null)
            {
                ImageFile = imageFile;
                OriginalLoaded = imageModel.OriginalLoaded;
                Progress = 100;
                State = ImageLoadingState.Loaded;
            }
            Init(hash.ToToken(), null);
        }

        internal GalleryImage(Gallery owner, int pageID)
        {
            Owner = owner;
            PageID = pageID;
        }

        internal void Init(ulong imageKey, Uri thumb)
        {
            ImageKey = imageKey;
            ThumbUri = thumb;
        }

        private static readonly Regex failTokenMatcher = new Regex(@"return\s+nl\(\s*'(.+?)'\s*\)", RegexOptions.Compiled);
        private static readonly Regex hashMatcher = new Regex(@"f_shash=([A-Fa-f0-9]{40})(&|\s|$)", RegexOptions.Compiled);

        private async Task loadImageUriAndHash(CancellationToken token)
        {
            var loadPageUri = failToken is null
                ? PageUri
                : new Uri(PageUri, $"?{failToken}");

            var doc = await Client.Current.HttpClient.GetDocumentAsync(loadPageUri).AsTask(token);

            imageUri = doc.GetElementbyId("img").GetAttribute("src", default(Uri));
            originalImageUri = doc.GetElementbyId("i7").Element("a")?.GetAttribute("href", default(Uri));
            var hashNode = doc.GetElementbyId("i6").Element("a");
            ImageHash = SHA1Value.Parse(hashMatcher.Match(hashNode.GetAttribute("href", "")).Groups[1].Value);
            var loadFail = doc.GetElementbyId("loadfail").GetAttribute("onclick", "");
            var oft = failToken;
            var nft = failTokenMatcher.Match(loadFail).Groups[1].Value;
            if (oft is null)
                failToken = "nl=" + nft;
            else
                failToken = oft + "&nl=" + nft;
        }

        private string failToken;

        private ImageLoadingState state;
        public ImageLoadingState State
        {
            get => state;
            private set => Set(ref state, value);
        }

        private Uri thumbUri;
        public Uri ThumbUri { get => thumbUri; private set => ForceSet(nameof(Thumb), ref thumbUri, value); }

        private readonly WeakReference<BitmapImage> thumb = new WeakReference<BitmapImage>(null);
        private async void loadThumb()
        {
            var file = imageFile;
            var uri = thumbUri;
            if (file is null && uri is null)
                return;

            thumb.TryGetTarget(out var current);

            var img = current;
            if (img is null)
            {
                if (!CoreApplication.MainView.Dispatcher.HasThreadAccess)
                    await CoreApplication.MainView.Dispatcher.Yield();
                img = new BitmapImage();
            }
            try
            {
                file = imageFile;
                uri = thumbUri;
                if (file != null)
                {
                    var size = (uint)(180 * ThumbHelper.Display.RawPixelsPerViewPixel);
                    using (var stream = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, size, ThumbnailOptions.ResizeThumbnail))
                    {
                        await img.SetSourceAsync(stream);
                    }
                }
                else if (!await ThumbClient.FetchThumbAsync(uri, img))
                    return;
            }
            catch
            {
                return;
            }
            if (img != current)
            {
                thumb.SetTarget(img);
                OnPropertyChanged(nameof(Thumb));
            }
        }

        public BitmapImage Thumb
        {
            get
            {
                if (thumb.TryGetTarget(out var thb))
                    return thb;
                loadThumb();
                return ThumbHelper.DefaultThumb;
            }
        }

        public Gallery Owner { get; }

        /// <summary>
        /// 1-based ID for image.
        /// </summary>
        public int PageID { get; }

        public Uri PageUri
            => imageKey == 0 ? null : new Uri(Client.Current.Uris.RootUri, $"s/{imageKey.ToTokenString()}/{Owner.ID}-{PageID}");

        private ulong imageKey;
        public ulong ImageKey { get => imageKey; private set => Set(nameof(PageUri), ref imageKey, value); }

        private SHA1Value imageHash;
        /// <summary>
        /// SHA-1 value for original image file.
        /// </summary>
        public SHA1Value ImageHash
        {
            get => imageHash;
            private set => Set(ref imageHash, value);
        }

        private IAsyncAction loadImageAction;

        public IAsyncAction LoadImageAsync(bool reload, ConnectionStrategy strategy, bool throwIfFailed)
        {
            var previousAction = loadImageAction;
            var previousEnded = previousAction is null || previousAction.Status != AsyncStatus.Started;
            if (reload)
            {
                if (!previousEnded)
                    previousAction.Cancel();
                return loadImageAction = startLoadImageAsync(reload, strategy, throwIfFailed);
            }
            switch (state)
            {
            case ImageLoadingState.Loaded:
                if (!previousEnded)
                    previousAction.Cancel();
                return AsyncAction.CreateCompleted();
            case ImageLoadingState.Failed:
                if (!previousEnded)
                    previousAction.Cancel();
                return loadImageAction = startLoadImageAsync(reload, strategy, throwIfFailed);
            default:
                if (!previousEnded)
                    return PollingAsyncWrapper.Wrap(previousAction, 1500);
                return loadImageAction = startLoadImageAsync(reload, strategy, throwIfFailed);
            }
        }

        private IAsyncAction startLoadImageAsync(bool reload, ConnectionStrategy strategy, bool throwIfFailed)
        {
            State = ImageLoadingState.Preparing;
            return Run(async token =>
            {
                try
                {
                    if (PageUri is null)
                        await Owner.LoadItemsAsync(PageID - 1, 1);
                    var loadFull = !ConnectionHelper.IsLofiRequired(strategy);
                    Progress = 0;

                    await loadImageUriAndHash(token);

                    using (var db = new GalleryDb())
                    {
                        var imageModel = db.ImageSet.SingleOrDefault(ImageModel.PKEquals(imageHash));
                        while (!reload && imageModel != null && (imageModel.OriginalLoaded || imageModel.OriginalLoaded == loadFull))
                        {
                            // Try load local file
                            var file = await StorageHelper.ImageFolder.TryGetFileAsync(imageModel.FileName);
                            if (file is null)
                            {
                                // Failed
                                break;
                            }
                            ImageFile = file;
                            OriginalLoaded = imageModel.OriginalLoaded;

                            var giModel = db.GalleryImageSet
                                .SingleOrDefault(model => model.GalleryId == Owner.ID && model.PageId == PageID);
                            if (giModel is null)
                            {
                                db.GalleryImageSet.Add(new GalleryImageModel().Update(this));
                            }
                            else
                            {
                                giModel.Update(this);
                            }

                            db.SaveChanges();
                            Progress = 100;
                            State = ImageLoadingState.Loaded;
                            return;
                        }

                        if (imageUri.LocalPath.EndsWith("/509.gif"))
                            throw new InvalidOperationException(LocalizedStrings.Resources.ExceedLimits);

                        token.ThrowIfCancellationRequested();
                        var imgUri = default(Uri);
                        if (loadFull)
                        {
                            imgUri = originalImageUri ?? imageUri;
                            OriginalLoaded = true;
                        }
                        else
                        {
                            imgUri = imageUri;
                            OriginalLoaded = (originalImageUri is null);
                        }
                        State = ImageLoadingState.Loading;

                        var imageLoadResponse = await Client.Current.HttpClient.GetAsync(imgUri).AsTask(token, new Progress<HttpProgress>(p =>
                        {
                            if (p.TotalBytesToReceive is null || p.TotalBytesToReceive == 0)
                            {
                                Progress = 0;
                            }
                            else
                            {
                                var pro = (int)(p.BytesReceived * 100 / ((ulong)p.TotalBytesToReceive));
                                Progress = pro;
                            }
                        }));

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
                        var buffer = await imageLoadResponse.Content.ReadAsBufferAsync().AsTask(token);

                        token.ThrowIfCancellationRequested();
                        await deleteImageFileAsync();
                        var ext = Path.GetExtension(imageLoadResponse.RequestMessage.RequestUri.LocalPath);
                        ImageFile = await StorageHelper.ImageFolder.SaveFileAsync($"{imageHash}{ext}", CreationCollisionOption.ReplaceExisting, buffer);
                        var myModel = db.GalleryImageSet
                            .Include(model => model.Image)
                            .SingleOrDefault(model => model.GalleryId == Owner.ID && model.PageId == PageID);
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
                        Progress = 100;
                        State = ImageLoadingState.Loaded;
                    }
                }
                catch (OperationCanceledException)
                {
                    Progress = 0;
                    State = ImageLoadingState.Waiting;
                    throw;
                }
                catch (Exception)
                {
                    Progress = 100;
                    State = ImageLoadingState.Failed;
                    if (throwIfFailed)
                        throw;
                }
            });
        }

        private async Task deleteImageFileAsync()
        {
            var file = ImageFile;
            if (file != null)
            {
                ImageFile = null;
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
            get => imageFile;
            private set
            {
                Set(ref imageFile, value);
                if (value != null && thumb.TryGetTarget(out _))
                {
                    loadThumb();
                }
            }
        }

        private bool originalLoaded;
        public bool OriginalLoaded
        {
            get => originalLoaded;
            private set => Set(ref originalLoaded, value);
        }
    }
}
