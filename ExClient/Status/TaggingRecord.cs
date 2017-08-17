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
            Tag = Tag.Parse(td[0].InnerText.DeEntitize());
            Score = int.Parse(td[1].InnerText.DeEntitize());
            GalleryID = long.Parse(td[2].InnerText.DeEntitize());
            Timestamp = DateTimeOffset.Parse(td[3].InnerText.DeEntitize(), null, System.Globalization.DateTimeStyles.AssumeUniversal);
            UsageCount = long.Parse(td[4].InnerText.DeEntitize());
            IsBlocked = td[5].InnerText.DeEntitize() == "B";
            IsSlaved = td[6].InnerText.DeEntitize() == "S";
        }
    }
}