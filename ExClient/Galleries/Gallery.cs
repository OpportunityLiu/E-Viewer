using ExClient.Api;
using ExClient.Galleries.Commenting;
using ExClient.Galleries.Metadata;
using ExClient.Galleries.Rating;
using ExClient.Internal;
using ExClient.Models;
using ExClient.Tagging;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Opportunity.MvvmUniverse.AsyncHelpers;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Web.Http;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Galleries
{
    [JsonObject]
    [System.Diagnostics.DebuggerDisplay(@"\{ID = {ID} Count = {Count} RecordCount = {RecordCount}\}")]
    public class Gallery : IncrementalLoadingCollection<GalleryImage>
    {
        internal static readonly int PageSize = 20;

        public static IAsyncOperation<Gallery> TryLoadGalleryAsync(long galleryId)
        {
            return Task.Run(async () =>
            {
                using (var db = new GalleryDb())
                {
                    db.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                    var cm = db.SavedSet.SingleOrDefault(c => c.GalleryId == galleryId);
                    var gm = db.GallerySet.SingleOrDefault(g => g.GalleryModelId == galleryId);
                    if (gm == null)
                        return null;
                    var r = (cm == null) ?
                         new Gallery(gm) :
                         new SavedGallery(gm);
                    await r.InitAsync();
                    return r;
                }
            }).AsAsyncOperation();
        }

        public static IAsyncOperation<IList<Gallery>> FetchGalleriesAsync(IReadOnlyList<GalleryInfo> galleryInfo)
        {
            if (galleryInfo == null)
                throw new ArgumentNullException(nameof(galleryInfo));
            if (galleryInfo.Count <= 0)
                return AsyncWrapper.CreateCompleted((IList<Gallery>)Array.Empty<Gallery>());
            if (galleryInfo.Count <= 25)
            {
                return Run<IList<Gallery>>(async token =>
                {
                    var re = await new GalleryDataRequest(galleryInfo, 0, galleryInfo.Count).GetResponseAsync();
                    var data = re.GalleryMetaData;
                    data.ForEach(async g => await g.InitAsync());
                    return data;
                });
            }
            else
            {
                return Run<IList<Gallery>>(async token =>
                {
                    var result = new Gallery[galleryInfo.Count];
                    var pageCount = MathHelper.GetPageCount(galleryInfo.Count, 25);
                    for (var i = 0; i < pageCount; i++)
                    {
                        var pageSize = MathHelper.GetSizeOfPage(galleryInfo.Count, 25, i);
                        var startIndex = MathHelper.GetStartIndexOfPage(25, i);
                        var re = await new GalleryDataRequest(galleryInfo, startIndex, pageSize).GetResponseAsync();
                        var data = re.GalleryMetaData;
                        data.ForEach(async g => await g.InitAsync());
                        data.CopyTo(result, startIndex);
                    }
                    return result;
                });
            }
        }

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

        public virtual IAsyncActionWithProgress<SaveGalleryProgress> SaveAsync(ConnectionStrategy strategy)
        {
            return Run<SaveGalleryProgress>(async (token, progress) =>
            {
                var toReport = new SaveGalleryProgress
                {
                    ImageCount = this.RecordCount,
                    ImageLoadedInternal = -1
                };
                if (this.HasMoreItems)
                {
                    progress.Report(toReport);
                    while (this.HasMoreItems)
                    {
                        await this.LoadMoreItemsAsync((uint)PageSize);
                        token.ThrowIfCancellationRequested();
                    }
                }

                toReport.ImageLoadedInternal = 0;
                progress.Report(toReport);

                var loadTasks = this.Select(image => Task.Run(async () =>
                {
                    token.ThrowIfCancellationRequested();
                    await image.LoadImageAsync(false, strategy, true);
                    Interlocked.Increment(ref toReport.ImageLoadedInternal);
                    progress.Report(toReport);
                }));
                await Task.WhenAll(loadTasks);
                using (var db = new GalleryDb())
                {
                    var gid = this.ID;
                    var myModel = db.SavedSet.SingleOrDefault(model => model.GalleryId == gid);
                    if (myModel == null)
                    {
                        db.SavedSet.Add(new SavedGalleryModel().Update(this));
                    }
                    else
                    {
                        myModel.Update(this);
                    }
                    await db.SaveChangesAsync();
                }
            });
        }

        private Gallery(long id, ulong token)
        {
            this.ID = id;
            this.Token = token;
            this.Comments = new CommentCollection(this);
            this.GalleryUri = new Uri(Client.Current.Uris.RootUri, $"g/{ID.ToString()}/{Token.ToTokenString()}/");
        }

        internal Gallery(GalleryModel model)
            : this(model.GalleryModelId, model.Token)
        {
            this.Available = model.Available;
            this.Title = model.Title;
            this.TitleJpn = model.TitleJpn;
            this.Category = model.Category;
            this.Uploader = model.Uploader;
            this.Posted = model.Posted;
            this.FileSize = model.FileSize;
            this.Expunged = model.Expunged;
            this.rating = model.Rating;
            this.Tags = new TagCollection(this, JsonConvert.DeserializeObject<IList<string>>(model.Tags).Select(t => Tag.Parse(t)));
            this.RecordCount = model.RecordCount;
            this.ThumbUri = new Uri(model.ThumbUri);
            this.PageCount = MathHelper.GetPageCount(this.RecordCount, PageSize);
        }

        [JsonConstructor]
        internal Gallery(
            long gid,
            string error = null,
            string token = "0",
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
            : this(gid, token?.ToToken() ?? 0)
        {
            if (error != null)
            {
                throw new Exception(error);
            }
            this.Available = !expunged;
            this.Title = HtmlEntity.DeEntitize(title);
            this.TitleJpn = HtmlEntity.DeEntitize(title_jpn);
            if (!categoriesForRestApi.TryGetValue(category, out var ca))
                ca = Category.Unspecified;
            this.Category = ca;
            this.Uploader = HtmlEntity.DeEntitize(uploader);
            this.Posted = DateTimeOffset.FromUnixTimeSeconds(long.Parse(posted, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture));
            this.RecordCount = int.Parse(filecount, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
            this.FileSize = filesize;
            this.Expunged = expunged;
            this.rating = double.Parse(rating, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture);
            this.TorrentCount = int.Parse(torrentcount, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
            this.Tags = new TagCollection(this, tags.Select(tag => Tag.Parse(tag)));
            this.ThumbUri = toEhCoverUri(thumb);
            this.PageCount = MathHelper.GetPageCount(this.RecordCount, PageSize);
        }

        private static readonly Regex imageUriRegex = new Regex(@"^(?<scheme>[a-zA-Z][.a-zA-Z0-9+-]*)://(?<domain>((\w+\.)?ehgt\.org(/t)?)|(exhentai\.org/t)|((\d{1,3}\.){3}\d{1,3}))/(?<body>.+)(?<tail>_(l|250))\.(?<ext>\w+)$", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

        // from gtX.eght.org//_l.jpg
        // to   exhentai.org/t//_250.jpg
        private static Uri toEhCoverUri(string uri)
        {
            return new Uri(imageUriRegex.Replace(uri, @"${scheme}://ehgt.org/${body}_250.${ext}"));
        }

        private static Uri toEhUri(string uri)
        {
            return new Uri(imageUriRegex.Replace(uri, @"${scheme}://ehgt.org/${body}_l.${ext}"));
        }

        private static HttpClient coverClient { get; } = new HttpClient();

        protected IAsyncAction InitAsync()
        {
            //return Run(async token =>
            //{
            //    await InitOverrideAsync();
            //});
            return InitOverrideAsync();
        }

        protected virtual IAsyncAction InitOverrideAsync()
        {
            return Task.Run(() =>
            {
                using (var db = new GalleryDb())
                {
                    var gid = this.ID;
                    var myModel = db.GallerySet.SingleOrDefault(model => model.GalleryModelId == gid);
                    if (myModel == null)
                    {
                        db.GallerySet.Add(new GalleryModel().Update(this));
                    }
                    else
                    {
                        myModel.Update(this);
                    }
                    db.SaveChanges();
                }
            }).AsAsyncAction();
        }

        protected virtual IAsyncOperation<SoftwareBitmap> GetThumbAsync()
        {
            return Run(async token =>
            {
                try
                {
                    var buffer = await coverClient.GetBufferAsync(this.ThumbUri);
                    using (var stream = buffer.AsRandomAccessStream())
                    {
                        var decoder = await BitmapDecoder.CreateAsync(stream);
                        return await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            });
        }

        public Uri GalleryUri { get; }

        #region MetaData

        public long ID { get; }

        public bool Available { get; protected set; }

        public ulong Token { get; }

        public string Title { get; protected set; }

        public string TitleJpn { get; protected set; }

        public Category Category { get; protected set; }

        private readonly WeakReference<SoftwareBitmap> thumbImage = new WeakReference<SoftwareBitmap>(null);
        public SoftwareBitmap Thumb
        {
            get
            {
                if (this.thumbImage.TryGetTarget(out var img))
                    return img;
                var load = GetThumbAsync();
                load.Completed = (asyncInfo, asyncStatus) =>
                {
                    try
                    {
                        if (asyncStatus != AsyncStatus.Completed)
                            return;
                        var r = asyncInfo.GetResults();
                        if (r == null)
                            return;
                        if (this.thumbImage.TryGetTarget(out var img2))
                        {
                            img2.Dispose();
                        }
                        this.thumbImage.SetTarget(r);
                        OnPropertyChanged(nameof(Thumb));
                    }
                    finally
                    {
                        asyncInfo.Close();
                    }
                };
                return null;
            }
        }


        public Uri ThumbUri { get; }

        public string Uploader { get; }

        public DateTimeOffset Posted { get; }

        public long FileSize { get; }

        public bool Expunged { get; }

        private double rating;
        public double Rating { get => this.rating; protected set => Set(ref this.rating, value); }

        public IAsyncAction RatingAsync(Score rating)
        {
            return Run(async token =>
            {
                var r = RatingHelper.RatingAsync(this, rating);
                token.Register(r.Cancel);
                var result = await r;
                this.Rating = result.AverageRating;
            });
        }

        public int TorrentCount { get; }

        public TagCollection Tags { get; }

        public Language Language => Language.Parse(this);

        private FavoriteCategory favoriteCategory;
        public FavoriteCategory FavoriteCategory
        {
            get => this.favoriteCategory;
            protected internal set => Set(ref this.favoriteCategory, value);
        }

        private string favoriteNote;
        public string FavoriteNote
        {
            get => this.favoriteNote;
            protected internal set => Set(ref this.favoriteNote, value);
        }

        private RevisionCollection revisions;
        public RevisionCollection Revisions
        {
            get => this.revisions;
            private set => Set(ref this.revisions, value);
        }

        #endregion

        private static readonly Regex imgLinkMatcher = new Regex(@"/s/([0-9a-f]+)/(\d+)-(\d+)", RegexOptions.Compiled);

        private void updateFavoriteInfo(HtmlDocument html)
        {
            var favNode = html.GetElementbyId("fav");
            if (favNode == null)
                return;
            var favContentNode = favNode.Element("div");
            this.FavoriteCategory = Client.Current.Favorites.GetCategory(favContentNode);
        }

        protected override IAsyncOperation<IList<GalleryImage>> LoadPageAsync(int pageIndex)
        {
            return Task.Run<IList<GalleryImage>>(async () =>
            {
                var needLoadComments = !this.Comments.IsLoaded;
                var uri = new Uri(this.GalleryUri, $"?{(needLoadComments ? "hc=1&" : "")}p={pageIndex.ToString()}");
                var html = await Client.Current.HttpClient.GetDocumentAsync(uri);
                ApiRequest.UpdateToken(html.DocumentNode.OuterHtml);
                updateFavoriteInfo(html);
                if (needLoadComments)
                {
                    this.Comments.AnalyzeDocument(html);
                }
                if (this.Revisions == null)
                    this.Revisions = new RevisionCollection(this, html);
                this.Tags.Update(html);
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
                           let thumb = HtmlEntity.DeEntitize(nodeA.Descendants("img").Single().GetAttributeValue("src", null))
                           let match = imgLinkMatcher.Match(HtmlEntity.DeEntitize(nodeA.GetAttributeValue("href", "")))
                           where match.Success && thumb != null
                           select new
                           {
                               pageId = int.Parse(match.Groups[3].Value, System.Globalization.NumberStyles.Integer),
                               imageKey = match.Groups[1].Value.ToToken(),
                               thumbUri = toEhUri(thumb)
                           };
                var toAdd = new List<GalleryImage>(PageSize);
                using (var db = new GalleryDb())
                {
                    db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                    foreach (var page in pics)
                    {
                        var gId = this.ID;
                        var pId = page.pageId;
                        var imageModel = db.GalleryImageSet
                            .Include(gi => gi.Image)
                            .FirstOrDefault(gi => gi.GalleryId == gId && gi.PageId == pId);
                        if (imageModel != null)
                        {
                            // Load cache
                            var galleryImage = await GalleryImage.LoadCachedImageAsync(this, imageModel, imageModel.Image);
                            toAdd.Add(galleryImage);
                            continue;
                        }
                        toAdd.Add(new GalleryImage(this, page.pageId, page.imageKey, page.thumbUri));
                    }
                }
                return toAdd;
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
                var doc = await Client.Current.HttpClient.GetDocumentAsync(new Uri($"gallerypopups.php?gid={this.ID}&t={this.Token.ToTokenString()}&act=addfav", UriKind.Relative));
                var favdel = doc.GetElementbyId("favdel");
                if (favdel != null)
                {
                    var favSet = false;
                    for (var i = 0; i < 10; i++)
                    {
                        var favNode = doc.GetElementbyId($"fav{i}");
                        var favNameNode = favNode.ParentNode.ParentNode.Elements("div").Skip(2).First();
                        Client.Current.Favorites[i].Name = HtmlEntity.DeEntitize(favNameNode.InnerText);
                        if (!favSet && favNode.GetAttributeValue("checked", null) == "checked")
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
                var gid = this.ID;
                using (var db = new GalleryDb())
                {
                    var toDelete = db.GalleryImageSet
                        .Include(gi => gi.Image)
                        .Where(gi => gi.GalleryId == gid)
                        .ToList();
                    foreach (var item in toDelete)
                    {
                        var usingCount = db.GalleryImageSet
                            .Count(GalleryImageModel.FKEquals(item.ImageId));
                        if (usingCount <= 1)
                        {
                            var i = item.PageId - 1;
                            var file = default(StorageFile);
                            if (i < this.Count)
                                file = this[i].ImageFile;
                            if (file == null)
                            {
                                var folder = GalleryImage.ImageFolder ?? await GalleryImage.GetImageFolderAsync();
                                file = await folder.TryGetFileAsync(item.Image.FileName);
                            }
                            if (file != null)
                                await file.DeleteAsync();
                            db.ImageSet.Remove(item.Image);
                        }
                        db.GalleryImageSet.Remove(item);
                    }
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
