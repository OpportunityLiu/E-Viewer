using System;

namespace ExClient.Launch
{
    internal static class UriHelper
    {
        public static string Unescape(this string value)
        {
            value = value.Replace('+', ' ');
            value = Uri.UnescapeDataString(value);
            return value;
        }

        /// <summary>
        /// Unescape twice for special usage.
        /// </summary>
        /// <param name="value">string to unescape</param>
        /// <returns>unescaped string</returns>
        public static string Unescape2(this string value)
        {
            value = Uri.UnescapeDataString(value);
            return Unescape(value);
        }
        public static bool QueryValueAsBoolean(this string value)
        {
            return value != "0" && value != "";
        }

        public static int QueryValueAsInt32(this string value)
        {
            if(int.TryParse(value, out var r))
                return r;
            value = value.Trim();
            var i = 0;
            for(; i < value.Length; i++)
            {
                if(value[i] < '0' || value[i] > '9')
                    break;
            }
            if(int.TryParse(value.Substring(0, i), out r))
                return r;
            return 0;
        }
    }
}
