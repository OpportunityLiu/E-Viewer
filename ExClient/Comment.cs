using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ExClient
{
    public sealed class Comment : ObservableObject
    {
        internal static IEnumerable<Comment> AnalyzeDocument(CommentCollection owner, HtmlDocument document)
        {
            var commentNodes = document.GetElementbyId("cdiv").ChildNodes;
            for(var i = 0; i < commentNodes.Count; i += 2)
            {
                var headerNode = commentNodes[i];
                var commentNode = commentNodes[i + 1];
                if(headerNode.Name != "a" || commentNode.Name != "div")
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
            if(editNode != null)
                this.Edited = DateTimeOffset.ParseExact(editNode.Element("strong").InnerText, "dd MMMM yyyy, HH:mm 'UTC'", culture, System.Globalization.DateTimeStyles.AssumeUniversal);
            var postedAndAuthorNode = commentNode.Descendants("div").First(node => node.GetAttributeValue("class", "") == "c3");
            this.Author = postedAndAuthorNode.LastChild.InnerText.DeEntitize();
            this.Posted = DateTimeOffset.ParseExact(postedAndAuthorNode.FirstChild.InnerText, "'Posted on' dd MMMM yyyy, HH:mm 'UTC by: &nbsp;'", culture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AllowWhiteSpaces);

            if(!this.IsUploaderComment)
            {
                this.score = int.Parse(document.GetElementbyId($"comment_score_{id}").InnerText);
                var actionNode = commentNode.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", "") == "c4 nosel");
                if(actionNode != null)
                {
                    var vuNode = document.GetElementbyId($"comment_vote_up_{id}");
                    var vdNode = document.GetElementbyId($"comment_vote_down_{id}");
                    if(vuNode != null && vdNode != null)
                    {
                        if(vuNode.GetAttributeValue("style", "") == "color:blue")
                            this.status = CommentStatus.VotedUp;
                        else if(vdNode.GetAttributeValue("style", "") == "color:blue")
                            this.status = CommentStatus.VotedDown;
                        else
                            this.status = CommentStatus.Votable;
                    }
                    else if(actionNode.InnerText == "[Edit]")
                    {
                        this.status = CommentStatus.Editable;
                    }
                }
            }
        }

        public CommentCollection Owner { get; }

        public int Id { get; }

        public string Author { get; }

        public bool IsUploaderComment => this.Id == 0;

        public DateTimeOffset Posted { get; }

        public DateTimeOffset? Edited { get; }

        public HtmlNode Content { get; }

        private int score;

        public int Score
        {
            get => score;
            set => Set(ref this.score, value);
        }

        public bool CanVote => this.status == CommentStatus.Votable
            || this.status == CommentStatus.VotedUp
            || this.status == CommentStatus.VotedDown;

        public IAsyncAction VoteAsync(Api.VoteCommand command)
        {
            if(!this.CanVote)
                throw new InvalidOperationException(LocalizedStrings.Resources.WrongVoteState);
            var request = new Api.CommentVote(this, command);
            return AsyncInfo.Run(async token =>
            {
                var res = await Client.Current.HttpClient.PostApiAsync(request);
                var r = JsonConvert.DeserializeObject<CommentResult>(res);
                if(this.Id != r.comment_id)
                    throw new InvalidOperationException(LocalizedStrings.Resources.WrongVoteResponse);
                switch(r.comment_vote)
                {
                case Api.VoteCommand.Default:
                    this.Status = CommentStatus.Votable;
                    break;
                case Api.VoteCommand.Up:
                    this.Status = CommentStatus.VotedUp;
                    break;
                case Api.VoteCommand.Down:
                    this.Status = CommentStatus.VotedDown;
                    break;
                }
                this.Score = r.comment_score;
            });
        }

        private CommentStatus status;

        public CommentStatus Status
        {
            get => status;
            set => Set(nameof(CanVote), ref this.status, value);
        }

        private class CommentResult
        {
#pragma warning disable IDE1006 // 命名样式
#pragma warning disable CS0649
            public int comment_id;
            public int comment_score;
            public Api.VoteCommand comment_vote;
#pragma warning restore CS0649
#pragma warning restore IDE1006 // 命名样式
        }
    }

    public enum CommentStatus
    {
        None,
        Votable,
        VotedUp,
        VotedDown,
        Editable
    }
}
