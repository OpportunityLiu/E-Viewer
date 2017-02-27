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
        public static string ToFriendlyNameString(this Namespace that)
        {
            return that.ToFriendlyNameString(LocalizedStrings.Namespace);
        }

        public static bool IsValid(this Namespace that)
        {
            return that != Namespace.Unknown && Enum.IsDefined(typeof(Namespace), that);
        }
    }
}
