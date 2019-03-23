using System;

namespace ExClient.Internal
{
    internal sealed class DomainProvider
    {
        public Uri RootUri { get; }
        public Uri ApiUri { get; }
        public Settings.SettingCollection Settings { get; }
        public HostType Type { get; }
        public Uri FileSearchUri { get; }

        private DomainProvider(HostType type, string root, string api, string fileSearch)
        {
            Type = type;
            RootUri = new Uri(root);
            ApiUri = new Uri(api);
            FileSearchUri = new Uri(fileSearch);
            Settings = new Settings.SettingCollection(this);
        }

        public static DomainProvider Ex { get; }
            = new DomainProvider(HostType.ExHentai, "https://exhentai.org/", "https://exhentai.org/api.php", "https://exhentai.org/upload/image_lookup.php");
        public static DomainProvider Eh { get; }
            = new DomainProvider(HostType.EHentai, "https://e-hentai.org/", "https://api.e-hentai.org/api.php", "https://upload.e-hentai.org/image_lookup.php");
    }
}