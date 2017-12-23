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
            get => this.ip;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    if (Set(nameof(AddressAndPort), ref this.ip, null))
                        ApplyChanges();
                    return;
                }
                try
                {
                    var ipv4 = value.Split(dotChars).Select(s => byte.Parse("0" + s)).ToArray();
                    Set(nameof(AddressAndPort), ref this.ip, $"{ipv4[0]}.{ipv4[1]}.{ipv4[2]}.{ipv4[3]}");
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(LocalizedStrings.Resources.OnlyIpv4, nameof(value), ex);
                }
                ApplyChanges();
            }
        }

        private uint port = 80;

        public uint Port
        {
            get => this.port;
            set
            {
                if (Set(nameof(AddressAndPort), ref this.port, value))
                    ApplyChanges();
            }
        }

        public string AddressAndPort
        {
            get
            {
                if (string.IsNullOrEmpty(this.ip))
                    return null;
                return $"{this.ip}:{this.port}";
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    this.port = 80;
                    this.ip = null;
                    OnPropertyChanged(nameof(Port), nameof(IPAddress), nameof(AddressAndPort));
                    ApplyChanges();
                    return;
                }
                try
                {
                    var ss = value.Split(colonChars, StringSplitOptions.RemoveEmptyEntries);
                    this.port = uint.Parse(ss[1]);
                    IPAddress = ss[0];
                }
                catch (Exception ex)
                {
                    ApplyChanges();
                    throw new ArgumentException(LocalizedStrings.Resources.OnlyIpv4, nameof(value), ex);
                }
            }
        }

        internal override string GetCookieContent()
        {
            var ap = AddressAndPort;
            if (ap == null)
                return null;
            return $"hh_{ap}";
        }
    }
}
