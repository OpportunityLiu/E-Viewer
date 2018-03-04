using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Windows.UI.Xaml.Interop;

namespace ExClient.Settings
{
    public sealed class ExcludedUploadersSettingProvider
        : SettingProvider, IBindableObservableVector, IList<string>, ICollection<string>, IEnumerable<string>, IEnumerable, IReadOnlyList<string>, IReadOnlyCollection<string>, IList, ICollection
    {
        public override string ToString() => string.Join("\n", this.userList);

        public static string[] FromString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Array.Empty<string>();
            return value.Split(crlf, StringSplitOptions.RemoveEmptyEntries);
        }

        private static readonly char[] crlf = new[] { '\r', '\n' };

        internal ExcludedUploadersSettingProvider()
        {
            this.userList = new ObservableList<string>();
            this.userList.VectorChanged += this.userList_VectorChanged;
        }


        public event BindableVectorChangedEventHandler VectorChanged;
        private void userList_VectorChanged(IBindableObservableVector vector, object e)
        {
            this.VectorChanged?.Invoke(this, e);
        }

        internal override void DataChanged(Dictionary<string, string> settings)
        {
            settings.TryGetValue("xu", out var data);
            this.userList.Update(FromString(data));
        }

        internal override void ApplyChanges(Dictionary<string, string> settings)
        {
            settings["xu"] = ToString();
        }

        private readonly ObservableList<string> userList = new ObservableList<string>();

        public int Count => this.userList.Count;

        bool IList.IsReadOnly => ((IList)this.userList).IsReadOnly;
        bool ICollection<string>.IsReadOnly => ((ICollection<string>)this.userList).IsReadOnly;

        bool IList.IsFixedSize => ((IList)this.userList).IsFixedSize;

        bool ICollection.IsSynchronized => ((ICollection)this.userList).IsSynchronized;

        object ICollection.SyncRoot => ((ICollection)this.userList).SyncRoot;

        object IList.this[int index] { get => ((IList)this.userList)[index]; set => ((IList)this.userList)[index] = checkUser(value); }
        public string this[int index] { get => this.userList[index]; set => this.userList[index] = checkUser(value); }

        private static string checkUser(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            var s = value.ToString().Trim();
            if (string.IsNullOrEmpty(s))
                throw new ArgumentException("Should not be empty", nameof(value));
            if (s.IndexOfAny(crlf) >= 0)
                throw new ArgumentException("Should not contain CR or LF", nameof(value));
            return s;
        }

        public int IndexOf(string user) => this.userList.IndexOf(user);
        public void Insert(int index, string value) => this.userList.Insert(index, checkUser(value));
        public void RemoveAt(int index) => this.userList.RemoveAt(index);
        public void Add(string value) => this.userList.Add(checkUser(value));
        public void Clear() => this.userList.Clear();
        public bool Contains(string value) => this.userList.Contains(value);
        public void CopyTo(string[] array, int arrayIndex) => this.userList.CopyTo(array, arrayIndex);
        public bool Remove(string value) => this.userList.Remove(value);
        public IEnumerator<string> GetEnumerator() => this.userList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.userList).GetEnumerator();
        int IList.Add(object value) => ((IList)this.userList).Add(checkUser(value));
        bool IList.Contains(object value) => ((IList)this.userList).Contains(value);
        int IList.IndexOf(object value) => ((IList)this.userList).IndexOf(value);
        void IList.Insert(int index, object value) => ((IList)this.userList).Insert(index, checkUser(value));
        void IList.Remove(object value) => ((IList)this.userList).Remove(value);
        void ICollection.CopyTo(Array array, int index) => ((ICollection)this.userList).CopyTo(array, index);
    }
}
