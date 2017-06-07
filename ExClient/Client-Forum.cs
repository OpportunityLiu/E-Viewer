using HtmlAgilityPack;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ExClient
{
    public partial class Client
    {
        public static Uri ForumsUri { get; } = new Uri("https://forums.e-hentai.org/");

        private static readonly Uri logOnUri = new Uri(ForumsUri, "index.php?act=Login&CODE=01");

        public IAsyncOperation<UserInfo> FetchCurrentUserInfoAsync()
        {
            if (this.UserID < 0)
                throw new InvalidOperationException("Hasn't log in");
            return UserInfo.FeachAsync(this.UserID);
        }
    }
}