using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace ExClient.Settings
{
    public sealed class ExcludedLanguagesSettingProvider : SettingProvider, ICollection<ExcludedLanguage>, INotifyCollectionChanged
    {
        internal ExcludedLanguagesSettingProvider() { }

        public static string ToString(IEnumerable<ExcludedLanguage> items)
        {
            return string.Join(", ", items);
        }

        public static IEnumerable<ExcludedLanguage> FromString(string value)
        {
            foreach (var item in (value ?? "").Split(", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if (Enum.TryParse<ExcludedLanguage>(item, out var r))
                    yield return r;
            }
        }

        public override string ToString()
        {
            return ToString(this);
        }

        public void AddRange(IEnumerable<ExcludedLanguage> items)
        {
            foreach (var item in items)
            {
                if (!item.IsDefined())
                    throw new ArgumentOutOfRangeException(nameof(item));
                this.items.Add((ushort)item);
            }
            OnPropertyChanged(nameof(Count));
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private HashSet<ushort> items = new HashSet<ushort>();

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Add(ExcludedLanguage item)
        {
            if (!item.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(item));
            if (this.items.Add((ushort)item))
            {
                OnPropertyChanged(nameof(Count));
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public void Clear()
        {
            if (this.items.Count == 0)
                return;
            this.items.Clear();
            OnPropertyChanged(nameof(Count));
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(ExcludedLanguage item) => this.items.Contains((ushort)item);

        public void CopyTo(ExcludedLanguage[] array, int arrayIndex)
        {
            var i = arrayIndex;
            foreach (var item in this)
            {
                if (i >= array.Length)
                    break;
                array[i] = item;
                i++;
            }
        }

        public bool Remove(ExcludedLanguage item)
        {
            var r = this.items.Remove((ushort)item);
            if (r)
            {
                OnPropertyChanged(nameof(Count));
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            return r;
        }

        public IEnumerator<ExcludedLanguage> GetEnumerator() => this.items.Cast<ExcludedLanguage>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.items.GetEnumerator();

        internal override void DataChanged(Dictionary<string, string> settings)
        {
            this.items.Clear();
            foreach (var item in settings.Keys.Where(k => k.StartsWith("xl_")))
            {
                var i = ushort.Parse(item.Substring(3));
                this.items.Add(i);
            }
            OnPropertyChanged(nameof(Count));
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        internal override void ApplyChanges(Dictionary<string, string> settings)
        {
            foreach (var item in settings.Keys.Where(k => k.StartsWith("xl_")).ToList())
            {
                settings.Remove(item);
            }
            foreach (var item in this.items)
            {
                settings["xl_" + item] = "on";
            }
        }

        public int Count => this.items.Count;

        bool ICollection<ExcludedLanguage>.IsReadOnly => false;
    }
}
