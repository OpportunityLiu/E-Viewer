using ExClient.Galleries;
using ExClient.Internal;
using Newtonsoft.Json;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;

namespace ExClient.Api
{
    internal static class ApiToken
    {
        public static long UserID { get; private set; }
        public static string ApiKey { get; private set; }

        public static void UpdateToken(long userID, string apiKey)
        {
            UserID = userID;
            ApiKey = apiKey;
        }

        private static Regex regUid = new Regex(@"var\s+apiuid\s*=\s*(\d+)", RegexOptions.Compiled);
        private static Regex regKey = new Regex(@"var\s+apikey\s*=\s*""([A-Fa-f0-9]+)""", RegexOptions.Compiled);

        public static void Update(string html)
        {
            var mUid = regUid.Match(html);
            if (mUid.Success)
                UserID = long.Parse(mUid.Groups[1].Value);
            var mKey = regKey.Match(html);
            if (mKey.Success)
                ApiKey = mKey.Groups[1].Value;
        }
    }

    internal abstract class ApiRequest
    {
        [JsonProperty("method")]
        public abstract string Method { get; }

        [JsonProperty("apiuid", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long ApiUid => ApiToken.UserID;

        [JsonProperty("apikey", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ApiKey => ApiToken.ApiKey;
    }

    internal abstract class ApiRequest<TResponse> : ApiRequest
        where TResponse : ApiResponse
    {
        public IAsyncOperation<TResponse> GetResponseAsync()
        {
            return AsyncInfo.Run(async token =>
            {
                var reqStr = JsonConvert.SerializeObject(this);
                var req = Client.Current.HttpClient.PostStringAsync(Client.Current.Uris.ApiUri, reqStr);
                token.Register(req.Cancel);
                var res = await req;
                var resobj = JsonConvert.DeserializeObject<TResponse>(res);
                resobj.CheckResponse(this);
                return resobj;
            });
        }
    }

    internal abstract class GalleryRequest<TResponse> : ApiRequest<TResponse>
        where TResponse : ApiResponse
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
