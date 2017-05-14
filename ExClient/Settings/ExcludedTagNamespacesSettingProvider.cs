using ExClient.Tagging;

namespace ExClient.Settings
{
    public sealed class ExcludedTagNamespacesSettingProvider : SettingProvider
    {
        internal ExcludedTagNamespacesSettingProvider()
        {
        }

        internal override string GetCookieContent()
        {
            if(this.ns == 0)
                return null;
            return $"xns_{this.ns}";
        }

        public override string ToString()
        {
            return this.ns.ToString();
        }

        private byte ns;

        public Namespace Value
        {
            get => (Namespace)ns;
            set
            {
                var v = unchecked((byte)value);
                if(Set(ref this.ns, v))
                    ApplyChanges();
            }
        }
    }
}
