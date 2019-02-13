using Newtonsoft.Json;
using System;

namespace ExClient.Api
{
    internal abstract class ApiResponse
    {
        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("login")]
        public string LogIn { get; set; }

        public void CheckResponse(ApiRequest request)
        {
            if (!LogIn.IsNullOrEmpty())
                throw new InvalidOperationException(LocalizedStrings.Resources.WrongAccountInfo);
            CheckResponseOverride(request);
            if (!Error.IsNullOrEmpty())
                throw new InvalidOperationException(Error);
        }

        protected virtual void CheckResponseOverride(ApiRequest request) { }
    }
}
