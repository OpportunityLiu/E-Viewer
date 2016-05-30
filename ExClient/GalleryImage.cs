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
        private IAsyncAction loadImageUri()
        {
            return Run(async token =>
            {
                var loadPageUri = PageUri;
                if(failToken != null)
                    loadPageUri = new Uri(pageBaseUri, $"{imageKey}/{Owner.Id.ToString()}-{PageId.ToString()}?nl={failToken}");
                var loadPage = Owner.Owner.PostStrAsync(loadPageUri, null);
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
                failToken = Regex.Match(loadFail, @"return\s+nl\(\s*'(.+?)'\s*\)").Groups[1].Value;
            });
        }

        internal GalleryImage(Gallery owner, int pageId, string imageKey, ImageSource thumb)
        {
            this.Owner = owner;
            this.PageId = pageId;
            this.imageKey = imageKey;
            this.PageUri = new Uri(pageBaseUri, $"{imageKey}/{owner.Id.ToString()}-{pageId.ToString()}");
            this.Thumb = thumb;
        }

        internal static IAsyncOperation<GalleryImage> LoadCachedImageAsync(Gallery owner, Models.ImageModel model)
        {
            return Task.Run(async () =>
            {
                var imageFile = await owner.GalleryFolder.TryGetFileAsync(model.FileName);
                if(imageFile == null)
                    return null;
                GalleryImage image = null;
                await DispatcherHelper.RunAsync(() =>
                {
                    var thumb = new BitmapImage();
                    image = new GalleryImage(owner, model.PageId, model.ImageKey, thumb);

                    var loadThumb = imageFile.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.SingleItem);
                    loadThumb.Completed = async (sender, e) =>
                    {
                        if(e != AsyncStatus.Completed)
                            return;
                        await DispatcherHelper.RunAsync(async () =>
                        {
                            using(var stream = sender.GetResults())
                                await thumb.SetSourceAsync(stream);
                        });
                    };

                    image.ImageFile = imageFile;
                    image.OriginalLoaded = model.OriginalLoaded;
                    image.Progress = 100;
                    image.State = ImageLoadingState.Loaded;
                });
                return image;
            }).AsAsyncOperation();
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
                    if(!reload && state == ImageLoadingState.Loaded)
                    {
                        //check whether image file is deleted.
                        using(var db = Models.CachedGalleryDb.Create())
                        {
                            if(db.ImageSet.SingleOrDefault(i => i.PageId == this.PageId && i.OwnerId == this.Owner.Id) == null)
                            {
                                reload = true;
                                ImageFile = null;
                            }
                        }
                    }
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
                    token.ThrowIfCancellationRequested();
                    var buffer = await imageLoadResponse.Content.ReadAsBufferAsync();
                    var ext = Path.GetExtension(imageLoadResponse.RequestMessage.RequestUri.LocalPath);
                    var save = Owner.GalleryFolder.SaveFileAsync($"{PageId}{ext}", buffer);
                    ImageFile = await save;
                    using(var db = Models.CachedGalleryDb.Create())
                    {
                        var myModel = db.ImageSet.SingleOrDefault(model => model.ImageKey == this.ImageKey);
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
                image = null;
                RaisePropertyChanged(nameof(Image));
                RaisePropertyChanged(nameof(ImageFileUri));
            }
        }

        private static Uri ImageBaseUri = new Uri("ms-appdata:///localCache/");

        public Uri ImageFileUri
        {
            get
            {
                if(imageFile == null)
                    return null;
                return new Uri(ImageBaseUri,$"{Owner.Id}/{imageFile.Name}");
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
                    if(e == AsyncStatus.Error && op.ErrorCode is FileNotFoundException)
                    {
                        ImageFile = null;
                        State = ImageLoadingState.Waiting;
                    }
                    if(e != AsyncStatus.Completed)
                        return;
                    await DispatcherHelper.RunAsync(async () =>
                    {
                        try
                        {
                            using(var stream = op.GetResults())
                            {
                                await image.SetSourceAsync(stream);
                            }
                        }
                        catch(Exception)
                        {
                            this.State = ImageLoadingState.Failed;
                        }
                    });
                };
                return image;
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
