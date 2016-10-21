using System.Collections.Generic;
using System.Reflection;

namespace ApplicationDataManager.Settings
{
    public class GroupedSettings : List<SettingInfo>
    {
        public string GroupName
        {
            get;
            private set;
        }

        internal GroupedSettings(string name, IEnumerable<SettingInfo> contents) : base(contents)
        {
            GroupName = name;
        }
    }
}