using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace ExClient
{
    public sealed class Comment
    {
        internal static ReadOnlyCollection<Comment> LoadComment(HtmlDocument document)
        {
            var commentNodes = document.GetElementbyId("cdiv").ChildNodes;
            var comments = new List<Comment>();
            for(int i = 0; i < commentNodes.Count; i += 2)
            {
                var culture = System.Globalization.CultureInfo.InvariantCulture;
                if(commentNodes[i].Name != "a" || commentNodes[i + 1].Name != "div")
                    break;
                var id = int.Parse(commentNodes[i].GetAttributeValue("name", "c0").Substring(1));
                var commentNode = commentNodes[i + 1];
                var content = HtmlNode.CreateNode(document.GetElementbyId($"comment_{id}").OuterHtml);
                var editNode = commentNode.ChildNodes.FirstOrDefault(node => node.GetAttributeValue("class", "") == "c8");
                var edit = editNode != null ? DateTimeOffset.ParseExact(editNode.Element("strong").InnerText, "dd MMMM yyyy, HH:mm 'UTC'", culture, System.Globalization.DateTimeStyles.AssumeUniversal) : (DateTimeOffset?)null;
                var postedAndAuthorNode = commentNode.Descendants("div").First(node => node.GetAttributeValue("class", "") == "c3");
                var author = postedAndAuthorNode.LastChild.InnerText.DeEntitize();
                var posted = DateTimeOffset.ParseExact(postedAndAuthorNode.FirstChild.InnerText, "'Posted on' dd MMMM yyyy, HH:mm 'UTC by: &nbsp;'", culture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AllowWhiteSpaces);
                var score = id == 0 ? //Uploader Comment
                    0 : int.Parse(document.GetElementbyId($"comment_score_{id}").InnerText);

                List<KeyValuePair<string, int>> recordList = null;
                if(id != 0)
                {
                    recordList = new List<KeyValuePair<string, int>>();
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
                }
                comments.Add(new Comment()
                {
                    Id = id,
                    Score = score,
                    Content = content,
                    Edited = edit,
                    Author = author,
                    Posted = posted,
                    VoteRecords = recordList?.AsReadOnly()
                });
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

        private Comment()
        {
        }

        public bool IsUploaderComment => Id == 0;

        public string Author
        {
            get;
            private set;
        }

        public DateTimeOffset Posted
        {
            get;
            private set;
        }

        public DateTimeOffset? Edited
        {
            get;
            private set;
        }

        public HtmlNode Content
        {
            get;
            private set;
        }

        public int Score
        {
            get;
            private set;
        }

        public IReadOnlyList<KeyValuePair<string, int>> VoteRecords
        {
            get;
            private set;
        }

        public int Id
        {
            get;
            private set;
        }
    }
}
