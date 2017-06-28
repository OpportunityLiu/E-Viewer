using ExClient.Api;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Opportunity.MvvmUniverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Web.Http;

namespace ExClient.Galleries.Commenting
{
    public sealed class Comment : ObservableObject
    {
        internal static IEnumerable<Comment> AnalyzeDocument(CommentCollection owner, HtmlDocument document)
        {
            var commentNodes = document?.GetElementbyId("cdiv")?.ChildNodes;
            if (commentNodes == null)
                yield break;
            for (var i = 0; i < commentNodes.Count; i += 2)
            {
                var headerNode = commentNodes[i];
                var commentNode = commentNodes[i + 1];
                if (headerNode.Name != "a" || commentNode.Name != "div")
                    break;
                var id = int.Parse(headerNode.GetAttributeValue("name", "c0").Substring(1));
                yield return new Comment(owner, id, commentNode);
            }
        }

        private static Regex voteRegex = new Regex(@"^(.+?)\s+([+-]\d+)$", RegexOptions.Compiled | RegexOptions.Singleline);

        private Comment(CommentCollection owner, int id, HtmlNode commentNode)
        {
            this.Owner = owner;
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            var document = commentNode.OwnerDocument;
            this.Id = id;

            var contentHtml = document.GetElementbyId($"comment_{id}").OuterHtml.Replace("://forums.exhentai.org", "://forums.e-hentai.org");
            this.Content = HtmlNode.CreateNode(contentHtml);

            var editNode = commentNode.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", "") == "c8");
            if (editNode != null)
                this.Edited = DateTimeOffset.ParseExact(editNode.Element("strong").InnerText, "dd MMMM yyyy, HH:mm 'UTC'", culture, System.Globalization.DateTimeStyles.AssumeUniversal);
            var postedAndAuthorNode = commentNode.Descendants("div").First(node => node.GetAttributeValue("class", "") == "c3");
            this.Author = postedAndAuthorNode.Element("a").InnerText.DeEntitize();
            this.Posted = DateTimeOffset.ParseExact(postedAndAuthorNode.FirstChild.InnerText, "'Posted on' dd MMMM yyyy, HH:mm 'UTC by: &nbsp;'", culture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AllowWhiteSpaces);

            if (!this.IsUploaderComment)
            {
                this.score = int.Parse(document.GetElementbyId($"comment_score_{id}").InnerText);
                var actionNode = commentNode.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", "") == "c4 nosel");
                if (actionNode != null)
                {
                    var vuNode = document.GetElementbyId($"comment_vote_up_{id}");
                    var vdNode = document.GetElementbyId($"comment_vote_down_{id}");
                    if (vuNode != null && vdNode != null)
                    {
                        if (vuNode.GetAttributeValue("style", "") == "color:blue")
                            this.status = CommentStatus.VotedUp;
                        else if (vdNode.GetAttributeValue("style", "") == "color:blue")
                            this.status = CommentStatus.VotedDown;
                        else
                            this.status = CommentStatus.Votable;
                    }
                    else if (actionNode.InnerText == "[Edit]")
                    {
                        this.status = CommentStatus.Editable;
                    }
                }
            }
        }

        private static HttpClient transClient = new HttpClient();

        public IAsyncOperation<HtmlNode> TranslateAsync(string targetLangCode)
        {
            return AsyncInfo.Run(async token =>
            {
                var node = HtmlNode.CreateNode(this.Content.OuterHtml);
                foreach (var item in node.Descendants("#text"))
                {
                    var data = HtmlEntity.DeEntitize(item.InnerHtml);
                    var uri = $"https://translate.googleapis.com/translate_a/single?client=gtx&dt=t&ie=UTF-8&oe=UTF-8"
                        + $"&sl=auto&tl={targetLangCode}&q={Uri.EscapeDataString(data)}";
                    var transRetHtml = await transClient.GetStringAsync(new Uri(uri));
                    var obj = JsonConvert.DeserializeObject<JArray>(transRetHtml);
                    var objarr = (JArray)obj[0];
                    var translated = string.Concat(objarr.Select(a => a[0].ToString()));
                    item.InnerHtml = HtmlEntity.Entitize(translated);
                }
                this.TranslatedContent = node;
                return node;
            });
        }

        public CommentCollection Owner { get; }

        public int Id { get; }

        public string Author { get; }

        public bool IsUploaderComment => this.Id == 0;

        public DateTimeOffset Posted { get; }

        private DateTimeOffset? edited;
        public DateTimeOffset? Edited
        {
            get => this.edited;
            internal set => Set(ref this.edited, value);
        }

        private HtmlNode content;
        public HtmlNode Content
        {
            get => this.content;
            internal set => Set(ref this.content, value);
        }

        private HtmlNode translatedContent;
        public HtmlNode TranslatedContent
        {
            get => this.translatedContent;
            internal set => Set(ref this.translatedContent, value);
        }

        private int score;

        public int Score
        {
            get => score;
            internal set => Set(ref this.score, value);
        }

        public bool CanVote => this.status == CommentStatus.Votable
            || this.status == CommentStatus.VotedUp
            || this.status == CommentStatus.VotedDown;

        public IAsyncAction VoteAsync(VoteState command)
        {
            if (command != VoteState.Down && command != VoteState.Up)
                throw new ArgumentOutOfRangeException(nameof(command), LocalizedStrings.Resources.VoteOutOfRange);
            if (!this.CanVote)
                if (this.IsUploaderComment)
                    throw new InvalidOperationException(LocalizedStrings.Resources.WrongVoteStateUploader);
                else
                    throw new InvalidOperationException(LocalizedStrings.Resources.WrongVoteState);
            var request = new CommentVote(this, command);
            return AsyncInfo.Run(async token =>
            {
                var res = await Client.Current.HttpClient.PostApiAsync(request);
                var r = JsonConvert.DeserializeObject<CommentVoteResponse>(res);
                r.CheckResponse();
                if (this.Id != r.Id)
                    throw new InvalidOperationException(LocalizedStrings.Resources.WrongVoteResponse);
                switch (r.Vote)
                {
                case VoteState.Default:
                    this.Status = CommentStatus.Votable;
                    break;
                case VoteState.Up:
                    this.Status = CommentStatus.VotedUp;
                    break;
                case VoteState.Down:
                    this.Status = CommentStatus.VotedDown;
                    break;
                }
                this.Score = r.Score;
            });
        }

        public bool CanEdit => this.status == CommentStatus.Editable;

        public IAsyncOperation<string> FetchEditableAsync()
        {
            if (!this.CanEdit)
                throw new InvalidOperationException(LocalizedStrings.Resources.WrongEditState);
            var request = new CommentEdit(this);
            return AsyncInfo.Run(async token =>
            {
                var res = await Client.Current.HttpClient.PostApiAsync(request);
                var r = JsonConvert.DeserializeObject<CommentEditResponse>(res);
                r.CheckResponse();
                var doc = HtmlNode.CreateNode(r.Editable.Trim());
                var textArea = doc.Descendants("textarea").FirstOrDefault();
                if (textArea == null)
                    return "";
                return HtmlEntity.DeEntitize(textArea.InnerText);
            });
        }

        public IAsyncAction EditAsync(string content)
        {
            if (!this.CanEdit)
                throw new InvalidOperationException(LocalizedStrings.Resources.WrongEditState);
            return this.Owner.PostFormAsync(content, this);
        }

        private CommentStatus status;

        public CommentStatus Status
        {
            get => status;
            internal set => Set(nameof(CanVote), nameof(CanEdit), ref this.status, value);
        }
    }
}
