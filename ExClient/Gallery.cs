using ExClient.Api;
using ExClient.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient
{
    public class SaveGalleryProgress
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
    [System.Diagnostics.DebuggerDisplay(@"\{Id = {Id} Count = {Count} RecordCount = {RecordCount}\}")]
    public class Gallery : IncrementalLoadingCollection<GalleryImage>
    {
        internal static readonly int PageSize = 20;

        public static IAsyncOperation<Gallery> TryLoadGalleryAsync(long galleryId)
        {
            return Task.Run(async () =>
            {
                using(var db = new GalleryDb())
                {
                    var cm = db.SavedSet.SingleOrDefault(c => c.GalleryId == galleryId);
                    var gm = db.GallerySet.SingleOrDefault(g => g.Id == galleryId);
                    if(gm == null)
                        return null;
                    else
                    {
                        var r = (cm == null) ?
                             new Gallery(gm) :
                             new SavedGallery(gm, cm);
                        await r.InitAsync();
                        return r;
                    }
                }
            }).AsAsyncOperation();
        }

        public static IAsyncOperation<IList<Gallery>> FetchGalleriesAsync(IReadOnlyList<GalleryInfo> galleryInfo)
        {
            if(galleryInfo == null)
                throw new ArgumentNullException(nameof(galleryInfo));
            return Run<IList<Gallery>>(async token =>
            {
                var type = new
                {
                    gmetadata = (IList<Gallery>)null
                };
                var result = new Gallery[galleryInfo.Count];
                for(var i = 0; i < galleryInfo.Count; i += 25)
                {
                    var pageCount = i + 25 < galleryInfo.Count ? 25 : galleryInfo.Count - i;
                    var str = await Client.Current.HttpClient.PostApiAsync(new GalleryData(galleryInfo, i, pageCount));
                    var re = JsonConvert.DeserializeAnonymousType(str, type).gmetadata;
                    for(var j = 0; j < re.Count; j++)
                    {
                        var item = re[j];
                        item.Owner = Client.Current;
                        var ignore = item.InitAsync();
                        result[i + j] = item;
                    }
                }
                return result;
            });
        }

        internal const string ThumbFileName = "thumb.jpg";

        private static readonly IReadOnlyDictionary<string, Category> categoriesForRestApi = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase)
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

        public virtual IAsyncActionWithProgress<SaveGalleryProgress> SaveGalleryAsync(ConnectionStrategy strategy)
        {
            return Run<SaveGalleryProgress>(async (token, progress) =>
            {
                var toReport = new SaveGalleryProgress
                {
                    ImageCount = this.RecordCount,
                    ImageLoaded = -1
                };
                progress.Report(toReport);
                while(this.HasMoreItems)
                {
                    await this.LoadMoreItemsAsync((uint)PageSize);
                }
                toReport.ImageLoaded = 0;
                progress.Report(toReport);

                var loadTasks = this.Select(image => Task.Run(async () =>
                {
                    await image.LoadImageAsync(false, strategy, true);
                    lock(toReport)
                    {
                        toReport.ImageLoaded++;
                        progress.Report(toReport);
                    }
                }));
                await Task.WhenAll(loadTasks);

                var thumb = (await this.Owner.HttpClient.GetBufferAsync(this.ThumbUri)).ToArray();
                using(var db = new GalleryDb())
                {
                    var gid = this.Id;
                    var myModel = db.SavedSet.SingleOrDefault(model => model.GalleryId == gid);
                    if(myModel == null)
                    {
                        db.SavedSet.Add(new SavedGalleryModel().Update(this, thumb));
                    }
                    else
                    {
                        db.SavedSet.Update(myModel.Update(this, thumb));
                    }
                    await db.SaveChangesAsync();
                }
            });
        }

        private Gallery(long id, string token)
            : base(0)
        {
            this.Id = id;
            this.Token = token;
            this.GalleryUri = new Uri(Client.Current.Uris.RootUri, $"g/{Id.ToString()}/{Token}/");
        }

        internal Gallery(GalleryModel model)
            : this(model.Id, model.Token)
        {
            this.Available = model.Available;
            this.ArchiverKey = model.ArchiverKey;
            this.Title = model.Title;
            this.TitleJpn = model.TitleJpn;
            this.Category = model.Category;
            this.Uploader = model.Uploader;
            this.Posted = model.Posted;
            this.FileSize = model.FileSize;
            this.Expunged = model.Expunged;
            this.Rating = model.Rating;
            this.Tags = new TagCollection(JsonConvert.DeserializeObject<IList<string>>(model.Tags).Select(t => Tag.Parse(t)));
            this.RecordCount = model.RecordCount;
            this.ThumbUri = new Uri(model.ThumbUri);
            this.Owner = Client.Current;
            this.PageCount = MathHelper.GetPageCount(RecordCount, PageSize);
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
            : this(gid, token)
        {
            if(error != null)
            {
                Available = false;
                return;
            }
            Available = !expunged;
            try
            {
                this.ArchiverKey = archiver_key;
                this.Title = HtmlEntity.DeEntitize(title);
                this.TitleJpn = HtmlEntity.DeEntitize(title_jpn);
                Category ca;
                if(!categoriesForRestApi.TryGetValue(category, out ca))
                    ca = Category.Unspecified;
                this.Category = ca;
                this.Uploader = HtmlEntity.DeEntitize(uploader);
                this.Posted = DateTimeOffset.FromUnixTimeSeconds(long.Parse(posted, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture));
                this.RecordCount = int.Parse(filecount, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                this.FileSize = filesize;
                this.Expunged = expunged;
                this.Rating = double.Parse(rating, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture);
                this.TorrentCount = int.Parse(torrentcount, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                this.Tags = new TagCollection(tags.Select(tag => Tag.Parse(tag)));
                this.ThumbUri = toExUri(thumb);
            }
            catch(Exception)
            {
                this.Available = false;
            }
            this.PageCount = MathHelper.GetPageCount(this.RecordCount, PageSize);
        }

        private static readonly Regex toExUriRegex = new Regex(@"(?<domain>((gt\d|ul)\.ehgt\.org)|(ehgt\.org/t)|((\d{1,3}\.){3}\d{1,3}))(?<body>.+)(?<tail>_l\.)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        // from gtX.eght.org//_l.jpg
        // to   exhentai.org/t//_250.jpg
        private static Uri toExUri(string uri)
        {
            return new Uri(toExUriRegex.Replace(uri, @"exhentai.org/t${body}_250."));
        }

        protected IAsyncAction InitAsync()
        {
            return Run(async token =>
            {
                await InitOverrideAsync();
                if(this.thumbImage != null)
                    return;
                var buffer = await Client.Current.HttpClient.GetBufferAsync(ThumbUri);
                using(var stream = buffer.AsRandomAccessStream())
                {
                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    this.Thumb = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }
            });
        }

        protected virtual IAsyncAction InitOverrideAsync()
        {
            return Task.Run(() =>
            {
                using(var db = new GalleryDb())
                {
                    var gid = this.Id;
                    var myModel = db.GallerySet.SingleOrDefault(model => model.Id == gid);
                    if(myModel == null)
                    {
                        db.GallerySet.Add(new GalleryModel().Update(this));
                    }
                    else
                    {
                        db.GallerySet.Update(myModel.Update(this));
                    }
                    db.SaveChanges();
                }
            }).AsAsyncAction();
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

        private SoftwareBitmap thumbImage;

        public SoftwareBitmap Thumb
        {
            get
            {
                return this.thumbImage;
            }
            protected set
            {
                Set(ref this.thumbImage, value?.GetReadOnlyView());
            }
        }

        public Uri ThumbUri
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

        public TagCollection Tags
        {
            get;
        }

        public Language Language => Language.Parse(this);

        public FavoriteCategory FavoriteCategory
        {
            get
            {
                return this.favorite;
            }
            protected internal set
            {
                Set(ref this.favorite, value);
            }
        }

        private FavoriteCategory favorite;

        public string FavoriteNote
        {
            get
            {
                return this.favNote;
            }
            protected internal set
            {
                Set(ref this.favNote, value);
            }
        }

        private string favNote;

        #endregion

        protected internal Client Owner
        {
            get; protected set;
        }

        public Uri GalleryUri { get; }

        private StorageFolder galleryFolder;

        public StorageFolder GalleryFolder
        {
            get
            {
                return galleryFolder;
            }
            private set
            {
                Set(ref galleryFolder, value);
            }
        }

        public IAsyncOperation<StorageFolder> GetFolderAsync()
        {
            return Run(async token =>
            {
                if(galleryFolder == null)
                    GalleryFolder = await StorageHelper.LocalCache.CreateFolderAsync(Id.ToString(), CreationCollisionOption.OpenIfExists);
                return galleryFolder;
            });
        }

        private static readonly Regex imgLinkMatcher = new Regex(@"/s/([0-9a-f]+)/(\d+)-(\d+)", RegexOptions.Compiled);

        private void updateFavoriteInfo(HtmlDocument html)
        {
            var favNode = html.GetElementbyId("fav");
            var favContentNode = favNode.Element("div");
            this.FavoriteCategory = Owner.Favorites.GetCategory(favContentNode);
        }

        protected override IAsyncOperation<IList<GalleryImage>> LoadPageAsync(int pageIndex)
        {
            return Task.Run(async () =>
            {
                await this.GetFolderAsync();
                var needLoadComments = comments == null;
                var uri = new Uri(this.GalleryUri, $"?inline_set=ts_l&p={pageIndex.ToString()}{(needLoadComments ? "hc=1" : "")}");
                var request = this.Owner.HttpClient.GetStringAsync(uri);
                var res = await request;
                ApiRequest.UpdateToken(res);
                var html = new HtmlDocument();
                html.LoadHtml(res);
                updateFavoriteInfo(html);
                if(needLoadComments)
                    this.Comments = Comment.LoadComment(html);
                var pcNodes = html.DocumentNode.Descendants("td")
                    .Where(node => "document.location=this.firstChild.href" == node.GetAttributeValue("onclick", ""))
                    .Select(node =>
                    {
                        int number;
                        var succeed = int.TryParse(node.InnerText, out number);
                        return new
                        {
                            succeed,
                            number
                        };
                    })
                    .Where(select => select.succeed)
                    .DefaultIfEmpty(new
                    {
                        succeed = true,
                        number = 1
                    })
                    .Max(select => select.number);
                PageCount = pcNodes;
                var pics = from node in html.GetElementbyId("gdt").Descendants("div")
                           where node.GetAttributeValue("class", null) == "gdtl"
                           let nodeA = node.Descendants("a").Single()
                           let nodeI = nodeA.Descendants("img").Single()
                           let thumb = nodeI.GetAttributeValue("src", null)
                           let imgLink = nodeA.GetAttributeValue("href", null)
                           let match = imgLinkMatcher.Match(nodeA.GetAttributeValue("href", ""))
                           where match.Success && thumb != null
                           select new
                           {
                               pageId = int.Parse(match.Groups[3].Value, System.Globalization.NumberStyles.Integer),
                               imageKey = match.Groups[1].Value,
                               thumbUri = new Uri(thumb)
                           };
                var toAdd = new List<GalleryImage>(PageSize);
                using(var db = new GalleryDb())
                {
                    foreach(var page in pics)
                    {
                        var imageKey = page.imageKey;
                        var imageModel = db.ImageSet.FirstOrDefault(im => im.ImageKey == imageKey);
                        if(imageModel != null)
                        {
                            // Load cache
                            var galleryImage = await GalleryImage.LoadCachedImageAsync(this, imageModel);
                            if(galleryImage != null)
                            {
                                toAdd.Add(galleryImage);
                                continue;
                            }
                        }
                        toAdd.Add(new GalleryImage(this, page.pageId, page.imageKey, page.thumbUri));
                    }
                }
                return (IList<GalleryImage>)toAdd;
            }).AsAsyncOperation();
        }

        public IAsyncOperation<ReadOnlyCollection<TorrentInfo>> LoadTorrnetsAsync()
        {
            return TorrentInfo.LoadTorrentsAsync(this);
        }

        private ReadOnlyCollection<Comment> comments;

        public ReadOnlyCollection<Comment> Comments
        {
            get
            {
                return comments;
            }
            protected set
            {
                Set(ref comments, value);
            }
        }

        public IAsyncOperation<ReadOnlyCollection<Comment>> LoadCommentsAsync()
        {
            return Run(async token =>
            {
                Comments = await Comment.LoadCommentsAsync(this);
                return comments;
            });
        }

        public virtual IAsyncAction DeleteAsync()
        {
            return Task.Run(async () =>
            {
                var gid = this.Id;
                await GetFolderAsync();
                var temp = GalleryFolder;
                GalleryFolder = null;
                await temp.DeleteAsync();
                using(var db = new GalleryDb())
                {
                    db.ImageSet.RemoveRange(db.ImageSet.Where(i => i.OwnerId == gid));
                    await db.SaveChangesAsync();
                }
                var c = this.RecordCount;
                ResetAll();
                this.RecordCount = c;
                this.PageCount = MathHelper.GetPageCount(RecordCount, PageSize);
            }).AsAsyncAction();
        }
    }
}