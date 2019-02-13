using System;

namespace ExClient
{
    [Flags]
    public enum Category
    {
        Unspecified = 0,
        Doujinshi = 2,
        Manga = 4,
        ArtistCG = 8,
        GameCG = 16,
        Western = 512,
        NonH = 256,
        ImageSet = 32,
        Cosplay = 64,
        AsianPorn = 128,
        Misc = 1,
        All = Doujinshi | Manga | ArtistCG | GameCG | Western | NonH | ImageSet | Cosplay | AsianPorn | Misc,
    }

    public static class CategoryExtention
    {
        public static string ToFriendlyNameString(this Category that)
        {
            return that.ToFriendlyNameString(LocalizedStrings.Category.GetValue);
        }
    }
}