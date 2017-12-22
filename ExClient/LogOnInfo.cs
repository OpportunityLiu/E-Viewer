using ExClient.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace ExClient
{
    public sealed class LogOnInfo
    {
        internal List<HttpCookie> Cookies { get; }

        internal LogOnInfo(Client client)
        {
            this.Cookies = client.CookieManager.GetCookies(UriProvider.Eh.RootUri)
                      .Concat(client.CookieManager.GetCookies(UriProvider.Ex.RootUri)).ToList();
        }
    }
}
