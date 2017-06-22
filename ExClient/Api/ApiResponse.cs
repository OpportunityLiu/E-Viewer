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

        public void CheckResponse()
        {
            if (LogIn != null)
                throw new InvalidOperationException("Need login");
            if (Error != null)
                throw new Exception(Error);
        }
    }
}
