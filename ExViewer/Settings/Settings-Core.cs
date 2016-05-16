using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace ExViewer.Settings
{

    public partial class Settings : ExClient.ObservableObject
    {
        public static Settings Current
        {
            get;
            private set;
        } = new Settings();

        private Settings()
        {
            roamingProperties = (from property in this.GetType().GetRuntimeProperties()
                                 where property.CustomAttributes.Any(data => data.AttributeType == typeof(RoamingAttribute))
                                 select property.Name).ToList();
            groupedProperties = (from property in this.GetType().GetRuntimeProperties()
                                 where property.GetCustomAttribute<SettingAttribute>() != null
                                 select new SettingInfo(property) into setting
                                 orderby setting.Index
                                 group setting by setting.Category into grouped
                                 select new GroupedSettings(grouped.Key, grouped)).ToList();
#if DEBUG
            testingProperties = (from property in this.GetType().GetRuntimeProperties()
                                 let t = property.GetCustomAttribute<TestingValueAttribute>()
                                 where t != null
                                 select Tuple.Create(property.Name, t.Value)).ToDictionary(p => p.Item1, p => p.Item2);
#endif
            ApplicationData.Current.DataChanged += DataChanged;
        }

        private List<string> roamingProperties;
        private List<GroupedSettings> groupedProperties;

#if DEBUG
        private Dictionary<string, object> testingProperties;
#endif

        public List<GroupedSettings> GroupedSettings => groupedProperties;

        private void DataChanged(ApplicationData sender, object args)
        {
            foreach(var item in roamingProperties)
            {
                RaisePropertyChanged(item);
            }
        }

        private IPropertySet local = ApplicationData.Current.LocalSettings.Values;
        private IPropertySet roaming = ApplicationData.Current.RoamingSettings.Values;

        private T Get<T>(IPropertySet cotainer, T def, string key)
        {
#if DEBUG
            if(testingProperties.ContainsKey(key))
                return (T)testingProperties[key];
#endif
            object v;
            if(cotainer.TryGetValue(key, out v))
            {
                if(def is Enum)
                    return (T)Enum.Parse(typeof(T), v.ToString());
                return (T)v;
            }
            Set(cotainer, def, key);
            return def;
        }

        private void Set<T>(IPropertySet cotainer, T value, string key)
        {
            var enu = value as Enum;
            if(enu != null)
                cotainer[key] = enu.ToString();
            else
                cotainer[key] = value;
            RaisePropertyChanged(key);
        }

        protected T GetLocal<T>(string key)
        {
            return GetLocal(default(T), key);
        }

        protected T GetLocal<T>(T def, [CallerMemberName]string key = null)
        {
            return Get(local, def, key);
        }

        protected void SetLocal<T>(T value, [CallerMemberName]string key = null)
        {
            Set(local, value, key);
        }

        protected T GetRoaming<T>(string key)
        {
            return GetRoaming(default(T), key);
        }

        protected T GetRoaming<T>(T def, [CallerMemberName]string key = null)
        {
            return Get(roaming, def, key);
        }

        protected void SetRoaming<T>(T value, [CallerMemberName]string key = null)
        {
            Set(roaming, value, key);
        }
    }
}