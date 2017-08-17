using HtmlAgilityPack;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Web.Http;

namespace ExClient.Galleries.Commenting
{
    public sealed class CommentCollection : ObservableList<Comment>
    {
        public CommentCollection(Gallery owner)
        {
            this.Owner = owner;
        }

        public Gallery Owner { get; }

        private bool isLoaded;

        public bool IsLoaded
        {
            get => isLoaded;
            private set => Set(nameof(IsEmpty), ref this.isLoaded, value);
        }

        public bool IsEmpty => this.Count == 0 && this.IsLoaded;

        private readonly object syncroot = new object();

        public IAsyncAction FetchAsync()
        {
            return fetchAsync(true);
        }

        private IAsyncAction fetchAsync(bool reload)
        {
            return AsyncInfo.Run(async token =>
            {
                if (reload)
                {
                    this.Clear();
                    this.IsLoaded = false;
                }
                var get = Client.Current.HttpClient.GetDocumentAsync(new Uri(this.Owner.GalleryUri, "?hc=1"));
                token.Register(get.Cancel);
                var document = await get;
                Api.ApiRequest.UpdateToken(document.DocumentNode.OuterHtml);
                AnalyzeDocument(document);
                return;
            });
        }

        private class CommentEqualityComparer : IEqualityComparer<Comment>
        {
            public bool Equals(Comment x, Comment y) => x?.ID == y?.ID;

            public int GetHashCode(Comment obj) => obj == null ? 0 : obj.ID.GetHashCode();

            public static CommentEqualityComparer Current { get; } = new CommentEqualityComparer();
        }

        internal void AnalyzeDocument(HtmlDocument doc)
        {
            lock (this.syncroot)
            {
                var newValues = Comment.AnalyzeDocument(this, doc).ToList();
                this.Update(newValues, CommentEqualityComparer.Current, (o, n) =>
                {
                    o.Score = n.Score;
                    o.Status = n.Status;
                    o.Edited = n.Edited;
                    o.Content = n.Content;
                });
                this.IsLoaded = true;
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
            content = content.Replace("\r\n", "\n").Replace('\r', '\n');
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException(LocalizedStrings.Resources.EmptyComment);
            var length = encoding.GetByteCount(content);
            if (length < 10)
                throw new ArgumentException(LocalizedStrings.Resources.ShortComment);
            return AsyncInfo.Run(async token =>
            {
                IEnumerable<KeyValuePair<string, string>> getData()
                {
                    yield return new KeyValuePair<string, string>("commenttext", content);
                    if (editable != null && editable.Status == CommentStatus.Editable)
                    {
                        yield return new KeyValuePair<string, string>("edit_comment", editable.ID.ToString());
                        yield return new KeyValuePair<string, string>("postcomment", "Edit Comment");
                    }
                    else
                    {
                        yield return new KeyValuePair<string, string>("postcomment", "Post Comment");
                    }
                }
                var request = new HttpFormUrlEncodedContent(getData());
                var requestTask = Client.Current.HttpClient.PostAsync(this.Owner.GalleryUri, request);
                token.Register(requestTask.Cancel);
                var response = await requestTask;
                var responseStr = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(responseStr);
                var cdiv = doc.GetElementbyId("cdiv");
                var pbr = cdiv.Element("p");
                if (pbr != null)
                {
                    var error = pbr.InnerText.DeEntitize().Trim();
                    switch (error)
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
                await fetchAsync(false);
            });
        }
    }
}
