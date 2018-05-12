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
            try
            {
                if (!LogIn.IsNullOrEmpty())
                    throw new InvalidOperationException("Need login");
                CheckResponseOverride(request);
                if (!Error.IsNullOrEmpty())
                    throw new InvalidOperationException(Error);
            }
            catch(Exception ex)
            {
                ex.Data["ApiRequest"] = JsonConvert.SerializeObject(request);
                ex.Data["ApiResponse"] = JsonConvert.SerializeObject(this);
                throw;
            }
        }

        protected virtual void CheckResponseOverride(ApiRequest request) { }
    }
}
