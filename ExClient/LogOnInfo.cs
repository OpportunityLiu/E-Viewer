using ExClient.Internal;

using System.Collections.Generic;
using System.Linq;

using Windows.Web.Http;

namespace ExClient
{
    public sealed class LogOnInfo
    {
        internal List<HttpCookie> EhCookies { get; }
        internal List<HttpCookie> ExCookies { get; }

        internal LogOnInfo(Client client)
        {
            EhCookies = client.CookieManager.GetCookies(DomainProvider.Eh.RootUri).ToList();
            ExCookies = client.CookieManager.GetCookies(DomainProvider.Ex.RootUri).ToList();
        }
    }
}
