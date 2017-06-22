using System.Collections.Generic;
using System.Linq;
using Windows.Web.Http;

namespace ExClient.Settings
{
    public class SettingCollection
    {
        private readonly Client client;

        internal SettingCollection(Client client)
        {
            this.client = client;
            foreach (var item in this.items)
            {
                item.Value.Owner = this;
            }
        }

        private readonly Dictionary<string, SettingProvider> items = new Dictionary<string, SettingProvider>
        {
            ["Default"] = new DefaultSettingProvider(),
            [nameof(HahProxy)] = new HahProxySettingProvider(),
            [nameof(ExcludedLanguages)] = new ExcludedLanguagesSettingProvider(),
            [nameof(ExcludedTagNamespaces)] = new ExcludedTagNamespacesSettingProvider()
        };

        private SettingProvider getProvider([System.Runtime.CompilerServices.CallerMemberName]string key = null)
        {
            return this.items[key];
        }

        public HahProxySettingProvider HahProxy => (HahProxySettingProvider)getProvider();
        public ExcludedLanguagesSettingProvider ExcludedLanguages => (ExcludedLanguagesSettingProvider)getProvider();
        public ExcludedTagNamespacesSettingProvider ExcludedTagNamespaces => (ExcludedTagNamespacesSettingProvider)getProvider();

        internal void ApplyChanges()
        {
            var cookie = new HttpCookie("uconfig", "exhentai.org", "/")
            {
                Value = string.Join("-", this.items.Values.Select(s => s.GetCookieContent()).Where(s => s != null).ToArray())
            };
            this.client.CookieManager.SetCookie(cookie);
            cookie = new HttpCookie("uconfig", "e-hentai.org", "/")
            {
                Value = string.Join("-", this.items.Values.Select(s => s.GetCookieContent()).Where(s => s != null).ToArray())
            };
            this.client.CookieManager.SetCookie(cookie);
        }

        private class DefaultSettingProvider : SettingProvider
        {
            internal override string GetCookieContent()
            {
                // Thumbnail Size - Large
                // Thumbnail Rows - LV2(means 4)
                // search Result Count - LV0(means 25) 
                // Favorite Search - order by Favorite time
                return "ts_l-tr_2-rc_0-fs_f";
            }
        }
    }
}
