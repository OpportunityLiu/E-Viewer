using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ExClient
{
    public sealed class Comment
    {
        internal static ReadOnlyCollection<Comment> LoadComment(HtmlDocument document)
        {
            var commentNodes = document.GetElementbyId("cdiv").ChildNodes;
            var comments = new List<Comment>();
            for(var i = 0; i < commentNodes.Count; i += 2)
            {
                var headerNode = commentNodes[i];
                var commentNode = commentNodes[i + 1];
                if(headerNode.Name != "a" || commentNode.Name != "div")
                    break;
                var id = int.Parse(headerNode.GetAttributeValue("name", "c0").Substring(1));
                comments.Add(new Comment(id, commentNode));
            }
            return comments.AsReadOnly();
        }

        private static Regex voteRegex = new Regex(@"^(.+?)\s+([+-]\d+)$", RegexOptions.Compiled | RegexOptions.Singleline);

        internal static IAsyncOperation<ReadOnlyCollection<Comment>> LoadCommentsAsync(Gallery gallery)
        {
            return Task.Run(async () =>
            {
                var html = await gallery.Owner.HttpClient.GetStringAsync(new Uri(gallery.GalleryUri, "?hc=1"));
                var document = new HtmlDocument();
                document.LoadHtml(html);
                return LoadComment(document);
            }).AsAsyncOperation();
        }

        private Comment(int id, HtmlNode commentNode)
        {
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
                this.Score = int.Parse(document.GetElementbyId($"comment_score_{id}").InnerText);

                var recordList = new List<KeyValuePair<string, int>>();
                var recordsNode = document.GetElementbyId($"cvotes_{id}");
                var voteBase = recordsNode.FirstChild.InnerText;
                voteBase = voteBase.Substring(5, voteBase.Length - (voteBase.EndsWith(" ") ? 7 : 5));
                recordList.Add(new KeyValuePair<string, int>(null, int.Parse(voteBase)));
                foreach(var item in recordsNode.Descendants("span"))
                {
                    var vote = item.InnerText.DeEntitize();
                    var m = voteRegex.Match(vote);
                    if(m.Success == false)
                        throw new Exception();
                    recordList.Add(new KeyValuePair<string, int>(m.Groups[1].Value, int.Parse(m.Groups[2].Value)));
                }
                this.VoteRecords = recordList.AsReadOnly();
            }
        }

        public int Id { get; }

        public string Author { get; }

        public bool IsUploaderComment => this.Id == 0;

        public DateTimeOffset Posted { get; }

        public DateTimeOffset? Edited { get; }

        public HtmlNode Content { get; }

        public int Score { get; }

        public ReadOnlyCollection<KeyValuePair<string, int>> VoteRecords { get; }
    }
}
