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
            Tag = Tag.Parse(td[1].GetInnerText());
            Score = int.Parse(td[2].GetInnerText());
            var uri = td[3].Element("a").GetAttribute("href", default(Uri));
            GalleryInfo = GalleryInfo.Parse(uri);
            Timestamp = DateTimeOffset.Parse(td[4].GetInnerText(), null, System.Globalization.DateTimeStyles.AssumeUniversal);
        }

        public Tag Tag { get; }

        public int Score { get; }

        public GalleryInfo GalleryInfo { get; }

        public DateTimeOffset Timestamp { get; }

        public static bool operator ==(in TaggingRecord left, in TaggingRecord right) => left.Equals(right);
        public static bool operator !=(in TaggingRecord left, in TaggingRecord right) => !left.Equals(right);

        public bool Equals(TaggingRecord other)
            => Tag == other.Tag
            && GalleryInfo == other.GalleryInfo
            && Timestamp == other.Timestamp
            && Score == other.Score;

        public override bool Equals(object obj) => obj is TaggingRecord other && Equals(other);

        public override int GetHashCode() => Timestamp.GetHashCode() * 17 ^ Tag.GetHashCode();
    }
}