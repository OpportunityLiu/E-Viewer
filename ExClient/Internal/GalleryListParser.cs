using ExClient.Api;
using ExClient.Galleries;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ExClient.Internal
{
    internal static class GalleryListParser
    {
        private enum ListType
        {
            Minimal,
            Compact,
            Extended,
            Thumbnail,
        }

        private static void _HandleAdditionalInfo(HtmlNode dataNode, Gallery gallery)
        {
            var postedId = "posted_" + gallery.Id.ToString();
            var postedTag = dataNode.Descendants("div").FirstOrDefault(d => d.Id == postedId);
            if (postedTag != null)
                gallery.FavoriteCategory = Client.Current.Favorites.GetCategory(postedTag);

            var noteId = "favnote_" + gallery.Id.ToString();
            var noteTag = dataNode.Descendants("div").FirstOrDefault(d => d.Id == noteId);
            if (noteTag != null)
            {
                var str = noteTag.GetInnerText();
                if(str.StartsWith("note:", StringComparison.OrdinalIgnoreCase))
                {
                    str = str.Substring("note:".Length).Trim();
                    gallery.FavoriteNote = str;
                }
            }

            var ratingNode = dataNode.Descendants("div").FirstOrDefault(d => d.HasClass("ir"));
            if(ratingNode!=null)
            gallery.Rating.AnalyzeNode(ratingNode);
        }

        private static readonly Regex _GLinkMatcher = new Regex(@".+?/g/(\d+)/([0-9a-f]+).+?", RegexOptions.Compiled);

        public static async Task<IList<Gallery>> Parse(HtmlDocument doc, CancellationToken token)
        {
            var dataRoot = doc.DocumentNode.Descendants().SingleOrDefault(node => node.HasClass("itg"));
            var gInfoList = new List<GalleryInfo>(dataRoot.ChildNodes.Count);
            var dataNodeList = new List<HtmlNode>(dataRoot.ChildNodes.Count);
            foreach (var node in dataRoot.ChildNodes)
            {
                foreach (var link in node.Descendants("a"))
                {
                    var match = _GLinkMatcher.Match(link.GetAttribute("href", ""));
                    if (!match.Success)
                        continue;
                    dataNodeList.Add(node);
                    gInfoList.Add(new GalleryInfo(long.Parse(match.Groups[1].Value), EToken.Parse(match.Groups[2].Value)));
                    break;
                }
            }
            var getG = Gallery.FetchGalleriesAsync(gInfoList);
            token.Register(getG.Cancel);
            var galleries = await getG;
            token.ThrowIfCancellationRequested();
            for (var i = 0; i < galleries.Count; i++)
            {
                _HandleAdditionalInfo(dataNodeList[i], galleries[i]);
            }
            return galleries;
        }
    }
}
