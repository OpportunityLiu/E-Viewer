using ExClient.Api;
using ExClient.Tagging;
using HtmlAgilityPack;
using System;
using System.Linq;

namespace ExClient.Status
{
    public readonly struct TaggingRecord
    {
        public Tag Tag { get; }

        public int Score { get; }

        public GalleryInfo GalleryInfo { get; }

        public DateTimeOffset Timestamp { get; }

        public long UsageCount { get; }

        public bool IsBlocked { get; }

        public bool IsSlaved { get; }

        internal TaggingRecord(HtmlNode trNode)
        {
            var td = trNode.Elements("td").ToList();
            Tag = Tag.Parse(td[0].InnerText.DeEntitize());
            Score = int.Parse(td[1].InnerText.DeEntitize());
            var uri = new Uri(td[2].Element("a").GetAttributeValue("href", "").DeEntitize());
            GalleryInfo = GalleryInfo.Parse(uri);
            Timestamp = DateTimeOffset.Parse(td[3].InnerText.DeEntitize(), null, System.Globalization.DateTimeStyles.AssumeUniversal);
            UsageCount = long.Parse(td[4].InnerText.DeEntitize());
            IsBlocked = td[5].InnerText.DeEntitize() == "B";
            IsSlaved = td[6].InnerText.DeEntitize() == "S";
        }
    }
}