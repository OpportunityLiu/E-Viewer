using ApplicationDataManager.Settings;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace ApplicationDataManager
{
    public class ApplicationDataCollection : ObservableObject
    {
        private const ApplicationDataCreateDisposition always = ApplicationDataCreateDisposition.Always;

        protected ApplicationDataCollection(ApplicationDataCollection parent, string containerName)
        {
            if(parent == null)
            {
                var data = ApplicationData.Current;
                this.LocalStorage = data.LocalSettings.CreateContainer(containerName, always);
                this.RoamingStorage = data.RoamingSettings.CreateContainer(containerName, always);
            }
            else
            {
                this.LocalStorage = parent.LocalStorage.CreateContainer(containerName, always);
                this.RoamingStorage = parent.RoamingStorage.CreateContainer(containerName, always);
            }
        }

        protected ApplicationDataCollection(string containerName)
            : this(null, containerName)
        {
        }

        protected ApplicationDataContainer LocalStorage
        {
            get;
        }

        protected ApplicationDataContainer RoamingStorage
        {
            get;
        }

        protected T Get<T>(ApplicationDataContainer container, [CallerMemberName]string key = null)
        {
            return Get(container, default(T), key);
        }

        protected T Get<T>(ApplicationDataContainer container, T @default, [CallerMemberName]string key = null)
        {
            try
            {
                object v;
                if(container.Values.TryGetValue(key, out v))
                {
                    if(@default is Enum)
                        return (T)Enum.Parse(typeof(T), v.ToString());
                    return (T)v;
                }
            }
            catch { }
            return @default;
        }

        private bool HasValue(ApplicationDataContainer container, string key)
        {
            return container.Values.ContainsKey(key);
        }

        protected void Set<T>(ApplicationDataContainer container, T value, [CallerMemberName]string key = null)
        {
            Set(container, value, key, false);
        }

        protected void ForceSet<T>(ApplicationDataContainer container, T value, [CallerMemberName]string key = null)
        {
            Set(container, value, key, true);
        }

        private void Set<T>(ApplicationDataContainer container, T value, string key, bool forceRaiseEvent)
        {
            if(!forceRaiseEvent && HasValue(container, key) && Equals(Get(container, value, key), value))
                return;
            var enu = value as Enum;
            if(enu != null)
                container.Values[key] = enu.ToString();
            else
                container.Values[key] = value;
            RaisePropertyChanged(key);
        }

        protected T GetLocal<T>([CallerMemberName]string key = null)
        {
            return GetLocal(default(T), key);
        }

        protected T GetLocal<T>(T @default, [CallerMemberName]string key = null)
        {
            return Get(LocalStorage, @default, key);
        }

        protected void SetLocal<T>(T value, [CallerMemberName]string key = null)
        {
            Set(LocalStorage, value, key, false);
        }

        protected void ForceSetLocal<T>(T value, [CallerMemberName]string key = null)
        {
            Set(LocalStorage, value, key, true);
        }

        protected T GetRoaming<T>(string key)
        {
            return GetRoaming(default(T), key);
        }

        protected T GetRoaming<T>(T @default, [CallerMemberName]string key = null)
        {
            return Get(RoamingStorage, @default, key);
        }

        protected void SetRoaming<T>(T value, [CallerMemberName]string key = null)
        {
            Set(RoamingStorage, value, key, false);
        }

        protected void ForceSetRoaming<T>(T value, [CallerMemberName]string key = null)
        {
            Set(RoamingStorage, value, key, true);
        }
    }
}