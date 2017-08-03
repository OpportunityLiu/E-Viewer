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
    internal interface IRequestOf<TRequest, TResponse>
        where TRequest : ApiRequest, IRequestOf<TRequest, TResponse>
        where TResponse : ApiResponse, IResponseOf<TRequest, TResponse>
    {
    }

    internal interface IResponseOf<TRequest, TResponse>
        where TRequest : ApiRequest, IRequestOf<TRequest, TResponse>
        where TResponse : ApiResponse, IResponseOf<TRequest, TResponse>
    {
    }

    internal static class ApiHelper
    {
        public static IAsyncOperation<TResponse> GetResponseAsync<TRequest, TResponse>(this IRequestOf<TRequest, TResponse> request)
            where TRequest : ApiRequest, IRequestOf<TRequest, TResponse>
            where TResponse : ApiResponse, IResponseOf<TRequest, TResponse>
        {
            return request.GetResponseAsync(null);
        }

        public static IAsyncOperation<TResponse> GetResponseAsync<TRequest, TResponse>(this IRequestOf<TRequest, TResponse> request, Action<TResponse> responseChecker)
            where TRequest : ApiRequest, IRequestOf<TRequest, TResponse>
            where TResponse : ApiResponse, IResponseOf<TRequest, TResponse>
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
