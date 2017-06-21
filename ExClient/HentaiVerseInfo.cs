using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient
{
    public class HentaiVerseInfo
    {
        public static Uri RootUri { get; } = new Uri("http://hentaiverse.org/");

        public static Uri LogOnUri => new Uri(RootUri, $"login.php?ipb_member_id={Client.Current.UserID}&ipb_pass_hash={Client.Current.PassHash}");
    }
}
