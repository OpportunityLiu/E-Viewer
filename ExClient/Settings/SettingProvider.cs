using System.Collections.Generic;

namespace ExClient.Settings
{
    public abstract class SettingProvider : Opportunity.MvvmUniverse.ObservableObject
    {
        internal SettingCollection Owner { get; set; }

        internal SettingProvider() { }

        internal abstract void DataChanged(Dictionary<string, string> settings);

        internal abstract void ApplyChanges(Dictionary<string, string> settings);
    }
}
