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
