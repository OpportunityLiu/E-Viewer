using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient.Internal
{
    internal static class TokenExtension
    {
        public static string TokenToString(this ulong token)
            => token.ToString("x10");

        public static ulong StringToToken(this string token)
            => string.IsNullOrEmpty(token) ? 0ul : ulong.Parse(token, System.Globalization.NumberStyles.HexNumber);
    }
}
