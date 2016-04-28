using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using HtmlAgilityPack;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using System.Threading;

namespace ExClient
{
    public struct SaveGalleryProgress
    {
        public int ImageLoaded
        {
            get; internal set;
        }

        public int ImageCount
        {
            get; internal set;
        }
    }

    [JsonObject]
    public class Gallery : IncrementalLoadingCollection<GalleryImage>
    {
        private const string thumbFileName = "thumb.jpg";

        private static readonly Dictionary<string, Category> categories = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase)
        {
            ["Doujinshi"] = Category.Doujinshi,
            ["Manga"] = Category.Manga,
            ["Artist CG Sets"] = Category.ArtistCG,
            ["Game CG Sets"] = Category.GameCG,
            ["Western"] = Category.Western,
            ["Image Sets"] = Category.ImageSet,
            ["Non-H"] = Category.NonH,
            ["Cosplay"] = Category.Cosplay,
            ["Asian Porn"] = Category.AsianPorn,
            ["Misc"] = Category.Misc
        };

        public static IAsyncOperation<Gallery> LoadGalleryAsync(long id)
        {
            return LoadGalleryAsync(id, Client.Current);
        }

        public static IAsyncOperation<Gallery> LoadGalleryAsync(long id, Client owner)
        {
            return Run(async token =>
            {
                var cache = await GalleryCache.LoadCacheAsync(id);
                if(cache == null)
                    return null;
                var gallery = new Gallery(cache.Id, cache.Token, 1)
                {
                    ArchiverKey = cache.ArchiverKey,
                    Available = cache.Available,
                    Category = (Category)cache.Category,
                    Expunged = cache.Expunged,
                    FileSize = cache.FileSize,
                    Owner = owner,
                    PageCount = 1,
                    Posted = DateTimeOffset.FromUnixTimeSeconds(cache.Posted),
                    Rating = cache.Rating,
                    RecordCount = cache.RecordCount,
                    Tags = new ReadOnlyCollection<string>(cache.Tags),
                    Title = cache.Title,
                    TitleJpn = cache.TitleJpn,
                    Uploader = cache.Uploader
                };
                BitmapImage thumb;
                var thumbFile = await CacheHelper.LoadFileAsync(cache.Id.ToString(), thumbFileName);
                if(thumbFile == null)
                    thumb = new BitmapImage(new Uri(cache.Thumb));
                else
                    using(var source = await thumbFile.OpenReadAsync())
                    {
                        thumb = new BitmapImage();
                        await thumb.SetSourceAsync(source);
                    }
                gallery.Thumb = thumb;
                for(int i = 0; i < cache.ImageKeys.Count; i++)
                {
                    gallery.Add(await GalleryImage.LoadCachedImageAsync(gallery, i + 1, cache.ImageKeys[i]));
                }
                return gallery;
            });
        }

        private IAsyncActionWithProgress<SaveGalleryProgress> saveTask;

        public IAsyncActionWithProgress<SaveGalleryProgress> SaveGalleryAsync()
        {
            if(saveTask?.Status != AsyncStatus.Started)
                saveTask = Run<SaveGalleryProgress>(async (token, progress) =>
                 {
                     var loadThumb = Owner.HttpClient.GetBufferAsync(((BitmapImage)Thumb).UriSource);
                     var toReport = new SaveGalleryProgress
                     {
                         ImageCount = this.RecordCount,
                         ImageLoaded = -1
                     };
                     progress.Report(toReport);
                     while(this.HasMoreItems)
                     {
                         await this.LoadMoreItemsAsync(40);
                     }
                     toReport.ImageLoaded = 0;
                     progress.Report(toReport);
                     for(int i = 0; i < this.Count; i++)
                     {
                         var image = this[i];
                         if(image.State == ImageLoadingState.Loaded)
                         {
                             toReport.ImageLoaded++;
                             progress.Report(toReport);
                             continue;
                         }
                         var load = image.LoadImage(false);
                         await load;
                         toReport.ImageLoaded++;
                         progress.Report(toReport);
                     }
                     var cache = new GalleryCache(this);
                     var cacheThumb = CacheHelper.SaveFileAsync(this.Id.ToString(), thumbFileName, await loadThumb);
                     await cache.SaveCacheAsync();
                     await cacheThumb;
                 });
            return saveTask;
        }

        private Gallery(long id, string token, int loadedPageCount)
            : base(loadedPageCount)
        {
            this.Id = id;
            this.Token = token;
            this.GalleryUri = new Uri(galleryBaseUri, $"{Id.ToString()}/{Token}/");
        }

