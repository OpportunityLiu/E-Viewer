using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient.Api
{
    internal abstract class ApiResponse
    {
        [JsonProperty("error")]
        public string Error { get; set; }

        public void CheckResponse()
        {
            if(string.IsNullOrEmpty(Error))
                return;
            throw new Exception(Error);
        }
    }
}
