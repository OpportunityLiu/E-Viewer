﻿using ExClient.Galleries;

using Newtonsoft.Json;

using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Windows.Web.Http;

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

        private static readonly Regex _RegUid = new Regex(@"var\s+apiuid\s*=\s*(\d+)", RegexOptions.Compiled);
        private static readonly Regex _RegKey = new Regex(@"var\s+apikey\s*=\s*""([A-Fa-f0-9]+)""", RegexOptions.Compiled);

        public static void Update(string html)
        {
            var mUid = _RegUid.Match(html);
            if (mUid.Success)
            {
                UserID = long.Parse(mUid.Groups[1].Value);
            }

            var mKey = _RegKey.Match(html);
            if (mKey.Success)
            {
                ApiKey = mKey.Groups[1].Value;
            }
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
        public async Task<TResponse> GetResponseAsync(CancellationToken token = default)
        {
            var reqStr = JsonConvert.SerializeObject(this);
            var resStr = default(string);
            try
            {
                var req = Client.Current.HttpClient.PostStringAsync(Client.Current.Uris.ApiUri, new HttpStringContent(reqStr));
                token.Register(() => req?.Cancel());
                resStr = await req;
                if (resStr.IsNullOrEmpty() || resStr[0] == '<')
                {
                    // sometimes apis returns HTML, try a second time
                    req = Client.Current.HttpClient.PostStringAsync(Client.Current.Uris.ApiUri, new HttpStringContent(reqStr));
                    resStr = await req;
                }
                var resobj = JsonConvert.DeserializeObject<TResponse>(resStr);
                resobj.CheckResponse(this);
                return resobj;
            }
            catch (Exception ex)
            {
                ex.AddData("ApiRequest", reqStr);
                if (resStr != null)
                    ex.AddData("ApiResponse", resStr);
                throw;
            }
        }
    }

    internal abstract class GalleryRequest<TResponse> : ApiRequest<TResponse>
        where TResponse : ApiResponse
    {
        public GalleryRequest(Gallery gallery)
        {
            GalleryID = gallery.Id;
            GalleryToken = gallery.Token.ToString();
        }

        [JsonProperty("gid")]
        public long GalleryID { get; }

        [JsonProperty("token")]
        public string GalleryToken { get; }
    }
}
