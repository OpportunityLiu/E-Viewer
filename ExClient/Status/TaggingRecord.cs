using ExClient.Tagging;
using HtmlAgilityPack;
using System;
using System.Linq;

namespace ExClient.Status
{
    public struct TaggingRecord
    {
        public Tag Tag { get; internal set; }

        public int Score { get; internal set; }

        public long GalleryID { get; internal set; }

        public DateTimeOffset Timestamp { get; internal set; }

        public long UsageCount { get; internal set; }

        public bool IsBlocked { get; internal set; }

        public bool IsSlaved { get; internal set; }

        internal TaggingRecord(HtmlNode trNode)
        {
            var td = trNode.Elements("td").ToList();
            Tag = Tag.Parse(HtmlEntity.DeEntitize(td[0].InnerText));
            Score = int.Parse(HtmlEntity.DeEntitize(td[1].InnerText));
            GalleryID = long.Parse(HtmlEntity.DeEntitize(td[2].InnerText));
            Timestamp = DateTimeOffset.Parse(HtmlEntity.DeEntitize(td[3].InnerText), null, System.Globalization.DateTimeStyles.AssumeUniversal);
            UsageCount = long.Parse(HtmlEntity.DeEntitize(td[4].InnerText));
            IsBlocked = HtmlEntity.DeEntitize(td[5].InnerText) == "B";
            IsSlaved = HtmlEntity.DeEntitize(td[6].InnerText) == "S";
        }
    }
}