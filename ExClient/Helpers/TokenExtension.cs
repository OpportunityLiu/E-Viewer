using System;

namespace ExClient
{
    internal static class TokenExtension
    {
        public const int TOKEN_LENGTH = 5;
        public const int TOKEN_STR_LENGTH = 10;

        public static string ToTokenString(this ulong token)
            => token.ToString("x" + TOKEN_STR_LENGTH);

        public static ulong ToToken(this string token)
            => string.IsNullOrEmpty(token) ? throw new ArgumentNullException(nameof(token)) : ulong.Parse(token, System.Globalization.NumberStyles.HexNumber);
    }
}
