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
                item.Owner = this;
            }
        }

        private readonly List<SettingProvider> items = new List<SettingProvider>
        {
            new HahProxySettingProvider(),
            new ExcludedLanguagesSettingProvider()
        };

        public HahProxySettingProvider HahProxy => (HahProxySettingProvider)items[0];
        public ExcludedLanguagesSettingProvider ExcludedLanguages => (ExcludedLanguagesSettingProvider)items[1];

        internal void ApplyChanges()
        {
            var cookie = new HttpCookie("uconfig", "exhentai.org", "/")
            {
                Expires = DateTimeOffset.Now.AddYears(1),
                HttpOnly = false,
                Secure = false,
                Value = string.Join("-", items.Select(s => s.GetCookieContent()).ToArray())
            };
            owner.CookieManager.SetCookie(cookie);
        }

        private readonly Client owner;
    }
}
