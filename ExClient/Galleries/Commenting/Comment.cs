using ExClient.Api;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Opportunity.MvvmUniverse;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Web.Http;

namespace ExClient.Galleries.Commenting
{
    [DebuggerDisplay(@"[{Author,nq}: {Content.InnerText,nq}]")]
    public sealed class Comment : ObservableObject
    {
        internal static IEnumerable<Comment> AnalyzeDocument(CommentCollection owner, HtmlDocument document)
        {
            var commentNodes = document?.GetElementbyId("cdiv")?.ChildNodes;
            if (commentNodes is null)
            {
                yield break;
            }
            for (var i = 0; i < commentNodes.Count; i += 2)
            {
                var headerNode = commentNodes[i];
                var commentNode = commentNodes[i + 1];
                if (headerNode.Name != "a" || commentNode.Name != "div")
                {
                    break;
                }
                var id = int.Parse(headerNode.GetAttribute("name", "c0").Substring(1));
                yield return new Comment(owner, id, commentNode);
            }
        }

        private static readonly Regex voteRegex = new Regex(@"^(.+?)\s+([+-]\d+)$", RegexOptions.Compiled | RegexOptions.Singleline);

        private Comment(CommentCollection owner, int id, HtmlNode commentNode)
        {
            Owner = owner;
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            var document = commentNode.OwnerDocument;
            Id = id;

            var contentHtml = document.GetElementbyId($"comment_{id}").OuterHtml.Replace("://forums.exhentai.org", "://forums.e-hentai.org");
            Content = HtmlNode.CreateNode(contentHtml);

            var editNode = commentNode.Descendants("div").FirstOrDefault(node => node.HasClass("c8"));
            if (editNode != null)
            {
                Edited = DateTimeOffset.ParseExact(editNode.Element("strong").InnerText, "dd MMMM yyyy, HH:mm 'UTC'", culture, System.Globalization.DateTimeStyles.AssumeUniversal);
            }

            var postedAndAuthorNode = commentNode.Descendants("div").First(node => node.HasClass("c3"));
            Author = postedAndAuthorNode.Element("a").GetInnerText();
            Posted = DateTimeOffset.ParseExact(postedAndAuthorNode.FirstChild.InnerText, "'Posted on' dd MMMM yyyy, HH:mm 'UTC by: &nbsp;'", culture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AllowWhiteSpaces);

            if (!IsUploaderComment)
            {
                score = int.Parse(document.GetElementbyId($"comment_score_{id}").InnerText);
                var actionNode = commentNode.Descendants("div").FirstOrDefault(node => node.HasClass("c4") && node.HasClass("nosel"));
                if (actionNode != null)
                {
                    var vuNode = document.GetElementbyId($"comment_vote_up_{id}");
                    var vdNode = document.GetElementbyId($"comment_vote_down_{id}");
                    if (vuNode != null && vdNode != null)
                    {
                        if (vuNode.GetAttribute("style", "").Contains("color:blue"))
                        {
                            status = CommentStatus.VotedUp;
                        }
                        else if (vdNode.GetAttribute("style", "").Contains("color:blue"))
                        {
                            status = CommentStatus.VotedDown;
                        }
                        else
                        {
                            status = CommentStatus.Votable;
                        }
                    }
                    else if (actionNode.InnerText == "[Edit]")
                    {
                        status = CommentStatus.Editable;
                    }
                }
            }
        }

        private static HttpClient transClient = new HttpClient();

        public IAsyncOperation<HtmlNode> TranslateAsync(string targetLangCode)
        {
            return AsyncInfo.Run(async token =>
            {
                var node = HtmlNode.CreateNode(Content.OuterHtml);
                foreach (var item in node.Descendants("#text"))
                {
                    var data = item.GetInnerText();
                    var uri = $"https://translate.google.cn/translate_a/single?client=gtx&dt=t&ie=UTF-8&oe=UTF-8"
                        + $"&sl=auto&tl={targetLangCode}&q={Uri.EscapeDataString(data)}";
                    var transRetHtml = await transClient.GetStringAsync(new Uri(uri));
                    var obj = JsonConvert.DeserializeObject<JArray>(transRetHtml);
                    var objarr = (JArray)obj[0];
                    var translated = string.Concat(objarr.Select(a => a[0].ToString()));
                    item.InnerHtml = HtmlEntity.Entitize(translated);
                }
                TranslatedContent = node;
                return node;
            });
        }

        public CommentCollection Owner { get; }

        public int Id { get; }

        public string Author { get; }

        public bool IsUploaderComment => Id == 0;

        public DateTimeOffset Posted { get; }

        private DateTimeOffset? edited;
        public DateTimeOffset? Edited
        {
            get => edited;
            internal set => Set(ref edited, value);
        }

        private HtmlNode content;
        public HtmlNode Content
        {
            get => content;
            internal set => Set(ref content, value);
        }

        private HtmlNode translatedContent;
        public HtmlNode TranslatedContent
        {
            get => translatedContent;
            internal set => Set(ref translatedContent, value);
        }

        private int score;
        public int Score
        {
            get => score;
            internal set => Set(ref score, value);
        }

        public bool CanVote => (status & CommentStatus.Votable) == CommentStatus.Votable;

        public IAsyncAction VoteAsync(VoteState command)
        {
            if (command == VoteState.Default)
            {
                // Withdraw votes
                if (status == CommentStatus.VotedDown)
                {
                    command = VoteState.Down;
                }
                else if (status == CommentStatus.VotedUp)
                {
                    command = VoteState.Up;
                }
            }
            if (command != VoteState.Down && command != VoteState.Up)
            {
                throw new ArgumentOutOfRangeException(nameof(command), LocalizedStrings.Resources.VoteOutOfRange);
            }
            if (!CanVote)
            {
                if (IsUploaderComment)
                {
                    throw new InvalidOperationException(LocalizedStrings.Resources.WrongVoteStateUploader);
                }
                else
                {
                    throw new InvalidOperationException(LocalizedStrings.Resources.WrongVoteState);
                }
            }
            var request = new CommentVoteRequest(this, command);
            return AsyncInfo.Run(async token =>
            {
                var r = await request.GetResponseAsync();
                switch (r.Vote)
                {
                case VoteState.Default:
                    Status = CommentStatus.Votable;
                    break;
                case VoteState.Up:
                    Status = CommentStatus.VotedUp;
                    break;
                case VoteState.Down:
                    Status = CommentStatus.VotedDown;
                    break;
                default:
                    Debug.Assert(false);
                    break;
                }
                Score = r.Score;
            });
        }

        public bool CanEdit => status == CommentStatus.Editable;

        public IAsyncOperation<string> FetchEditableAsync()
        {
            if (!CanEdit)
            {
                throw new InvalidOperationException(LocalizedStrings.Resources.WrongEditState);
            }
            var request = new CommentEditRequest(this);
            return AsyncInfo.Run(async token =>
            {
                var r = await request.GetResponseAsync();
                var doc = HtmlNode.CreateNode(r.Editable.Trim());
                var textArea = doc.Descendants("textarea").FirstOrDefault();
                if (textArea is null)
                {
                    return "";
                }
                return textArea.GetInnerText();
            });
        }

        public IAsyncAction EditAsync(string content)
        {
            if (!CanEdit)
            {
                throw new InvalidOperationException(LocalizedStrings.Resources.WrongEditState);
            }
            return Owner.PostFormAsync(content, this);
        }

        private CommentStatus status;
        public CommentStatus Status
        {
            get => status;
            internal set => Set(nameof(CanVote), nameof(CanEdit), ref status, value);
        }
    }
}
