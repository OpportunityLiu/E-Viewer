using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace ApplicationDataManager
{
    public sealed class ApplicationDataDictionary<TValue> : ApplicationDataCollection, IObservableMap<string, TValue>
    {
        public ApplicationDataDictionary(ApplicationDataCollection parent, string containerName, ApplicationDataLocality locality)
             : base(parent, containerName)
        {
            switch(locality)
            {
            case ApplicationDataLocality.Local:
                this.container = this.LocalStorage;
                break;
            case ApplicationDataLocality.Roaming:
                this.container = this.RoamingStorage;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(locality), "Must be Local or Roaming.");
            }
        }

        public ApplicationDataDictionary(string containerName, ApplicationDataLocality locality)
            : this(null, containerName, locality)
        {
        }

        public ApplicationDataLocality Locality => container.Locality;

        private readonly ApplicationDataContainer container;

        public ICollection<string> Keys => container.Values.Keys;

        private class ValueCollection : ICollection<TValue>
        {
            private ApplicationDataDictionary<TValue> parent;

            public ValueCollection(ApplicationDataDictionary<TValue> parent)
            {
                this.parent = parent;
            }

            public int Count => parent.Count;

            public bool IsReadOnly => true;

            public void Add(TValue item)
            {
                throw new InvalidOperationException();
            }

            public void Clear()
            {
                throw new InvalidOperationException();
            }

            public bool Contains(TValue item)
            {
                foreach(var i in this)
                {
                    if(Equals(i, item))
                        return true;
                }
                return false;
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                foreach(var item in this)
                {
                    array[arrayIndex] = item;
                    arrayIndex++;
                }
            }

            public IEnumerator<TValue> GetEnumerator()
            {
                foreach(var item in parent.Keys)
                {
                    yield return parent.Get<TValue>(parent.container, item);
                }
            }

            public bool Remove(TValue item)
            {
                throw new InvalidOperationException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public ICollection<TValue> Values => new ValueCollection(this);

        public int Count => container.Values.Count;

        public bool IsReadOnly => false;

        public TValue this[string key]
        {
            get
            {
                TValue r;
                if(this.TryGetValue(key, out r))
                    return r;
                throw new KeyNotFoundException();
            }
            set
            {
                if(this.ContainsKey(key))
                    raiseMapChanged(CollectionChange.ItemInserted, key);
                else
                    raiseMapChanged(CollectionChange.ItemChanged, key);
                Set(container, value, key);
            }
        }

        public event MapChangedEventHandler<string, TValue> MapChanged;

        private class MapChangedEventArgs : IMapChangedEventArgs<string>
        {
            private CollectionChange collectionChange;
            private string key;

            public MapChangedEventArgs(CollectionChange collectionChange, string key)
            {
                this.collectionChange = collectionChange;
                this.key = key;
            }

            public CollectionChange CollectionChange => collectionChange;

            public string Key => key;
        }

        private void raiseMapChanged(CollectionChange collectionChange, string key)
        {
            switch(collectionChange)
            {
            case CollectionChange.Reset:
            case CollectionChange.ItemInserted:
            case CollectionChange.ItemRemoved:
                RaisePropertyChanged(nameof(Count));
                break;
            case CollectionChange.ItemChanged:
                break;
            }
            MapChanged?.Invoke(this, new MapChangedEventArgs(collectionChange, key));
        }

        public void Add(string key, TValue value)
        {
            if(this.ContainsKey(key))
                throw new ArgumentException(nameof(key));
            this[key] = value;
        }

        public bool ContainsKey(string key)
        {
            return container.Values.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return container.Values.Remove(key);
        }

        public bool TryGetValue(string key, out TValue value)
        {
            try
            {
                if(!this.ContainsKey(key))
                {
                    value = default(TValue);
                    return false;
                }
                value = Get<TValue>(container, key);
                return true;
            }
            catch(Exception)
            {
                value = default(TValue);
                return false;
            }
        }

        void ICollection<KeyValuePair<string, TValue>>.Add(KeyValuePair<string, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            container.Values.Clear();
            raiseMapChanged(CollectionChange.Reset, null);
        }

        bool ICollection<KeyValuePair<string, TValue>>.Contains(KeyValuePair<string, TValue> item)
        {
            TValue v;
            if(this.TryGetValue(item.Key, out v))
                return Equals(item.Value, v);
            return false;
        }

        void ICollection<KeyValuePair<string, TValue>>.CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            foreach(var item in this)
            {
                array[arrayIndex] = item;
                arrayIndex++;
            }
        }

        bool ICollection<KeyValuePair<string, TValue>>.Remove(KeyValuePair<string, TValue> item)
        {
            if(this.Contains(item))
                return this.Remove(item.Key);
            else
                return false;
        }

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            foreach(var item in this.Keys)
            {
                yield return new KeyValuePair<string, TValue>(item, this[item]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