        [JsonConstructor]
        internal Gallery(
            long gid,
            string error = null,
            string token = null,
            string archiver_key = null,
            string title = null,
            string title_jpn = null,
            string category = null,
            string thumb = null,
            string uploader = null,
            string posted = null,
            string filecount = null,
            long filesize = 0,
            bool expunged = true,
            string rating = null,
            string torrentcount = null,
            string[] tags = null)
            : this(gid, token, 0)
        {
            this.Id = gid;
            if(error != null)
            {
                Available = false;
                return;
            }
            Available = !expunged;
            try
            {
                this.Token = token;
                this.ArchiverKey = archiver_key;
                this.Title = WebUtility.HtmlDecode(title);
                this.TitleJpn = WebUtility.HtmlDecode(title_jpn);
                Category ca;
                if(!categories.TryGetValue(category, out ca))
                    ca = Category.Unknown;
                this.Category = ca;
                this.Thumb = new BitmapImage(new Uri(thumb));
                this.Uploader = WebUtility.HtmlDecode(uploader);
                this.Posted = DateTimeOffset.FromUnixTimeSeconds(long.Parse(posted, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture));
                this.RecordCount = int.Parse(filecount, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                this.FileSize = filesize;
                this.Expunged = expunged;
                this.Rating = double.Parse(rating, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture);
                this.TorrentCount = int.Parse(torrentcount, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                this.Tags = new ReadOnlyCollection<string>(tags);
            }
            catch(Exception)
            {
                Available = false;
            }
            if(this.RecordCount > 0)
                this.PageCount = 1;
        }

        #region MetaData

        public long Id
        {
            get; private set;
        }

        public bool Available
        {
            get; private set;
        }

        public string Token
        {
            get; private set;
        }

        public string ArchiverKey
        {
            get; private set;
        }

        public string Title
        {
            get; private set;
        }

        public string TitleJpn
        {
            get; private set;
        }

        public Category Category
        {
            get; private set;
        }

        public ImageSource Thumb
        {
            get; private set;
        }

        public string Uploader
        {
            get; private set;
        }

        public DateTimeOffset Posted
        {
            get; private set;
        }

        public long FileSize
        {
            get; private set;
        }

        public bool Expunged
        {
            get; private set;
        }

        public double Rating
        {
            get; private set;
        }

        public int TorrentCount
        {
            get; private set;
        }

        public IReadOnlyList<string> Tags
        {
            get; private set;
        }

        #endregion
        private int currentPage;

        public int CurrentPage
        {
            get
            {
                return currentPage;
            }
            set
            {
                Set(ref currentPage, value);
            }
        }

        public Client Owner
        {
            get; internal set;
        }

        public Uri GalleryUri
        {
            get; private set;
        }

        private static Uri galleryBaseUri = new Uri(Client.RootUri, "g/");

        protected override IAsyncOperation<uint> LoadPage(int pageIndex)
        {
            return Run(async token =>
            {
                var uri = new Uri(GalleryUri, $"?p={pageIndex.ToString()}");
                var request = Owner.PostStrAsync(uri, null);
                token.Register(request.Cancel);
                var res = await request;
                var html = new HtmlDocument();
                html.LoadHtml(res);
                var pcNodes = html.DocumentNode.Descendants("td")
                            .Where(node => "document.location=this.firstChild.href" == node.GetAttributeValue("onclick", ""))
                            .Select(node =>
                            {
                                int i;
                                var su = int.TryParse(node.InnerText, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out i);
                                return Tuple.Create(su, i);
                            })
                            .Where(select => select.Item1)
                            .DefaultIfEmpty(Tuple.Create(true, 1))
                            .Max(select => select.Item2);
                PageCount = pcNodes;
                var pics = (from node in html.GetElementbyId("gdt").Descendants("div")
                            where node.GetAttributeValue("class", null) == "gdtm"
                            let nodeBackGround = node.Descendants("div").Single()
                            let matchUri = Regex.Match(nodeBackGround.GetAttributeValue("style", ""),
                            @"width:\s*(\d+)px;\s*height:\s*(\d+)px;.*url\((.+)\)\s*-\s*(\d+)px")
                            where matchUri.Success
                            let nodeA = nodeBackGround.Descendants("a").Single()
                            let match = Regex.Match(nodeA.GetAttributeValue("href", ""), @"/s/([0-9a-f]+)/(\d+)-(\d+)")
                            where match.Success
                            let r = new
                            {
                                page = int.Parse(match.Groups[3].Value, System.Globalization.NumberStyles.Integer),
                                imageKey = match.Groups[1].Value,
                                thumbUri = new Uri(matchUri.Groups[3].Value),
                                width = uint.Parse(matchUri.Groups[1].Value, System.Globalization.NumberStyles.Integer),
                                height = uint.Parse(matchUri.Groups[2].Value, System.Globalization.NumberStyles.Integer) - 1,
                                offset = uint.Parse(matchUri.Groups[4].Value, System.Globalization.NumberStyles.Integer)
                            }
                            group r by r.thumbUri).ToDictionary(group => Owner.HttpClient.GetBufferAsync(group.Key).AsTask());
                var count = 0u;
                await Task.WhenAll(pics.Keys);
                foreach(var group in pics)
                {
                    var buf = group.Key.Result;
                    var decoder = await BitmapDecoder.CreateAsync(buf.AsStream().AsRandomAccessStream());
                    var transform = new BitmapTransform();
                    foreach(var page in group.Value)
                    {
                        {
                            var image = await GalleryImage.LoadCachedImageAsync(this, page.page, page.imageKey);
                            if(image != null)
                            {
                                this.Add(image);
                                count++;
                                continue;
                            }
                        }
                        transform.Bounds = new BitmapBounds()
                        {
                            Height = page.height,
                            Width = page.width,
                            X = page.offset,
                            Y = 0
                        };
                        using(var thumb = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, transform, ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage))
                        {
                            var image = new WriteableBitmap(thumb.PixelWidth, thumb.PixelHeight);
                            thumb.CopyToBuffer(image.PixelBuffer);
                            this.Add(new GalleryImage(this, page.page, page.imageKey, image));
                            count++;
                        }
                    }
                }
                return count;
            });
        }
    }
}