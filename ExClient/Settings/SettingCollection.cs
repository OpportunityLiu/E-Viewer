using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace ExClient.Settings
{
    public class SettingCollection
    {
        internal SettingCollection(Client owner)
        {
            this.owner = owner;
            foreach(var item in items)
            {
                item.Value.Owner = this;
            }
            ApplyChanges();
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
            return items[key];
        }

        public HahProxySettingProvider HahProxy => (HahProxySettingProvider)getProvider();
        public ExcludedLanguagesSettingProvider ExcludedLanguages => (ExcludedLanguagesSettingProvider)getProvider();
        public ExcludedTagNamespacesSettingProvider ExcludedTagNamespaces => (ExcludedTagNamespacesSettingProvider)getProvider();

        internal void ApplyChanges()
        {
            var cookie = new HttpCookie("uconfig", "exhentai.org", "/")
            {
                Expires = DateTimeOffset.Now.AddYears(1),
                HttpOnly = false,
                Secure = false,
                Value = string.Join("-", items.Values.Select(s => s.GetCookieContent()).Where(s => s != null).ToArray())
            };
            owner.CookieManager.SetCookie(cookie);
        }

        private class DefaultSettingProvider : SettingProvider
        {
            internal override string GetCookieContent()
            {
                return "ts_l-tr_2-rc_0";
            }
        }

        private readonly Client owner;
    }
}
