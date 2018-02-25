using ExClient.Tagging;
using System.Collections.Generic;
using System.Linq;

namespace ExClient.Settings
{
    internal sealed class ExcludedTagNamespacesSettingProvider : SettingProvider
    {
        internal ExcludedTagNamespacesSettingProvider()
        {
        }

        public override string ToString()
        {
            return this.value.ToString();
        }

        internal override void DataChanged(Dictionary<string, string> settings)
        {
            var data = Namespace.Unknown;
            foreach (var item in settings.Keys.Where(k => k.StartsWith("xn_")))
            {
                var i = ushort.Parse(item.Substring(3));
                data |= (Namespace)(1 << (i - 1));
            }
            this.Value = data;
        }

        internal override void ApplyChanges(Dictionary<string, string> settings)
        {
            foreach (var item in settings.Keys.Where(k => k.StartsWith("xn_")).ToList())
            {
                settings.Remove(item);
            }
            var value = this.Value;
            for (var i = 1; i <= 8; i++)
            {
                var check = (Namespace)(1 << (i - 1));
                if ((value & check) == check)
                    settings["xn_" + i] = "on";
            }
        }

        private Namespace value;
        public Namespace Value
        {
            get => this.value;
            set => Set(ref this.value, value & (Namespace)0xFF);
        }
    }
}
