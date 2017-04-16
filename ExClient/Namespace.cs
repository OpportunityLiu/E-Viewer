using System;
using System.Collections.Generic;
using System.Linq;

namespace ExClient
{
    [Flags]
    public enum Namespace
    {
        Unknown = 0,

        Reclass = 1,
        Language = 2,
        Parody = 4,
        Character = 8,
        Group = 16,
        Artist = 32,
        Male = 64,
        Female = 128,
        Misc = 256
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
                ["M"] = Namespace.Male,
                ["Male"] = Namespace.Male,
                ["F"] = Namespace.Female,
                ["Female"] = Namespace.Female
            };

        public static string ToFriendlyNameString(this Namespace that)
            => that.ToFriendlyNameString(LocalizedStrings.Namespace.GetValue);

        public static bool IsDefined(this Namespace that)
            => that.IsDefined<Namespace>() && that != Namespace.Unknown;

        public static Namespace Parse(string str)
        {
            if(TryParse(str, out var r))
                return r;
            throw new FormatException("Wrong namespace.");
        }

        public static bool TryParse(string str, out Namespace result)
        {
            if(string.IsNullOrWhiteSpace(str))
            {
                result = Namespace.Misc;
                return true;
            }
            str = str.Trim();
            if(parsingDic.TryGetValue(str, out result))
                return true;
            var f = str.FirstOrDefault(char.IsLetter);
            if(f == default(char))
                return false;
            if(parsingDic.TryGetValue(f.ToString(), out result))
                return true;
            return false;
        }
    }
}
