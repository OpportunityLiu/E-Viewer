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

namespace ExViewer
{
    public class Settings : ExClient.ObservableObject
    {
        [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
        sealed class RoamingAttribute : Attribute
        {
            public RoamingAttribute()
            {
            }
        }

        public static Settings Current
        {
            get;
            private set;
        } = new Settings();

        private Settings()
        {
            ApplicationData.Current.DataChanged += DataChanged;
            roamingProperties = (from property in this.GetType().GetRuntimeProperties()
                                 where property.CustomAttributes.Any(data => data.AttributeType == typeof(RoamingAttribute))
                                 select property.Name).ToList();
        }

        private List<string> roamingProperties;

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
    }
}
