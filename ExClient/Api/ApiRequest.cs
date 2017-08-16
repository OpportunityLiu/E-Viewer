using ExClient.Galleries;
using ExClient.Internal;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace ExClient.Api
{
    internal abstract class ApiRequest
    {
        [JsonProperty("method")]
        public abstract string Method { get; }

        private static long userID;
        private static string apiKey;

        [JsonProperty("apiuid", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long ApiUid => userID;

        [JsonProperty("apikey", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ApiKey => apiKey;

        public static void UpdateToken(long userID, string apiKey)
        {
            ApiRequest.userID = userID;
            ApiRequest.apiKey = apiKey;
        }

        private static Regex regUid = new Regex(@"var\s+apiuid\s*=\s*(\d+)", RegexOptions.Compiled);
        private static Regex regKey = new Regex(@"var\s+apikey\s*=\s*""([A-Fa-f0-9]+)""", RegexOptions.Compiled);

        public static void UpdateToken(string html)
        {
            var mUid = regUid.Match(html);
            if (mUid.Success)
                userID = long.Parse(mUid.Groups[1].Value);
            var mKey = regKey.Match(html);
            if (mKey.Success)
                apiKey = mKey.Groups[1].Value;
        }
    }

    internal abstract class GalleryRequest : ApiRequest
    {
        public GalleryRequest(Gallery gallery)
        {
            this.GalleryID = gallery.ID;
            this.GalleryToken = gallery.Token.ToTokenString();
        }

        [JsonProperty("gid")]
        public long GalleryID { get; }

        [JsonProperty("token")]
        public string GalleryToken { get; }
    }
}
