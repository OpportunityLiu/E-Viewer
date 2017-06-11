using System;
using System.Collections.Generic;
using System.Linq;

namespace ExClient.Tagging
{
    [Flags]
    public enum Namespace : ushort
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
}
