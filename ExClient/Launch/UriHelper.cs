using System;
using System.Collections.Generic;
using Windows.Foundation;

namespace ExClient.Launch
{
    internal static class UriHelper
    {
        public static bool QueryValueAsBoolean(this string value)
        {
            return value != "0" && value != "";
        }

        public static int QueryValueAsInt32(this string value)
        {
            if (int.TryParse(value, out var r))
            {
                return r;
            }

            value = value.Trim();
            var i = 0;
            for (; i < value.Length; i++)
            {
                if (value[i] < '0' || value[i] > '9')
                {
                    break;
                }
            }
            if (int.TryParse(value.Substring(0, i), out r))
            {
                return r;
            }

            return 0;
        }

        public static string GetString(this WwwFormUrlDecoder query, string key)
        {
            try
            {
                return query.GetFirstValueByName(key);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public static int GetInt32(this WwwFormUrlDecoder query, string key)
        {
            try
            {
                return query.GetFirstValueByName(key).QueryValueAsInt32();
            }
            catch (ArgumentException)
            {
                return 0;
            }
        }

        public static bool GetBoolean(this WwwFormUrlDecoder query, string key)
        {
            try
            {
                return query.GetFirstValueByName(key).QueryValueAsBoolean();
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
