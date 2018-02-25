using ExClient.Internal;
using System.Collections.Generic;
using System.Linq;
using Windows.Web.Http;

namespace ExClient
{
    public sealed class LogOnInfo
    {
        internal List<HttpCookie> Cookies { get; }

        internal LogOnInfo(Client client)
        {
            this.Cookies = client.CookieManager.GetCookies(DomainProvider.Eh.RootUri)
                      .Concat(client.CookieManager.GetCookies(DomainProvider.Ex.RootUri)).ToList();
        }
    }
}
