using System;

namespace ExClient.Status
{
    public struct ToplistItem : IEquatable<ToplistItem>
    {
        internal ToplistItem(int rank, ToplistName name)
        {
            this.Rank = rank;
            this.Name = name;
        }

        public int Rank { get; }
        public ToplistName Name { get; }

        public bool Equals(ToplistItem other)
        {
            return this.Name == other.Name && this.Rank == other.Rank;
        }

        public override bool Equals(object obj)
        {
            if (obj is ToplistItem t)
                return this.Equals(t);
            return false;
        }

        public override int GetHashCode()
        {
            return Rank << 16 ^ (int)Name;
        }
    }

    public enum ToplistName
    {
        GalleriesAllTime = 11,
        GalleriesPastYear = 12,
        GalleriesPastMonth = 13,
        GalleriesYesterday = 15,

        UploaderAllTime = 21,
        UploaderPastYear = 22,
        UploaderPastMonth = 23,
        UploaderYesterday = 25,

        TaggingAllTime = 31,
        TaggingPastYear = 32,
        TaggingPastMonth = 33,
        TaggingYesterday = 35,

        HentaiAtHomeAllTime = 41,
        HentaiAtHomePastYear = 42,
        HentaiAtHomePastMonth = 43,
        HentaiAtHomeYesterday = 45,

        EHTrackerAllTime = 51,
        EHTrackerPastYear = 52,
        EHTrackerPastMonth = 53,
        EHTrackerYesterday = 55,

        CleanupAllTime = 61,
        CleanupPastYear = 62,
        CleanupPastMonth = 63,
        CleanupYesterday = 65,

        RatingAndReviewingAllTime = 71,
        RatingAndReviewingPastYear = 72,
        RatingAndReviewingPastMonth = 73,
        RatingAndReviewingYesterday = 75,
    }

    public static class ToplistNameExtension
    {
        public static Uri Uri(this ToplistName topList)
        {
            if (!topList.IsDefined())
                return null;
            return new Uri(Internal.UriProvider.Eh.RootUri, $"toplist.php?tl={(int)topList}");
        }

        public static string ToDisplayString(this ToplistName topList)
        {
            return topList.ToFriendlyNameString(LocalizedStrings.Toplist.GetValue);
        }
    }
}
