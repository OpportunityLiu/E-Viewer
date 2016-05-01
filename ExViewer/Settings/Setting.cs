using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Storage;
using System.Reflection;
using Windows.UI.Xaml;

namespace ExViewer.Settings
{

    public class Setting : ExClient.ObservableObject
    {
        public static Setting Current
        {
            get;
            private set;
        } = new Setting();

        private Setting()
        {
            ApplicationData.Current.DataChanged += DataChanged;
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

        [Roaming]
        [Setting("Searching", "Default keywords on the fount page", Index = 10)]
        public string DefaultSearchString
        {
            get
            {
                return GetRoaming("");
            }
            set
            {
                SetRoaming(value);
            }
        }

        [Roaming]
        [Setting("Searching", "Default categories on the front page", Index = 20)]
        public ExClient.Category DefaultSearchCategory
        {
            get
            {
                return GetRoaming(ExClient.Category.All);
            }
            set
            {
                SetRoaming(value);
            }
        }

        [Setting("Overall", "The theme of the app", Index = 10)]
        public ApplicationTheme Theme
        {
            get
            {
                return GetLocal(ApplicationTheme.Dark);
            }
            set
            {
                SetLocal(value);
            }
        }

        [Setting("Image viewing", "Zoom factor for double tapping", Index = 10)]
        [SingleRange(1, 4, Small = 0.1)]
        public float DefaultFactor
        {
            get
            {
                return GetLocal(2f);
            }
            set
            {
                SetLocal(value);
            }
        }

        [Setting("Image viewing","Maximum zoom factor", Index = 20)]
        [SingleRange(4, 8, Small = 0.1)]
        public float MaxFactor
        {
            get
            {
                return GetLocal(8f);
            }
            set
            {
                SetLocal(value);
            }
        }

        [Setting("Image viewing","Factor for inertia of mouse dragging, set to 0 to disable", Index = 30)]
        [DoubleRange(0, 1, Small = 0.05)]
        public double MouseInertialFactor
        {
            get
            {
                return GetLocal(0.5);
            }
            set
            {
                SetLocal(value);
            }
        }

        [Setting("Image viewing", "The latency for the command bar to hide or show after tapping", Index = 40)]
        [Int32Range(0, 1000, Tick = 100, Small = 10, Large = 100)]
        public int ChangeCommandBarDelay
        {
            get
            {
                return GetLocal(150);
            }
            set
            {
                SetLocal(value);
            }
        }
    }
}
