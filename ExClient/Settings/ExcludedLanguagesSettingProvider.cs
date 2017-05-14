using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ExClient.Settings
{
    public sealed class ExcludedLanguagesSettingProvider : SettingProvider, ICollection<ExcludedLanguage>
    {
        internal ExcludedLanguagesSettingProvider()
        {
        }

        public static string ToString(IEnumerable<ExcludedLanguage> items)
        {
            return string.Join(", ", items);
        }

        public static IEnumerable<ExcludedLanguage> FromString(string value)
        {
            foreach(var item in (value ?? "").Split(", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if(Enum.TryParse(item, out ExcludedLanguage r))
                    yield return r;
            }
        }

        internal override string GetCookieContent()
        {
            if(this.items.Count == 0)
                return null;
            return $"xl_{string.Join("x", this.items)}";
        }

        public override string ToString()
        {
            return ToString(this);
        }

        public void AddRange(IEnumerable<ExcludedLanguage> items)
        {
            foreach(var item in items)
            {
                if(!item.IsDefined())
                    throw new ArgumentOutOfRangeException(nameof(item));
                this.items.Add((ushort)item);
            }
            ApplyChanges();
            RaisePropertyChanged(nameof(Count));
        }

        private HashSet<ushort> items = new HashSet<ushort>();

        public void Add(ExcludedLanguage item)
        {
            if(!item.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(item));
            if(this.items.Add((ushort)item))
            {
                ApplyChanges();
                RaisePropertyChanged(nameof(Count));
            }
        }

        public void Clear()
        {
            if(this.items.Count == 0)
                return;
            this.items.Clear();
            ApplyChanges();
            RaisePropertyChanged(nameof(Count));
        }

        public bool Contains(ExcludedLanguage item) => this.items.Contains((ushort)item);

        public void CopyTo(ExcludedLanguage[] array, int arrayIndex)
        {
            var i = arrayIndex;
            foreach(var item in this)
            {
                if(i >= array.Length)
                    break;
                array[i] = item;
                i++;
            }
        }

        public bool Remove(ExcludedLanguage item)
        {
            var r = this.items.Remove((ushort)item);
            if(r)
            {
                ApplyChanges();
                RaisePropertyChanged(nameof(Count));
            }
            return r;
        }

        public IEnumerator<ExcludedLanguage> GetEnumerator() => this.items.Cast<ExcludedLanguage>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.items.GetEnumerator();

        public int Count => this.items.Count;

        bool ICollection<ExcludedLanguage>.IsReadOnly => false;
    }
}
