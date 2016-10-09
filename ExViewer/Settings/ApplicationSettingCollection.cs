using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace ExViewer.Settings
{

    public class ApplicationSettingCollection : ExClient.ObservableObject
    {
        protected ApplicationSettingCollection(string containerName)
        {
            var data = ApplicationData.Current;
            this.localStorage = data.LocalSettings.CreateContainer(containerName, ApplicationDataCreateDisposition.Always).Values;
            this.roamingStorage = data.RoamingSettings.CreateContainer(containerName, ApplicationDataCreateDisposition.Always).Values;
        }

        private bool loaded;
        private object syncroot = new object();

        private void load()
        {
            if(loaded)
                return;
            lock(syncroot)
            {
                if(loaded)
                    return;
                loaded = true;
                properties = (from property in this.GetType().GetRuntimeProperties()
                              where property.GetCustomAttribute<SettingAttribute>() != null
                              select new SettingInfo(property)).ToDictionary(si => si.Name);
                groupedProperties = (from setting in properties.Values
                                     orderby setting.Index
                                     group setting by setting.Category into grouped
                                     select new GroupedSettings(grouped.Key, grouped)).ToList();
#if DEBUG
                testingProperties = (from property in this.GetType().GetRuntimeProperties()
                                     let t = property.GetCustomAttribute<TestingValueAttribute>()
                                     where t != null
                                     select Tuple.Create(property.Name, t.Value)).ToDictionary(p => p.Item1, p => p.Item2);
#endif
            }
        }

        private List<GroupedSettings> groupedProperties;

#if DEBUG
        private Dictionary<string, object> testingProperties;
#endif

        public List<GroupedSettings> GroupedSettings => groupedProperties;

        private readonly IPropertySet localStorage;
        private readonly IPropertySet roamingStorage;

        private Dictionary<string, SettingInfo> properties;

        private T Get<T>(IPropertySet container, T @default, string key)
        {
            load();
#if DEBUG
            if(testingProperties.ContainsKey(key))
                return (T)testingProperties[key];
#endif
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
            load();
#if DEBUG
            if(testingProperties.ContainsKey(key))
                return true;
#endif
            return container.ContainsKey(key);
        }

        private void Set<T>(IPropertySet container, T value, string key, bool forceRaiseEvent)
        {
            load();
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