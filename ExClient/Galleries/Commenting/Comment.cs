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
using System.Threading;
using System.Threading.Tasks;

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
                Edited = DateTimeOffset.ParseExact(editNode.Element("strong").InnerText, "dd MMMM yyyy, HH:mm", culture, System.Globalization.DateTimeStyles.AssumeUniversal);
            }

            var postedAndAuthorNode = commentNode.Descendants("div").First(node => node.HasClass("c3"));
            var authorNode = postedAndAuthorNode.Element("a");
            if (authorNode != null)
            {
                Author = authorNode.GetInnerText();
                Posted = DateTimeOffset.ParseExact(postedAndAuthorNode.FirstChild.InnerText, "'Posted on' dd MMMM yyyy, HH:mm 'by: &nbsp;'", culture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AllowWhiteSpaces);
            }
            else
            {
                Author = "(Disowned)";
                Posted = DateTimeOffset.ParseExact(postedAndAuthorNode.FirstChild.InnerText, "'Posted on' dd MMMM yyyy, HH:mm", culture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AllowWhiteSpaces);
            }

            if (!IsUploaderComment)
            {
                _Score = int.Parse(document.GetElementbyId($"comment_score_{id}").InnerText);
                var actionNode = commentNode.Descendants("div").FirstOrDefault(node => node.HasClass("c4") && node.HasClass("nosel"));
                if (actionNode != null)
                {
                    var vuNode = document.GetElementbyId($"comment_vote_up_{id}");
                    var vdNode = document.GetElementbyId($"comment_vote_down_{id}");
                    if (vuNode != null && vdNode != null)
                    {
                        if (vuNode.GetAttribute("style", "").Contains("color:blue"))
                        {
                            _Status = CommentStatus.VotedUp;
                        }
                        else if (vdNode.GetAttribute("style", "").Contains("color:blue"))
                        {
                            _Status = CommentStatus.VotedDown;
                        }
                        else
                        {
                            _Status = CommentStatus.Votable;
                        }
                    }
                    else if (actionNode.InnerText == "[Edit]")
                    {
                        _Status = CommentStatus.Editable;
                    }
                }
            }
        }

        private static readonly HttpClient _TransClient = new HttpClient();

        private static async Task<string> _TranslateAsync(string source, string targetLangCode, CancellationToken token)
        {
            if (Uri.TryCreate(source, UriKind.Absolute, out var urlContent) && (urlContent.Scheme == "http" || urlContent.Scheme == "https"))
            {
                // Do not translate url
                return source;
            }
            var uri = $"https://clients5.google.com/translate_a/t?client=dict-chrome-ex"
                + $"&sl=auto&tl={Uri.EscapeDataString(targetLangCode)}&q={Uri.EscapeDataString(source)}";
            var transTask = _TransClient.GetStringAsync(new(uri));
            var transRetHtml = await transTask.AsTask(token);
            var obj = JsonConvert.DeserializeObject<List<string>[]>(transRetHtml);
            var first = obj[0];
            first.RemoveAt(first.Count - 1);
            return string.Concat(first);
        }

        public IAsyncOperation<HtmlNode> TranslateAsync(string targetLangCode)
        {
            return AsyncInfo.Run(async token =>
            {
                var node = HtmlNode.CreateNode(Content.OuterHtml);
                var textNodes = node.Descendants("#text").ToList();
                var tasks = textNodes.Select(async node =>
                {
                    var data = node.GetInnerText();
                    var translated = await _TranslateAsync(data, targetLangCode, token);
                    node.InnerHtml = HtmlEntity.Entitize(translated);
                });
                await Task.WhenAll(tasks);
                TranslatedContent = node;
                return node;
            });
        }

        public CommentCollection Owner { get; }

        public int Id { get; }

        public string Author { get; }

        public bool IsUploaderComment => Id == 0;

        public DateTimeOffset Posted { get; }

        private DateTimeOffset? _Edited;
        public DateTimeOffset? Edited
        {
            get => _Edited;
            internal set => Set(ref _Edited, value);
        }

        private HtmlNode _Content;
        public HtmlNode Content
        {
            get => _Content;
            internal set => Set(ref _Content, value);
        }

        private HtmlNode _TranslatedContent;
        public HtmlNode TranslatedContent
        {
            get => _TranslatedContent;
            internal set => Set(ref _TranslatedContent, value);
        }

        private int _Score;
        public int Score
        {
            get => _Score;
            internal set => Set(ref _Score, value);
        }

        public bool CanVote => (_Status & CommentStatus.Votable) == CommentStatus.Votable;

        public async Task VoteAsync(VoteState command, CancellationToken token = default)
        {
            if (command == VoteState.Default)
            {
                // Withdraw votes
                if (_Status == CommentStatus.VotedDown)
                    command = VoteState.Down;
                else if (_Status == CommentStatus.VotedUp)
                    command = VoteState.Up;
            }
            if (command != VoteState.Down && command != VoteState.Up)
                throw new ArgumentOutOfRangeException(nameof(command), LocalizedStrings.Resources.VoteOutOfRange);
            if (!CanVote)
            {
                if (IsUploaderComment)
                    throw new InvalidOperationException(LocalizedStrings.Resources.WrongVoteStateUploader);
                else
                    throw new InvalidOperationException(LocalizedStrings.Resources.WrongVoteState);
            }
            var request = new CommentVoteRequest(this, command);
            var r = await request.GetResponseAsync(token);
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
        }

        public bool CanEdit => _Status == CommentStatus.Editable;

        private void _CheckEdit()
        {
            if (!CanEdit)
                throw new InvalidOperationException(LocalizedStrings.Resources.WrongEditState);
        }

        public async Task<string> FetchEditableAsync(CancellationToken token = default)
        {
            _CheckEdit();

            var request = new CommentEditRequest(this);
            var r = await request.GetResponseAsync(token);
            var doc = HtmlNode.CreateNode(r.Editable.Trim());
            var textArea = doc.Descendants("textarea").FirstOrDefault();
            if (textArea is null)
            {
                return "";
            }
            return textArea.GetInnerText();
        }

        public Task EditAsync(string content, CancellationToken token = default)
        {
            _CheckEdit();

            return Owner.PostFormAsync(content, this, token);
        }

        private CommentStatus _Status;
        public CommentStatus Status
        {
            get => _Status;
            internal set => Set(nameof(CanVote), nameof(CanEdit), ref _Status, value);
        }
    }
}
