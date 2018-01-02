using ExClient.Api;
using ExClient.Tagging;
using HtmlAgilityPack;
using System;
using System.Linq;

namespace ExClient.Status
{
    public readonly struct TaggingRecord : IEquatable<TaggingRecord>
    {
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

        public Tag Tag { get; }

        public int Score { get; }

        public GalleryInfo GalleryInfo { get; }

        public DateTimeOffset Timestamp { get; }

        public long UsageCount { get; }

        public bool IsBlocked { get; }

        public bool IsSlaved { get; }

        public static bool operator ==(TaggingRecord left, TaggingRecord right) => left.Equals(right);
        public static bool operator !=(TaggingRecord left, TaggingRecord right) => !left.Equals(right);

        public bool Equals(TaggingRecord other)
        {
            return this.Tag == other.Tag
                && this.GalleryInfo == other.GalleryInfo
                && this.Timestamp == other.Timestamp
                && this.Score == other.Score
                && this.UsageCount == other.UsageCount
                && this.IsBlocked == other.IsBlocked
                && this.IsSlaved == other.IsSlaved;
        }

        public override bool Equals(object obj)
        {
            if (obj is TaggingRecord other)
                return this.Equals(other);
            return false;
        }

        public override int GetHashCode()
        {
            return Timestamp.GetHashCode() * 17 ^ Tag.GetHashCode();
        }
    }
}