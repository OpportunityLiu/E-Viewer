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

        private readonly Dictionary<Type, SettingProvider> items = new Dictionary<Type, SettingProvider>
        {
            [typeof(DefaultSettingProvider)] = new DefaultSettingProvider(),
            [typeof(HahProxySettingProvider)] = new HahProxySettingProvider(),
            [typeof(ExcludedLanguagesSettingProvider)] = new ExcludedLanguagesSettingProvider()
        };

        public HahProxySettingProvider HahProxy => (HahProxySettingProvider)items[typeof(HahProxySettingProvider)];
        public ExcludedLanguagesSettingProvider ExcludedLanguages => (ExcludedLanguagesSettingProvider)items[typeof(ExcludedLanguagesSettingProvider)];

        internal void ApplyChanges()
        {
            var cookie = new HttpCookie("uconfig", "exhentai.org", "/")
            {
                Expires = DateTimeOffset.Now.AddYears(1),
                HttpOnly = false,
                Secure = false,
                Value = string.Join("-", items.Values.Select(s => s.GetCookieContent()).ToArray())
            };
            owner.CookieManager.SetCookie(cookie);
        }

        private class DefaultSettingProvider : SettingProvider
        {
            internal override string GetCookieContent()
            {
                return "ts_l";
            }
        }

        private readonly Client owner;
    }
}
