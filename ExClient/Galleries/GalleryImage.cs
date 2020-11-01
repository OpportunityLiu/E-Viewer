using ExClient.Api;
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
    [System.Diagnostics.DebuggerDisplay(@"\{PageId = {PageId} State = {State} File = {ImageFile?.Name}\}")]
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

        internal GalleryImage(Gallery owner, int pageId)
        {
            Owner = owner;
            PageId = pageId;
        }

        internal void Init(EToken imageKey, Uri thumb)
        {
            ImageKey = imageKey;
            ThumbUri = thumb;
        }

        private static readonly Regex _FailTokenMatcher = new Regex(@"nl\(\s*['""](.+?)['""]\s*\)", RegexOptions.Compiled);
        private static readonly Regex _HashMatcher = new Regex(@"f_shash=([A-Fa-f0-9]{40})(&|\s|$)", RegexOptions.Compiled);
        private static readonly Regex _ShowKeyMatcher = new Regex(@"var\s+showkey\s*=\s*['""]([a-zA-Z0-9]+?)['""]", RegexOptions.Compiled);

        private Task _LoadImageUriAndHash(CancellationToken token)
        {
            if (_FailToken is null && Owner.ShowKey is string showKey)
                return loadFromApi();
            else
                return loadFromHtml();

            async Task loadFromApi()
            {
                try
                {
                    var req = new ImageDataRequest(showKey, this);
                    var res = await req.GetResponseAsync(token);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(res.i3);
                    analyzeI3Node(doc.DocumentNode);
                    doc.LoadHtml(res.i6);
                    analyzeI6Node(doc.DocumentNode);
                    doc.LoadHtml(res.i7);
                    analyzeI7Node(doc.DocumentNode);
                }
                catch (ArgumentException)
                {
                    await loadFromHtml();
                }
            }

            async Task loadFromHtml()
            {
                var loadPageUri = _FailToken is null
                    ? PageUri
                    : new Uri(PageUri, $"?{_FailToken}");

                var doc = await Client.Current.HttpClient.GetDocumentAsync(loadPageUri).AsTask(token);
                analyzeI3Node(doc.GetElementbyId("i3"));
                analyzeI6Node(doc.GetElementbyId("i6"));
                analyzeI7Node(doc.GetElementbyId("i7"));
                Owner.ShowKey = doc.DocumentNode.Descendants("script").Select(n =>
                {
                    var match = _ShowKeyMatcher.Match(n.GetInnerText());
                    if (match.Success)
                        return match.Groups[1].Value;
                    return null;
                }).First(s => s != null);
            }

            void analyzeI3Node(HtmlNode i3)
            {
                var img = i3.Element("a").Element("img");
                imageUri = img.GetAttribute("src", default(Uri));

                var loadFail = img.GetAttribute("onerror", "");
                var oft = _FailToken;
                var nft = _FailTokenMatcher.Match(loadFail).Groups[1].Value;
                if (oft is null)
                    _FailToken = "nl=" + nft;
                else
                    _FailToken = oft + "&nl=" + nft;
            }
            void analyzeI6Node(HtmlNode i6)
            {
                var hashNode = i6.Element("a");
                ImageHash = SHA1Value.Parse(_HashMatcher.Match(hashNode.GetAttribute("href", "")).Groups[1].Value);
            }
            void analyzeI7Node(HtmlNode i7)
            {
                var origNode = i7.Element("a");
                originalImageUri = origNode?.GetAttribute("href", default(Uri));
            }
        }

        private string _FailToken;

        private ImageLoadingState _State;
        public ImageLoadingState State
        {
            get => _State;
            private set => Set(ref _State, value);
        }

        private Uri _ThumbUri;
        public Uri ThumbUri { get => _ThumbUri; private set => ForceSet(nameof(Thumb), ref _ThumbUri, value); }

        private readonly WeakReference<BitmapImage> _Thumb = new WeakReference<BitmapImage>(null);
        private async void _LoadThumb()
        {
            var file = imageFile;
            var uri = _ThumbUri;
            if (file is null && uri is null)
                return;

            _Thumb.TryGetTarget(out var current);

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
                uri = _ThumbUri;
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
                _Thumb.SetTarget(img);
                OnPropertyChanged(nameof(Thumb));
            }
        }

        public BitmapImage Thumb
        {
            get
            {
                if (_Thumb.TryGetTarget(out var thb))
                    return thb;
                _LoadThumb();
                return ThumbHelper.DefaultThumb;
            }
        }

        public Gallery Owner { get; }

        /// <summary>
        /// 1-based ID for image.
        /// </summary>
        public int PageId { get; }

        public Uri PageUri
            => _ImageKey == default ? null : new Uri(Client.Current.Uris.RootUri, $"s/{_ImageKey.ToString()}/{Owner.Id}-{PageId}");

        private EToken _ImageKey;
        public EToken ImageKey { get => _ImageKey; private set => Set(nameof(PageUri), ref _ImageKey, value); }

        private SHA1Value _ImageHash;
        /// <summary>
        /// SHA-1 value for original image file.
        /// </summary>
        public SHA1Value ImageHash
        {
            get => _ImageHash;
            private set => Set(ref _ImageHash, value);
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
            switch (_State)
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
                        await Owner.LoadItemsAsync(PageId - 1, 1);
                    var loadFull = !ConnectionHelper.IsLofiRequired(strategy);
                    Progress = 0;

                    await _LoadImageUriAndHash(token);

                    using (var db = new GalleryDb())
                    {
                        var imageModel = db.ImageSet.SingleOrDefault(ImageModel.PKEquals(_ImageHash));
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
                                .SingleOrDefault(model => model.GalleryId == Owner.Id && model.PageId == PageId);
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

                        if (imageLoadResponse.Content.Headers.ContentType?.MediaType == "text/html")
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
                        ImageFile = await StorageHelper.ImageFolder.SaveFileAsync($"{_ImageHash}{ext}", CreationCollisionOption.ReplaceExisting, buffer);
                        var myModel = db.GalleryImageSet
                            .Include(model => model.Image)
                            .SingleOrDefault(model => model.GalleryId == Owner.Id && model.PageId == PageId);
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
                if (value != null && _Thumb.TryGetTarget(out _))
                {
                    _LoadThumb();
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
