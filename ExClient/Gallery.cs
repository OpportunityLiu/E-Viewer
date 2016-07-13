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
using Windows.Storage;
using Windows.Storage.Search;
using ExClient.Models;
using GalaSoft.MvvmLight.Threading;
using Windows.Globalization;

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
        public static IAsyncOperation<Gallery> TryLoadGalleryAsync(long galleryId)
        {
            return Task.Run(async () =>
            {
                using(var db = GalleryDb.Create())
                {
                    var cm = db.CacheSet.SingleOrDefault(c => c.GalleryId == galleryId);
                    var gm = db.GallerySet.SingleOrDefault(g => g.Id == galleryId);
                    if(gm == null)
                        return null;
                    else
                    {
                        var r = (cm == null) ?
                             new Gallery(gm, true) :
                             new CachedGallery(gm, cm);
                        await r.InitAsync();
                        return r;
                    }
                }
            }).AsAsyncOperation();
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

        private IAsyncActionWithProgress<SaveGalleryProgress> saveTask;

        public virtual IAsyncActionWithProgress<SaveGalleryProgress> SaveGalleryAsync(ConnectionStrategy strategy)
        {
            if(saveTask?.Status != AsyncStatus.Started)
                saveTask = Run<SaveGalleryProgress>(async (token, progress) =>
                {
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

                    var thumb = (await Owner.HttpClient.GetBufferAsync(ThumbUri)).ToArray();
                    using(var db = GalleryDb.Create())
                    {
                        var myModel = db.CacheSet.SingleOrDefault(model => model.GalleryId == this.Id);
                        if(myModel == null)
                        {
                            db.CacheSet.Add(new CachedGalleryModel().Update(this, thumb));
                        }
                        else
                        {
                            db.CacheSet.Update(myModel.Update(this, thumb));
                        }
                        await db.SaveChangesAsync();
                    }
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

        internal Gallery(GalleryModel model, bool setThumbUriSource)
            : this(model.Id, model.Token, 0)
        {
            this.Id = model.Id;
            this.Available = model.Available;
            this.ArchiverKey = model.ArchiverKey;
            this.Token = model.Token;
            this.Title = model.Title;
            this.TitleJpn = model.TitleJpn;
            this.Category = model.Category;
            this.Uploader = model.Uploader;
            this.Posted = model.Posted;
            this.FileSize = model.FileSize;
            this.Expunged = model.Expunged;
            this.Rating = model.Rating;
            this.Tags = JsonConvert.DeserializeObject<IList<string>>(model.Tags).Select(t => new Tag(this, t)).ToList();
            this.RecordCount = model.RecordCount;
            this.ThumbUri = new Uri(model.ThumbUri);
            this.setThumbUriWhenInit = setThumbUriSource;
            this.Owner = Client.Current;
            if(this.RecordCount > 0)
                this.PageCount = 1;
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
                if(!categoriesForRestApi.TryGetValue(category, out ca))
                    ca = Category.Unspecified;
                this.Category = ca;
                this.Uploader = WebUtility.HtmlDecode(uploader);
                this.Posted = DateTimeOffset.FromUnixTimeSeconds(long.Parse(posted, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture));
                this.RecordCount = int.Parse(filecount, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                this.FileSize = filesize;
                this.Expunged = expunged;
                this.Rating = double.Parse(rating, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture);
                this.TorrentCount = int.Parse(torrentcount, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                this.Tags = new ReadOnlyCollection<Tag>(tags.Select(tag => new Tag(this, tag)).ToList());
                this.ThumbUri = toExUri(thumb);
            }
            catch(Exception)
            {
                Available = false;
            }
            if(this.RecordCount > 0)
                this.PageCount = 1;
        }

        private static readonly Regex toExUriRegex = new Regex(@"(?<domain>((gt\d|ul)\.ehgt\.org)|(ehgt\.org/t)|((\d{1,3}\.){3}\d{1,3}))(?<body>.+)(?<tail>_l\.)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        // from gtX.eght.org//_l.jpg
        // to   exhentai.org/t//_250.jpg
        private static Uri toExUri(string uri)
        {
            return new Uri(toExUriRegex.Replace(uri, @"exhentai.org/t${body}_250."));
        }

        private bool setThumbUriWhenInit = true;

        public IAsyncAction InitAsync()
        {
            return Run(async token =>
            {
                await initCoreAsync();
                await InitOverrideAsync();
            });
        }

        private IAsyncAction initCoreAsync()
        {
            return DispatcherHelper.RunAsync(() =>
            {
                var t = new BitmapImage();
                if(setThumbUriWhenInit)
                    t.UriSource = ThumbUri;
                this.Thumb = t;
            });
        }

        public virtual IAsyncAction InitOverrideAsync()
        {
            return Task.Run(() =>
            {
                using(var db = GalleryDb.Create())
                {
                    var myModel = db.GallerySet.SingleOrDefault(model => model.Id == this.Id);
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

        private BitmapImage thumbImage;

        public BitmapImage Thumb
        {
            get
            {
                return thumbImage;
            }
            private set
            {
                Set(ref thumbImage, value);
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

        public IReadOnlyList<Tag> Tags
        {
            get; protected set;
        }

        private static readonly string[] technicalTags = new string[]
        {
            "rewrite",
            "speechless",
            "text cleaned",
            "translated"
        };

        public string Language
        {
            get
            {
                return Tags.FirstOrDefault(t => t.NameSpace == NameSpace.Language && !technicalTags.Contains(t.Content))?.Content.ToUpper();
            }
        }

        #endregion

        public Client Owner
        {
            get; internal set;
        }

        public Uri GalleryUri
        {
            get; private set;
        }

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
                GalleryFolder = await StorageHelper.LocalCache.CreateFolderAsync(Id.ToString(), CreationCollisionOption.OpenIfExists);
                return galleryFolder;
            });
        }

        private static Uri galleryBaseUri = new Uri(Client.RootUri, "g/");
        private static readonly Regex picInfoMatcher = new Regex(@"width:\s*(\d+)px;\s*height:\s*(\d+)px;.*url\((.+)\)\s*-\s*(\d+)px", RegexOptions.Compiled);
        private static readonly Regex imgLinkMatcher = new Regex(@"/s/([0-9a-f]+)/(\d+)-(\d+)", RegexOptions.Compiled);

        protected override IAsyncOperation<uint> LoadPageAsync(int pageIndex)
        {
            return Task.Run(async () =>
            {
                if(GalleryFolder == null)
                {
                    await GetFolderAsync();
                }
                var uri = new Uri(GalleryUri, $"?p={pageIndex.ToString()}{(comments == null ? "hc=1" : "")}");
                var request = Owner.PostStrAsync(uri, null);
                var res = await request;
                var html = new HtmlDocument();
                html.LoadHtml(res);
                if(comments == null)
                    Comments = Comment.LoadComment(html);
                var pcNodes = html.DocumentNode.Descendants("td")
                              .Where(node => "document.location=this.firstChild.href" == node.GetAttributeValue("onclick", ""))
                              .Select(node =>
                              {
                                  int i;
                                  var su = int.TryParse(node.InnerText, out i);
                                  return Tuple.Create(su, i);
                              })
                              .Where(select => select.Item1)
                              .DefaultIfEmpty(Tuple.Create(true, 1))
                              .Max(select => select.Item2);
                PageCount = pcNodes;
                var pics = (from node in html.GetElementbyId("gdt").Descendants("div")
                            where node.GetAttributeValue("class", null) == "gdtm"
                            let nodeBackGround = node.Descendants("div").Single()
                            let matchUri = picInfoMatcher.Match(nodeBackGround.GetAttributeValue("style", ""))
                            where matchUri.Success
                            let nodeA = nodeBackGround.Descendants("a").Single()
                            let match = imgLinkMatcher.Match(nodeA.GetAttributeValue("href", ""))
                            where match.Success
                            let r = new
                            {
                                pageId = int.Parse(match.Groups[3].Value, System.Globalization.NumberStyles.Integer),
                                imageKey = match.Groups[1].Value,
                                thumbUri = new Uri(matchUri.Groups[3].Value),
                                width = uint.Parse(matchUri.Groups[1].Value, System.Globalization.NumberStyles.Integer),
                                height = uint.Parse(matchUri.Groups[2].Value, System.Globalization.NumberStyles.Integer) - 1,
                                offset = uint.Parse(matchUri.Groups[4].Value, System.Globalization.NumberStyles.Integer)
                            }
                            group r by r.thumbUri).ToDictionary(group => Owner.HttpClient.GetBufferAsync(group.Key));
                var toAdd = new List<GalleryImage>(40);
                using(var db = GalleryDb.Create())
                {
                    foreach(var group in pics)
                    {
                        using(var stream = (await group.Key).AsRandomAccessStream())
                        {
                            var decoder = await BitmapDecoder.CreateAsync(stream);
                            var transform = new BitmapTransform();
                            foreach(var page in group.Value)
                            {
                                var imageModel = db.ImageSet.SingleOrDefault(im => im.OwnerId == this.Id && im.PageId == page.pageId);
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
                                transform.Bounds = new BitmapBounds()
                                {
                                    Height = page.height,
                                    Width = page.width,
                                    X = page.offset,
                                    Y = 0
                                };
                                var pix = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, transform, ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.ColorManageToSRgb);
                                await DispatcherHelper.RunAsync(async () =>
                                {
                                    var image = new SoftwareBitmapSource();
                                    toAdd.Add(new GalleryImage(this, page.pageId, page.imageKey, image));
                                    await image.SetBitmapAsync(pix);
                                });
                            }
                        }
                    }
                }
                return (uint)AddRange(toAdd);
            }).AsAsyncOperation();
        }

        public IAsyncOperation<List<TorrentInfo>> LoadTorrnetsAsync()
        {
            return TorrentInfo.LoadTorrentsAsync(this);
        }

        private List<Comment> comments;

        public List<Comment> Comments
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

        public IAsyncOperation<List<Comment>> LoadCommentsAsync()
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
                if(GalleryFolder == null)
                {
                    await GetFolderAsync();
                }
                var temp = GalleryFolder;
                GalleryFolder = null;
                await temp.DeleteAsync();
                using(var db = GalleryDb.Create())
                {
                    db.ImageSet.RemoveRange(db.ImageSet.Where(i => i.OwnerId == this.Id));
                    await db.SaveChangesAsync();
                }
                var c = this.RecordCount;
                ResetAll();
                this.RecordCount = c;
                if(this.RecordCount > 0)
                    this.PageCount = 1;
            }).AsAsyncAction();
        }
    }
}