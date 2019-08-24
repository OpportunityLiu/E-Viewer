using HtmlAgilityPack;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Web.Http;

namespace ExClient.Galleries.Commenting
{
    public sealed class CommentCollection : ObservableList<Comment>
    {
        public CommentCollection(Gallery owner)
        {
            Owner = owner;
        }

        public Gallery Owner { get; }

        private bool _IsLoaded;
        public bool IsLoaded
        {
            get => _IsLoaded; private set => Set(nameof(IsEmpty), ref _IsLoaded, value);
        }

        public bool IsEmpty => Count == 0 && IsLoaded;

        private readonly object _Syncroot = new object();

        public Task FetchAsync(CancellationToken token = default) => FetchAsync(true, token);

        internal async Task FetchAsync(bool reload, CancellationToken token = default)
        {
            if (reload)
            {
                Clear();
                IsLoaded = false;
            }
            var get = Client.Current.HttpClient.GetDocumentAsync(new Uri(Owner.GalleryUri, "?hc=1"));
            token.Register(get.Cancel);
            var document = await get;
            token.ThrowIfCancellationRequested();
            Owner.RefreshMetaData(document);
            AnalyzeDocument(document);
        }

        internal void AnalyzeDocument(HtmlDocument doc)
        {
            lock (_Syncroot)
            {
                var newValues = Comment.AnalyzeDocument(this, doc).ToList();
                Update(newValues, (x, y) => x.Id - y.Id, (o, n) =>
                {
                    o.Score = n.Score;
                    o.Status = n.Status;
                    o.Edited = n.Edited;
                    o.Content = n.Content;
                });
                IsLoaded = true;
            }
        }

        public Task PostCommentAsync(string content, CancellationToken token = default) => PostFormAsync(content, null, token);

        private static readonly Encoding _Encoding = Encoding.UTF8;

        internal async Task PostFormAsync(string content, Comment editable, CancellationToken token = default)
        {
            content = (content ?? "").Trim();
            content = content.Replace("\r\n", "\n").Replace('\r', '\n');
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException(LocalizedStrings.Resources.EmptyComment);

            if (content.Length < 10 && _Encoding.GetByteCount(content) < 10)
                throw new ArgumentException(LocalizedStrings.Resources.ShortComment);

            IEnumerable<KeyValuePair<string, string>> getData()
            {
                if (editable != null && editable.Status == CommentStatus.Editable)
                {
                    yield return new KeyValuePair<string, string>("edit_comment", editable.Id.ToString());
                    yield return new KeyValuePair<string, string>("commenttext_edit", content);
                }
                else
                {
                    yield return new KeyValuePair<string, string>("commenttext_new", content);
                }
            }
            var requestTask = Client.Current.HttpClient.PostAsync(Owner.GalleryUri, getData());
            token.Register(requestTask.Cancel);
            var response = await requestTask;
            var responseStr = await response.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(responseStr);
            var cdiv = doc.GetElementbyId("cdiv");
            var pbr = cdiv.Element("p");
            if (pbr != null)
            {
                var error = pbr.GetInnerText().Trim();
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
                case "You have been banned from posting comments.":
                    error = LocalizedStrings.Resources.BannedPostingComments;
                    break;
                default:
                    break;
                }
                throw new InvalidOperationException(error);
            }
            await FetchAsync(false, token);
        }
    }
}
