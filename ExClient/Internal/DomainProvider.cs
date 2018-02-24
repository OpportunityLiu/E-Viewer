using System;

namespace ExClient.Internal
{
    internal sealed class DomainProvider
    {
        public Uri RootUri { get; }
        public Uri ApiUri { get; }
        public Settings.SettingCollection Settings { get; }
        public HostType Type { get; }

        private DomainProvider(HostType type, string root, string api)
        {
            Type = type;
            RootUri = new Uri(root);
            ApiUri = new Uri(api);
            Settings = new ExClient.Settings.SettingCollection(this);
        }

        public static DomainProvider Ex { get; }
            = new DomainProvider(HostType.ExHentai, "https://exhentai.org/", "https://exhentai.org/api.php");
        public static DomainProvider Eh { get; }
            = new DomainProvider(HostType.EHentai, "https://e-hentai.org/", "https://api.e-hentai.org/api.php");
    }
}
