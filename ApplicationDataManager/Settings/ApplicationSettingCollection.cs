using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ApplicationDataManager.Settings
{
    public class ApplicationSettingCollection : ApplicationDataCollection
    {
        protected ApplicationSettingCollection(ApplicationDataCollection parent, string containerName)
            : base(parent, containerName)
        {
            this.groupedSettings = new Lazy<ReadOnlyCollection<GroupedSettings>>(loadGroupedSettings, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        }

        protected ApplicationSettingCollection(string containerName)
            : this(null, containerName)
        {
        }

        private ReadOnlyCollection<GroupedSettings> loadGroupedSettings()
        {
            var properties = from property in this.GetType().GetRuntimeProperties()
                             where property.GetCustomAttribute<SettingAttribute>() != null
                             select new SettingInfo(property, this);
            return new ReadOnlyCollection<GroupedSettings>(
                (from setting in properties
                 orderby setting.Index
                 group setting by setting.Category into grouped
                 select new GroupedSettings(grouped.Key, grouped)).ToArray());
        }

        private readonly Lazy<ReadOnlyCollection<GroupedSettings>> groupedSettings;

        public IReadOnlyList<GroupedSettings> GroupedSettings => groupedSettings.Value;
    }
}
