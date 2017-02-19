using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient.Settings
{
    public sealed class ExcludedTagNamespacesSettingProvider : SettingProvider
    {
        internal ExcludedTagNamespacesSettingProvider()
        {
        }

        internal override string GetCookieContent()
        {
            if(ns == 0)
                return null;
            return $"xns_{ns}";
        }

        public override string ToString()
        {
            return ns.ToString();
        }

        private byte ns;

        public Namespace Value
        {
            get
            {
                return (Namespace)ns;
            }
            set
            {
                var v = unchecked((byte)value);
                if(v == ns)
                    return;
                ns = v;
                ApplyChanges();
            }
        }
    }
}
