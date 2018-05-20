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
            Tag = Tag.Parse(td[0].GetInnerText());
            Score = int.Parse(td[1].GetInnerText());
            var uri = td[2].Element("a").GetAttribute("href", default(Uri));
            GalleryInfo = GalleryInfo.Parse(uri);
            Timestamp = DateTimeOffset.Parse(td[3].GetInnerText(), null, System.Globalization.DateTimeStyles.AssumeUniversal);
            UsageCount = long.Parse(td[4].GetInnerText());
            IsBlocked = td[5].GetInnerText() == "B";
            IsSlaved = td[6].GetInnerText() == "S";
        }

        public Tag Tag { get; }

        public int Score { get; }

        public GalleryInfo GalleryInfo { get; }

        public DateTimeOffset Timestamp { get; }

        public long UsageCount { get; }

        public bool IsBlocked { get; }

        public bool IsSlaved { get; }

        public static bool operator ==(in TaggingRecord left, in TaggingRecord right) => left.Equals(right);
        public static bool operator !=(in TaggingRecord left, in TaggingRecord right) => !left.Equals(right);

        public bool Equals(TaggingRecord other)
            => this.Tag == other.Tag
            && this.GalleryInfo == other.GalleryInfo
            && this.Timestamp == other.Timestamp
            && this.Score == other.Score
            && this.UsageCount == other.UsageCount
            && this.IsBlocked == other.IsBlocked
            && this.IsSlaved == other.IsSlaved;

        public override bool Equals(object obj) => obj is TaggingRecord other && this.Equals(other);

        public override int GetHashCode() => Timestamp.GetHashCode() * 17 ^ Tag.GetHashCode();
    }
}