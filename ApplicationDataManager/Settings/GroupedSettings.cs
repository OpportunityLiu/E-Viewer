using System.Collections.Generic;

namespace ApplicationDataManager.Settings
{
    public sealed class GroupedSettings : List<SettingInfo>
    {
        public string GroupName
        {
            get;
            private set;
        }

        internal GroupedSettings(string name, IEnumerable<SettingInfo> contents) : base(contents)
        {
            this.GroupName = name;
        }
    }
}