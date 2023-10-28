using ExClient.Api;
using ExClient.Tagging;

using HtmlAgilityPack;

using System;
using System.Collections.Generic;
using System.Linq;

namespace ExClient.Status
{
    public readonly struct TaggingRecord : IEquatable<TaggingRecord>
    {
        internal TaggingRecord(GalleryInfo gallery, List<HtmlNode> tdNodes)
        {
            GalleryInfo = gallery; 
            Timestamp = DateTimeOffset.Parse(tdNodes[0].GetInnerText(), null, System.Globalization.DateTimeStyles.AssumeUniversal);
            Score = int.Parse(tdNodes[1].GetInnerText());
            Tag = Tag.Parse(tdNodes[2].GetInnerText()); 
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