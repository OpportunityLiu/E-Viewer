using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace ExClient
{
    public class Comment
    {
        internal static IAsyncOperation<List<Comment>> LoadCommentsAsync(Gallery gallery)
        {
            return Task.Run(async () =>
            {
                var html = await gallery.Owner.HttpClient.GetStringAsync(new Uri(gallery.GalleryUri, "?hc=1"));
                var document = new HtmlDocument();
                document.LoadHtml(html);
                var commentNodes = document.GetElementbyId("cdiv").ChildNodes;
                var comments = new List<Comment>();
                for(int i = 0; i < commentNodes.Count; i += 2)
                {
                    if(commentNodes[i].Name != "a" || commentNodes[i + 1].Name != "div")
                        break;
                    var id = int.Parse(commentNodes[i].GetAttributeValue("name", "c0").Substring(1));
                    var commentNode = commentNodes[i + 1];
                    var content = getContent(document.GetElementbyId($"comment_{id}").InnerHtml);
                    var editNode = commentNode.ChildNodes.FirstOrDefault(node => node.GetAttributeValue("class", "") == "c8");
                    var edit = editNode != null ? DateTimeOffset.ParseExact(editNode.Element("strong").InnerText, "dd MMMM yyyy, HH:mm 'UTC'", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal) : (DateTimeOffset?)null;
                    var postedAndAuthorNode = commentNode.Descendants("div").First(node => node.GetAttributeValue("class", "") == "c3");
                    var author = HtmlEntity.DeEntitize(postedAndAuthorNode.LastChild.InnerText);
                    var posted = DateTimeOffset.ParseExact(postedAndAuthorNode.FirstChild.InnerText, "'Posted on' dd MMMM yyyy, HH:mm 'UTC by: &nbsp;'", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AllowWhiteSpaces);
                    var score = id == 0 ? //Uploader Comment
                        0 : int.Parse(document.GetElementbyId($"comment_score_{id}").InnerText);
                    comments.Add(new Comment()
                    {
                        Id = id,
                        Score = score,
                        Content = content,
                        Edited = edit,
                        Author = author,
                        Posted = posted
                    });
                }
                return comments;
            }).AsAsyncOperation();
        }

        private static string getContent(string innerHtml)
        {
            var sb = new StringBuilder(innerHtml);
            sb.Replace("<br>", Environment.NewLine);
            sb.Replace("<strong>", "[b]");
            sb.Replace("</strong>", "[/b]");
            sb.Replace("<em>", "[i]");
            sb.Replace("</em>", "[/i]");
            sb.Replace("<span style=\"text-decoration:underline;\">", "[u]");
            sb.Replace("<span>", "[/u]");
            sb.Replace("<del>", "[s]");
            sb.Replace("</del>", "[/s]");
            sb.Replace("<a href=\"", "[url=");
            sb.Replace("\">", "]");
            sb.Replace("</a>", "[/url]");
            return HtmlEntity.DeEntitize(sb.ToString());
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

        public string Content
        {
            get;
            private set;
        }

        public int Score
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
