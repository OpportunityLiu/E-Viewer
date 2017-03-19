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
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient
{
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

        private class GalleryResult : ApiResponse
        {
#pragma warning disable IDE1006 // 命名样式
#pragma warning disable CS0649
            public List<Gallery> gmetadata;
#pragma warning restore CS0649
#pragma warning restore IDE1006 // 命名样式
        }

        public static IAsyncOperation<IReadOnlyList<Gallery>> FetchGalleriesAsync(IReadOnlyList<GalleryInfo> galleryInfo)
        {
            return Run<IReadOnlyList<Gallery>>(async token =>
            {
                async void myinit(Gallery g)
                {
                    await g.InitAsync();
                }
                var result = new Gallery[galleryInfo.Count];
                var pageCount = MathHelper.GetPageCount(galleryInfo.Count, 25);
                for(var i = 0; i < pageCount; i++)
                {
                    var pageSize = MathHelper.GetSizeOfPage(galleryInfo.Count, 25, i);
                    var startIndex = MathHelper.GetStartIndexOfPage(25, i);
                    var str = await Client.Current.HttpClient.PostApiAsync(new GalleryData(galleryInfo, startIndex, pageSize));
                    var re = JsonConvert.DeserializeObject<GalleryResult>(str);
                    re.CheckResponse();
                    var data = re.gmetadata;
                    data.ForEach(myinit);
                    data.CopyTo(result, startIndex);
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

                var thumb = (await Client.Current.HttpClient.GetBufferAsync(this.ThumbUri)).ToArray();
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
            this.Comments = new CommentCollection(this);
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
            this.PageCount = MathHelper.GetPageCount(this.RecordCount, PageSize);
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
                this.Available = false;
                return;
            }
            this.Available = !expunged;
            try
            {
                this.ArchiverKey = archiver_key;
                this.Title = HtmlEntity.DeEntitize(title);
                this.TitleJpn = HtmlEntity.DeEntitize(title_jpn);
                if(!categoriesForRestApi.TryGetValue(category, out var ca))
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
                try
                {
                    var buffer = await Client.Current.HttpClient.GetBufferAsync(this.ThumbUri);
                    using(var stream = buffer.AsRandomAccessStream())
                    {
                        var decoder = await BitmapDecoder.CreateAsync(stream);
                        this.Thumb = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                    }
                }
                catch(Exception)
                {
                    this.thumbImage?.Dispose();
                    this.Thumb = null;
                }
                await InitOverrideAsync();
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
            get => this.thumbImage;
            protected set => Set(ref this.thumbImage, value?.GetReadOnlyView());
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
            get => this.favorite;
            protected internal set => Set(ref this.favorite, value);
        }

        private FavoriteCategory favorite;

        public string FavoriteNote
        {
            get => this.favNote;
            protected internal set => Set(ref this.favNote, value);
        }

        private string favNote;

        public RevisionCollection Revisions { get; private set; }

        #endregion

        public Uri GalleryUri { get; }

        private StorageFolder galleryFolder;

        public StorageFolder GalleryFolder
        {
            get => this.galleryFolder;
            private set => Set(ref this.galleryFolder, value);
        }

        public IAsyncOperation<StorageFolder> GetFolderAsync()
        {
            return Run(async token =>
            {
                if(this.galleryFolder == null)
                    this.GalleryFolder = await StorageHelper.LocalCache.CreateFolderAsync(this.Id.ToString(), CreationCollisionOption.OpenIfExists);
                return this.galleryFolder;
            });
        }

        private static readonly Regex imgLinkMatcher = new Regex(@"/s/([0-9a-f]+)/(\d+)-(\d+)", RegexOptions.Compiled);

        private void updateFavoriteInfo(HtmlDocument html)
        {
            var favNode = html.GetElementbyId("fav");
            var favContentNode = favNode.Element("div");
            this.FavoriteCategory = Client.Current.Favorites.GetCategory(favContentNode);
        }

        protected override IAsyncOperation<IReadOnlyList<GalleryImage>> LoadPageAsync(int pageIndex)
        {
            return Task.Run(async () =>
            {
                await this.GetFolderAsync();
                var needLoadComments = !this.Comments.IsLoaded;
                var uri = new Uri(this.GalleryUri, $"?inline_set=ts_l&p={pageIndex.ToString()}{(needLoadComments ? "hc=1" : "")}");
                var request = Client.Current.HttpClient.GetStringAsync(uri);
                var res = await request;
                ApiRequest.UpdateToken(res);
                var html = new HtmlDocument();
                html.LoadHtml(res);
                updateFavoriteInfo(html);
                if(needLoadComments)
                {
                    this.Comments.AnalyzeDocument(html, false);
                }
                if(this.Revisions == null)
                    this.Revisions = new RevisionCollection(this, html);
                var pcNodes = html.DocumentNode.Descendants("td")
                    .Where(node => "document.location=this.firstChild.href" == node.GetAttributeValue("onclick", ""))
                    .Select(node =>
                    {
                        var succeed = int.TryParse(node.InnerText, out var number);
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
                this.PageCount = pcNodes;
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
                return (IReadOnlyList<GalleryImage>)toAdd;
            }).AsAsyncOperation();
        }

        public IAsyncOperation<ReadOnlyCollection<TorrentInfo>> FetchTorrnetsAsync()
        {
            return TorrentInfo.LoadTorrentsAsync(this);
        }

        public CommentCollection Comments { get; }

        public IAsyncOperation<string> FetchFavoriteNoteAsync()
        {
            return Run(async token =>
            {
                var r = await Client.Current.HttpClient.GetStringAsync(new Uri($"gallerypopups.php?gid={this.Id}&t={this.Token}&act=addfav", UriKind.Relative));
                var doc = new HtmlDocument();
                doc.LoadHtml(r);
                var favdel = doc.GetElementbyId("favdel");
                if(favdel != null)
                {
                    var favSet = false;
                    for(var i = 0; i < 10; i++)
                    {
                        var favNode = doc.GetElementbyId($"fav{i}");
                        var favNameNode = favNode.ParentNode.ParentNode.Elements("div").Skip(2).First();
                        Client.Current.Favorites[i].Name = HtmlEntity.DeEntitize(favNameNode.InnerText);
                        if(!favSet && favNode.GetAttributeValue("checked", null) == "checked")
                        {
                            this.FavoriteCategory = Client.Current.Favorites[i];
                            favSet = true;
                        }
                    }
                    this.FavoriteNote = HtmlEntity.DeEntitize(doc.DocumentNode.Descendants("textarea").First().InnerText);
                }
                else
                {
                    this.FavoriteCategory = FavoriteCategory.Removed;
                    this.FavoriteNote = "";
                }
                return this.FavoriteNote;
            });
        }

        public virtual IAsyncAction DeleteAsync()
        {
            return Task.Run(async () =>
            {
                var gid = this.Id;
                await GetFolderAsync();
                var temp = this.GalleryFolder;
                this.GalleryFolder = null;
                await temp.DeleteAsync();
                using(var db = new GalleryDb())
                {
                    db.ImageSet.RemoveRange(db.ImageSet.Where(i => i.OwnerId == gid));
                    await db.SaveChangesAsync();
                }
                var c = this.RecordCount;
                ResetAll();
                this.RecordCount = c;
                this.PageCount = MathHelper.GetPageCount(this.RecordCount, PageSize);
            }).AsAsyncAction();
        }
    }
}