using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace ApplicationDataManager.Settings
{
    public class ApplicationSettingCollection : ObservableObject
    {
        protected ApplicationSettingCollection(string containerName)
        {
            var data = ApplicationData.Current;
            this.localStorage = data.LocalSettings.CreateContainer(containerName, ApplicationDataCreateDisposition.Always).Values;
            this.roamingStorage = data.RoamingSettings.CreateContainer(containerName, ApplicationDataCreateDisposition.Always).Values;
            this.groupedSettings = new Lazy<ReadOnlyCollection<GroupedSettings>>(loadGroupedSettings, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
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

        private readonly IPropertySet localStorage;
        private readonly IPropertySet roamingStorage;

        private T Get<T>(IPropertySet container, T @default, string key)
        {
            try
            {
                object v;
                if(container.TryGetValue(key, out v))
                {
                    if(@default is Enum)
                        return (T)Enum.Parse(typeof(T), v.ToString());
                    return (T)v;
                }
            }
            catch { }
            return @default;
        }

        private bool HasValue(IPropertySet container, string key)
        {
            return container.ContainsKey(key);
        }

        private void Set<T>(IPropertySet container, T value, string key, bool forceRaiseEvent)
        {
            if(!forceRaiseEvent && HasValue(container, key) && Equals(Get(container, value, key), value))
                return;
            var enu = value as Enum;
            if(enu != null)
                container[key] = enu.ToString();
            else
                container[key] = value;
            RaisePropertyChanged(key);
        }

        protected T GetLocal<T>([CallerMemberName]string key = null)
        {
            return GetLocal(default(T), key);
        }

        protected T GetLocal<T>(T @default, [CallerMemberName]string key = null)
        {
            return Get(localStorage, @default, key);
        }

        protected void SetLocal<T>(T value, [CallerMemberName]string key = null)
        {
            Set(localStorage, value, key, false);
        }

        protected void ForceSetLocal<T>(T value, [CallerMemberName]string key = null)
        {
            Set(localStorage, value, key, true);
        }

        protected T GetRoaming<T>(string key)
        {
            return GetRoaming(default(T), key);
        }

        protected T GetRoaming<T>(T @default, [CallerMemberName]string key = null)
        {
            return Get(roamingStorage, @default, key);
        }

        protected void SetRoaming<T>(T value, [CallerMemberName]string key = null)
        {
            Set(roamingStorage, value, key, false);
        }

        protected void ForceSetRoaming<T>(T value, [CallerMemberName]string key = null)
        {
            Set(roamingStorage, value, key, true);
        }
    }
}