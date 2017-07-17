using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ExClient.Api
{
    internal interface IRequestOf<TResponse>
        where TResponse : ApiResponse
    {

    }

    internal interface IResponseOf<TRequest>
        where TRequest : ApiRequest
    {

    }

    internal static class ApiHelper
    {
        public static IAsyncOperation<TResponse> GetResponseAsync<TResponse>(this IRequestOf<TResponse> request)
            where TResponse : ApiResponse
        {
            return request.GetResponseAsync(null);
        }

        public static IAsyncOperation<TResponse> GetResponseAsync<TResponse>(this IRequestOf<TResponse> request, Action<TResponse> responseChecker)
            where TResponse : ApiResponse
        {
            return AsyncInfo.Run(async token =>
            {
                var reqStr = JsonConvert.SerializeObject(request);
                var req = Client.Current.HttpClient.PostStringAsync(Client.Current.Uris.ApiUri, reqStr);
                token.Register(req.Cancel);
                var res = await req;
                var resobj = JsonConvert.DeserializeObject<TResponse>(res);
                responseChecker?.Invoke(resobj);
                resobj.CheckResponse();
                return resobj;
            });
        }
    }
}
