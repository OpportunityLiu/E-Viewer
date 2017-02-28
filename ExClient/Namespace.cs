using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private static HashSet<Namespace> definedNamespaces = new HashSet<Namespace>
        {
            Namespace.Reclass,
            Namespace.Language,
            Namespace.Parody,
            Namespace.Character,
            Namespace.Group,
            Namespace.Artist,
            Namespace.Male,
            Namespace.Female,
            Namespace.Misc
        };

        public static string ToFriendlyNameString(this Namespace that)
            => EnumExtension.ToFriendlyNameString(that, LocalizedStrings.Namespace);

        public static bool IsValid(this Namespace that)
            => definedNamespaces.Contains(that);
    }
}
