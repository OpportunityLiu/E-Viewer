using System;
using System.Linq;

namespace ExClient.Settings
{
    public sealed class HahProxySettingProvider : SettingProvider
    {
        private static readonly char[] colonChars = ":：፥᠄꛴꞉ː˸⦂𒑱﹕：︓".ToCharArray();
        private static readonly char[] dotChars = ".։𖫵۔。᠃︒｡．﹒።𛲟⳹꓿᠉⳾᙮⸼꘎꛳𝪈܁܂".ToCharArray();

        internal HahProxySettingProvider()
        {
        }

        private string ip;

        public string IPAddress
        {
            get => ip;
            set
            {
                if(string.IsNullOrWhiteSpace(value))
                {
                    ip = null;
                    return;
                }
                try
                {
                    var ipv4 = value.Split(dotChars).Select(s => byte.Parse("0" + s)).ToArray();
                    ip = $"{ipv4[0]}.{ipv4[1]}.{ipv4[2]}.{ipv4[3]}";
                }
                catch(Exception ex)
                {
                    throw new ArgumentException(LocalizedStrings.Resources.OnlyIpv4, nameof(value), ex);
                }
                ApplyChanges();
            }
        }

        private uint port = 80;

        public uint Port
        {
            get => port;
            set
            {
                port = value;
                ApplyChanges();
            }
        }

        public string AddressAndPort
        {
            get
            {
                if(string.IsNullOrEmpty(ip))
                    return null;
                return $"{ip}:{port}";
            }
            set
            {
                if(string.IsNullOrWhiteSpace(value))
                {
                    port = 80;
                    ip = null;
                    return;
                }
                try
                {
                    var ss = value.Split(colonChars, StringSplitOptions.RemoveEmptyEntries);
                    port = uint.Parse(ss[1]);
                    IPAddress = ss[0];
                }
                catch(Exception ex)
                {
                    ApplyChanges();
                    throw new ArgumentException(LocalizedStrings.Resources.OnlyIpv4, nameof(value), ex);
                }
            }
        }

        internal override string GetCookieContent()
        {
            var ap = AddressAndPort;
            if(ap == null)
                return null;
            return $"hh_{ap}";
        }
    }
}
