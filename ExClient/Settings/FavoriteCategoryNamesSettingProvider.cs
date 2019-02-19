using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ExClient.Settings
{
    public sealed class FavoriteCategoryNamesSettingProvider : SettingProvider, IList<string>, IList, INotifyCollectionChanged
    {
        private readonly string[] data = new string[10];

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        internal override void ApplyChanges(Dictionary<string, string> settings)
        {
            for (var i = 0; i < data.Length; i++)
            {
                settings["favorite_" + i] = data[i];
            }
        }

        internal override void DataChanged(Dictionary<string, string> settings)
        {
            for (var i = 0; i < data.Length; i++)
            {
                if (!settings.TryGetValue("favorite_" + i, out var name))
                {
                    name = "Favorite " + i;
                }

                this[i] = name;
            }
        }

        public string this[int index]
        {
            get => data[index];
            set
            {
                var old = data[index];
                if (Set(ref data[index], value))
                {
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, old, index));
                    var fav = Client.Current?.Favorites;
                    if (fav != null)
                    {
                        fav[index].OnNameChanged();
                    }
                }
            }
        }
        object IList.this[int index] { get => this[index]; set => this[index] = (string)value; }

        public int Count => 10;

        public bool Contains(string item) => ((IList<string>)data).Contains(item);
        bool IList.Contains(object value) => ((IList)data).Contains(value);
        public void CopyTo(string[] array, int arrayIndex) => ((IList<string>)data).CopyTo(array, arrayIndex);
        void ICollection.CopyTo(Array array, int index) => data.CopyTo(array, index);
        public int IndexOf(string item) => ((IList<string>)data).IndexOf(item);
        int IList.IndexOf(object value) => ((IList)data).IndexOf(value);
        public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)data).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        bool ICollection<string>.IsReadOnly => false;
        bool IList.IsFixedSize => true;
        bool IList.IsReadOnly => false;
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => data;


        void ICollection<string>.Add(string item) => throw new NotSupportedException();
        void ICollection<string>.Clear() => throw new NotSupportedException();
        void IList<string>.Insert(int index, string item) => throw new NotSupportedException();
        bool ICollection<string>.Remove(string item) => throw new NotSupportedException();
        void IList<string>.RemoveAt(int index) => throw new NotSupportedException();
        int IList.Add(object value) => throw new NotSupportedException();
        void IList.Clear() => throw new NotSupportedException();
        void IList.Insert(int index, object value) => throw new NotSupportedException();
        void IList.Remove(object value) => throw new NotSupportedException();
        void IList.RemoveAt(int index) => throw new NotSupportedException();
    }
}
