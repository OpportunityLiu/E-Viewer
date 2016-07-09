using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace ExClient
{
    public class HahProxyConfig
    {
        public HahProxyConfig(string addressAndPort) : this(addressAndPort, null) { }

        private static readonly char[] colonChars = ":：፥᠄꛴꞉ː˸⦂𒑱﹕：︓".ToCharArray();
        private static readonly char[] dotChars = ".։𖫵۔。᠃︒｡．﹒።𛲟⳹꓿᠉⳾᙮⸼꘎꛳𝪈܁܂".ToCharArray();

        public HahProxyConfig(string addressAndPort, string passkey)
        {
            if(string.IsNullOrWhiteSpace(addressAndPort))
                throw new ArgumentNullException(nameof(addressAndPort));
            Passkey = passkey?.Trim();
            try
            {
                var ss = addressAndPort.Split(colonChars, StringSplitOptions.RemoveEmptyEntries);
                Port = uint.Parse(ss[1]);
                var ipv4 = ss[0].Split(dotChars).Select(s => byte.Parse("0" + s)).ToArray();
                IPAddress = $"{ipv4[0]}.{ipv4[1]}.{ipv4[2]}.{ipv4[3]}";
            }
            catch(Exception ex)
            {
                throw new ArgumentException(nameof(addressAndPort), LocalizedStrings.Resources.OnlyIpv4, ex);
            }
        }

        public HahProxyConfig(string clientAddress, uint port) : this(clientAddress, port, null) { }

        public HahProxyConfig(string clientAddress, uint port, string passkey) : this($"{clientAddress}:{port}", passkey) { }

        public string IPAddress
        {
            get;
        }

        public uint Port
        {
            get;
        }

        public string Passkey
        {
            get;
        }

        public string AddressAndPort => $"{IPAddress}:{Port}";

        internal HttpCookie GetCookie()
        {
            return new HttpCookie("uconfig", "exhentai.org", "/")
            {
                Expires = DateTimeOffset.Now.AddYears(1),
                HttpOnly = false,
                Secure = false,
                Value = $"hp_{AddressAndPort}-hk_{Passkey}"
            };
        }
    }
}
