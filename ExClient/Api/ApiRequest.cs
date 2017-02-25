using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace ExClient.Api
{
    internal abstract class ApiRequest
    {
        [JsonProperty("method")]
        public abstract string Method { get; }

        private static long uid;
        private static string key;

        [JsonProperty("apiuid", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long ApiUid => uid;

        [JsonProperty("apikey", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ApiKey => key;

        public static void UpdateToken(long uid, string key)
        {
            ApiRequest.uid = uid;
            ApiRequest.key = key;
        }

        private static Regex regUid = new Regex(@"var\s+apiuid\s*=\s*(\d+)", RegexOptions.Compiled);
        private static Regex regKey = new Regex(@"var\s+apikey\s*=\s*""([A-Fa-f0-9]+)""", RegexOptions.Compiled);

        public static void UpdateToken(string html)
        {
            var mUid = regUid.Match(html);
            if(mUid.Success)
                uid = long.Parse(mUid.Groups[1].Value);
            var mKey = regKey.Match(html);
            if(mKey.Success)
                key = mKey.Groups[1].Value;
        }
    }
}
