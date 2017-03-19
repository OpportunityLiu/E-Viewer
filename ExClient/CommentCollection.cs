using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Web.Http;

namespace ExClient
{
    public sealed class CommentCollection : IncrementalLoadingCollection<Comment>
    {
        public CommentCollection(Gallery owner)
            : base(0)
        {
            this.Owner = owner;
        }

        public Gallery Owner { get; }

        private bool isLoaded;

        public bool IsLoaded
        {
            get => isLoaded;
            set => Set(nameof(IsEmpty), ref this.isLoaded, value);
        }

        public new bool IsEmpty => this.Count == 0 && this.IsLoaded;

        private readonly object syncroot = new object();

        public IAsyncOperation<int> FetchAsync()
        {
            return AsyncInfo.Run(async token =>
            {
                this.Clear();
                this.IsLoaded = false;
                var get = Client.Current.HttpClient.GetStringAsync(new Uri(this.Owner.GalleryUri, "?hc=1"));
                token.Register(get.Cancel);
                var html = await get;
                if(this.IsLoaded)
                    return this.Count;
                var document = new HtmlDocument();
                document.LoadHtml(html);
                Api.ApiRequest.UpdateToken(html);
                return AnalyzeDocument(document, false);
            });
        }

        internal int AnalyzeDocument(HtmlDocument doc, bool reload)
        {
            if(this.IsLoaded && !reload)
                return this.Count;
            lock(this.syncroot)
            {
                if(this.IsLoaded && !reload)
                    return this.Count;
                var newValues = Comment.AnalyzeDocument(this, doc).ToList();
                var count = 0;
                if(this.Count == 0)
                {
                    count = this.AddRange(newValues);
                }
                else
                {
                    var pairsO = new List<Comment>();
                    var pairsN = new List<Comment>();
                    for(int oi = 0, ni = 0; ni < newValues.Count; ni++)
                    {
                        var o = oi < this.Count ? this[oi] : null;
                        var n = newValues[ni];
                        if(o?.Id == n.Id)
                        {
                            pairsO.Add(o);
                            pairsN.Add(n);
                            o.Score = n.Score;
                            o.Status = n.Status;
                            o.Edited = n.Edited;
                            o.Content = n.Content;
                            oi++;
                        }
                        else
                        {
                            pairsO.Add(null);
                            pairsN.Add(n);
                        }
                    }
                    var removeIndex = new List<int>();
                    for(var i = 0; i < this.Count; i++)
                    {
                        if(!pairsO.Contains(this[i]))
                        {
                            count--;
                            removeIndex.Add(i);
                        }
                    }
                    foreach(var item in removeIndex)
                    {
                        this.RemoveAt(item);
                    }
                    for(var i = 0; i < pairsN.Count; i++)
                    {
                        var o = pairsO[i];
                        var n = pairsN[i];
                        if(o == null)
                        {
                            this.Insert(i, n);
                            count++;
                        }
                    }
                }
                this.IsLoaded = true;
                return count;
            }
        }

        public IAsyncAction PostCommentAsync(string content)
        {
            return PostFormAsync(content, null);
        }

        private static Encoding encoding = Encoding.UTF8;

        internal IAsyncAction PostFormAsync(string content, Comment editable)
        {
            content = (content ?? "").Trim();
            if(string.IsNullOrEmpty(content))
                throw new ArgumentException(LocalizedStrings.Resources.EmptyComment);
            var length = encoding.GetByteCount(content);
            if(length < 10)
                throw new ArgumentException(LocalizedStrings.Resources.ShortComment);
            return AsyncInfo.Run(async token =>
            {
                IEnumerable<KeyValuePair<string, string>> getData()
                {
                    yield return new KeyValuePair<string, string>("commenttext", content);
                    if(editable != null && editable.Status == CommentStatus.Editable)
                    {
                        yield return new KeyValuePair<string, string>("edit_comment", editable.Id.ToString());
                        yield return new KeyValuePair<string, string>("postcomment", "Edit Comment");
                    }
                    else
                    {
                        yield return new KeyValuePair<string, string>("postcomment", "Post Comment");
                    }
                }
                var request = new HttpFormUrlEncodedContent(getData());
                var requestTask = Client.Current.HttpClient.PostAsync(new Uri(this.Owner.GalleryUri, "?hc=1"), request);
                token.Register(requestTask.Cancel);
                var response = await requestTask;
                var responseStr = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(responseStr);
                var cdiv = doc.GetElementbyId("cdiv");
                var pbr = cdiv.Element("p");
                if(pbr != null)
                {
                    var error = HtmlEntity.DeEntitize(pbr.InnerText).Trim();
                    switch(error)
                    {
                    case "You can only add comments for active galleries.":
                        error = LocalizedStrings.Resources.WrongGalleryState;
                        break;
                    case "You did not enter a valid comment.":
                        error = LocalizedStrings.Resources.EmptyComment;
                        break;
                    case "Your comment is too short.":
                        error = LocalizedStrings.Resources.ShortComment;
                        break;
                    default:
                        break;
                    }
                    throw new InvalidOperationException(error);
                }
                AnalyzeDocument(doc, true);
            });
        }

        protected override IAsyncOperation<IReadOnlyList<Comment>> LoadPageAsync(int pageIndex)
        {
            throw new NotImplementedException();
        }
    }
}
