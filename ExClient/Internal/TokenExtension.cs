using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient.Internal
{
    internal static class TokenExtension
    {
        public static string ToTokenString(this ulong token)
            => token.ToString("x10");

        public static ulong ToToken(this string token)
            => string.IsNullOrEmpty(token) ? throw new ArgumentNullException(nameof(token)) : ulong.Parse(token, System.Globalization.NumberStyles.HexNumber);

        public static ulong ToToken(this SHA1Value hash)
        {
            var data = hash.Data;
            return
                (ulong)data[0] << 32 |
                (ulong)data[1] << 24 |
                (ulong)data[2] << 16 |
                (ulong)data[3] << 8 |
                (ulong)data[4] << 0;
        }

        public static string ToTokenString(this SHA1Value hash)
        {
            var data = hash.Data;
            return
                $"{data[0]:x2}" +
                $"{data[1]:x2}" +
                $"{data[2]:x2}" +
                $"{data[3]:x2}" +
                $"{data[4]:x2}";
        }
    }
}
