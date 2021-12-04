using System;
using System.Collections.Generic;
using System.Linq;

namespace ExClient.Tagging
{
    [Flags]
    public enum Namespace
    {
        Unknown = 0,

        Reclass = 1,
        Language = 2,
        Other = 512,

        Parody = 4,
        Character = 8,

        Group = 16,
        Artist = 32,
        Cosplayer = 1024,

        Male = 64,
        Female = 128,
        Mixed = 256,

        Temp = 2048,
    }

    public static class NamespaceExtention
    {
        private static readonly Dictionary<string, Namespace> parsingDic
            = new Dictionary<string, Namespace>(StringComparer.OrdinalIgnoreCase)
            {
                ["R"] = Namespace.Reclass,
                ["Reclass"] = Namespace.Reclass,

                ["L"] = Namespace.Language,
                ["Language"] = Namespace.Language,
                ["Lang"] = Namespace.Language,

                ["O"] = Namespace.Other,
                ["Other"] = Namespace.Other,

                ["P"] = Namespace.Parody,
                ["Parody"] = Namespace.Parody,
                ["Series"] = Namespace.Parody,

                ["C"] = Namespace.Character,
                ["Char"] = Namespace.Character,
                ["Character"] = Namespace.Character,

                ["G"] = Namespace.Group,
                ["Group"] = Namespace.Group,
                ["Creator"] = Namespace.Group,
                ["Circle"] = Namespace.Group,

                ["A"] = Namespace.Artist,
                ["Artist"] = Namespace.Artist,

                ["Cos"] = Namespace.Cosplayer,
                ["Coser"] = Namespace.Cosplayer,
                ["Cosplayer"] = Namespace.Cosplayer,

                ["M"] = Namespace.Male,
                ["Male"] = Namespace.Male,

                ["F"] = Namespace.Female,
                ["Female"] = Namespace.Female,

                ["X"] = Namespace.Mixed,
                ["Mixed"] = Namespace.Mixed,
            };

        private static readonly Dictionary<Namespace, string> _SearchDic
            = new()
            {
                [Namespace.Reclass] = "reclass",
                [Namespace.Language] = "language",
                [Namespace.Other] = "other",
                [Namespace.Parody] = "parody",
                [Namespace.Character] = "character",
                [Namespace.Group] = "group",
                [Namespace.Artist] = "artist",
                [Namespace.Cosplayer] = "cosplayer",
                [Namespace.Male] = "male",
                [Namespace.Female] = "female",
                [Namespace.Mixed] = "mixed",
            };

        private static readonly Dictionary<Namespace, string> _SearchDicAbbr
            = new()
            {
                [Namespace.Reclass] = "r",
                [Namespace.Language] = "l",
                [Namespace.Other] = "o",
                [Namespace.Parody] = "p",
                [Namespace.Character] = "c",
                [Namespace.Group] = "g",
                [Namespace.Artist] = "a",
                [Namespace.Cosplayer] = "cos",
                [Namespace.Male] = "m",
                [Namespace.Female] = "f",
                [Namespace.Mixed] = "x",
            };

        public static string ToFriendlyNameString(this Namespace that)
            => that.ToFriendlyNameString(LocalizedStrings.Namespace.GetValue);

        public static string ToSearchString(this Namespace that, bool abbr)
        {
            if ((abbr ? _SearchDicAbbr : _SearchDic).TryGetValue(that, out var r))
                return r;
            return null;
        }

        public static bool IsDefined(this Namespace that)
            => that.IsDefined<Namespace>() && that != Namespace.Unknown;

        public static Namespace Parse(string str)
        {
            if (TryParse(str, out var r))
                return r;
            throw new FormatException(LocalizedStrings.Resources.InvalidNamespace);
        }

        public static bool TryParse(string str, out Namespace result)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                result = Namespace.Temp;
                return true;
            }
            str = str.Trim();
            if (parsingDic.TryGetValue(str, out result))
                return true;
            var f = str.FirstOrDefault(char.IsLetter);
            if (f == default(char))
                return false;
            if (parsingDic.TryGetValue(f.ToString(), out result))
                return true;
            return false;
        }
    }
}
