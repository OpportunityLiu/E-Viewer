using System.Collections.Generic;
using System.Reflection;

namespace ExViewer.Settings
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