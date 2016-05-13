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
        protected const string thumbFileName = "thumb.jpg";

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

        /// <summary>
        /// 查询已缓存的 <see cref="Gallery"/> 列表
        /// </summary>
        /// <returns>包含已缓存的 <see cref="Id"/> 列表</returns>
        public static IAsyncOperation<ICollection<long>> GetCachedGalleriesAsync()
        {
            return Run<ICollection<long>>(async token =>
            {
                var option = new Windows.Storage.Search.QueryOptions(Windows.Storage.Search.CommonFileQuery.DefaultQuery, new string[] { ".json" })
                {
                    FolderDepth = Windows.Storage.Search.FolderDepth.Deep,
                    ApplicationSearchFilter = "info"
                };
                var query = CacheHelper.LocalCache.CreateFileQueryWithOptions(option);
                var files = await query.GetFilesAsync();
                var list = new List<long>();
                foreach(var item in files)
                {
                    var d = Path.GetFileName(Path.GetDirectoryName(item.Path));
                    long id;
                    if(long.TryParse(d, out id))
                        list.Add(id);
                }
                return list;
            });
        }

        /// <summary>
        /// 清空缓存（包含自动缓存的文件）
        /// </summary>
        public static IAsyncAction ClearCachedGalleriesAsync()
        {
            return Run(async token =>
            {
                foreach(var item in await CacheHelper.LocalCache.GetItemsAsync())
                {
                    await item.DeleteAsync();
                }
            });
        }

        /// <summary>
        /// 从缓存中载入相应的 <see cref="Gallery"/>
        /// </summary>
        /// <param name="id">要载入的 <see cref="Id"/></param>
        /// <returns>相应的 <see cref="Gallery"/></returns>
        public static IAsyncOperation<Gallery> LoadGalleryAsync(long id)
        {
            return LoadGalleryAsync(id, Client.Current);
        }

        /// <summary>
        /// 从缓存中载入相应的 <see cref="Gallery"/>
        /// </summary>
        /// <param name="id">要载入的 <see cref="Id"/></param>
        /// <param name="owner">要设置的 <see cref="Owner"/></param>
        /// <returns>相应的 <see cref="Gallery"/></returns>
        public static IAsyncOperation<Gallery> LoadGalleryAsync(long id, Client owner)
        {
            return Run<Gallery>(async token =>
            {
                var cache = await GalleryCache.LoadCacheAsync(id);
                if(cache == null)
                    return null;
                var gallery = new CachedGallery(cache, owner);
                await gallery.InitAsync();
                return gallery;
            });
        }

        private IAsyncActionWithProgress<SaveGalleryProgress> saveTask;

        public virtual IAsyncActionWithProgress<SaveGalleryProgress> SaveGalleryAction => saveTask;

        public virtual IAsyncActionWithProgress<SaveGalleryProgress> SaveGalleryAsync()
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
                         if(image.State != ImageLoadingState.Loaded)
                         {
                             var load = image.LoadImage(false, true);
                             await load;
                         }
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

        protected Gallery(long id, string token, int loadedPageCount)
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
                    ca = Category.Unspecified;
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
            get; protected set;
        }

        public bool Available
        {
            get; protected set;
        }

        public string Token
        {
            get; protected set;
        }

        public string ArchiverKey
        {
            get; protected set;
        }

        public string Title
        {
            get; protected set;
        }

        public string TitleJpn
        {
            get; protected set;
        }

        public Category Category
        {
            get; protected set;
        }

        public ImageSource Thumb
        {
            get; protected set;
        }

        public string Uploader
        {
            get; protected set;
        }

        public DateTimeOffset Posted
        {
            get; protected set;
        }

        public long FileSize
        {
            get; protected set;
        }

        public bool Expunged
        {
            get; protected set;
        }

        public double Rating
        {
            get; protected set;
        }

        public int TorrentCount
        {
            get; protected set;
        }

        public IReadOnlyList<string> Tags
        {
            get; protected set;
        }

        #endregion
        private int currentImage;

        public int CurrentImage
        {
            get
            {
                return currentImage;
            }
            set
            {
                Set(ref currentImage, value);
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