using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace ExClient.Settings
{
    public abstract class SettingProvider
    {
        internal SettingProvider()
        {
        }

        internal abstract string GetCookieContent();

        protected void ApplyChanges()
        {
            Owner.ApplyChanges();
        }

        internal SettingCollection Owner
        {
            get;
            set;
        }
    }
}
