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
            this.groupedSettings = new Lazy<ReadOnlyCollection<GroupedSettings>>(this.loadGroupedSettings, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private ReadOnlyCollection<GroupedSettings> loadGroupedSettings()
        {
            var properties = from property in this.GetType().GetRuntimeProperties()
                             let attr = property.GetCustomAttribute<SettingAttribute>()
                             where attr != null
                             select new SettingInfo(property, this, attr);
            return new ReadOnlyCollection<GroupedSettings>(
                (from setting in properties
                 orderby setting.Index
                 group setting by setting.Category into grouped
                 select new GroupedSettings(grouped.Key, grouped)).ToArray());
        }

        private readonly Lazy<ReadOnlyCollection<GroupedSettings>> groupedSettings;

        public IReadOnlyList<GroupedSettings> GroupedSettings => this.groupedSettings.Value;

        private readonly IPropertySet localStorage;
        private readonly IPropertySet roamingStorage;

        private T get<T>(IPropertySet container, T @default, string key)
        {
            try
            {
                if(container.TryGetValue(key, out var v))
                {
                    if(@default is Enum)
                        return (T)Enum.Parse(typeof(T), v.ToString());
                    return (T)v;
                }
            }
            catch { }
            return @default;
        }

        private bool hasValue(IPropertySet container, string key) 
            => container.ContainsKey(key);

        private void set<T>(IPropertySet container, T value, string key, bool forceRaiseEvent)
        {
            if(!forceRaiseEvent && hasValue(container, key) && Equals(get(container, value, key), value))
                return;
            if(value is Enum)
                container[key] = value.ToString();
            else
                container[key] = value;
            RaisePropertyChanged(key);
        }

        protected T GetLocal<T>([CallerMemberName]string key = null) 
            => GetLocal(default(T), key);

        protected T GetLocal<T>(T @default, [CallerMemberName]string key = null)
            => get(this.localStorage, @default, key);

        protected void SetLocal<T>(T value, [CallerMemberName]string key = null)
            => set(this.localStorage, value, key, false);

        protected void ForceSetLocal<T>(T value, [CallerMemberName]string key = null) 
            => set(this.localStorage, value, key, true);

        protected T GetRoaming<T>(string key) 
            => GetRoaming(default(T), key);

        protected T GetRoaming<T>(T @default, [CallerMemberName]string key = null)
            => get(this.roamingStorage, @default, key);

        protected void SetRoaming<T>(T value, [CallerMemberName]string key = null) 
            => set(this.roamingStorage, value, key, false);

        protected void ForceSetRoaming<T>(T value, [CallerMemberName]string key = null) 
            => set(this.roamingStorage, value, key, true);
    }
}