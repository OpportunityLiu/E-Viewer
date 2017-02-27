using System;
using System.Text;

namespace ExClient
{
    [Flags]
    public enum Category : uint
    {
        Unspecified = 0,
        Doujinshi = 0x01,
        Manga = 0x02,
        ArtistCG = 0x04,
        GameCG = 0x08,
        Western = 0x10,
        NonH = 0x20,
        ImageSet = 0x40,
        Cosplay = 0x80,
        AsianPorn = 0x100,
        Misc = 0x200,
        All = Doujinshi | Manga | ArtistCG | GameCG | Western | NonH | ImageSet | Cosplay | AsianPorn | Misc
    }

    public static class CategoryExtention
    {
        public static string ToFriendlyNameString(this Category that)
        {
            return that.ToFriendlyNameString(LocalizedStrings.Category);
        }
    }
}