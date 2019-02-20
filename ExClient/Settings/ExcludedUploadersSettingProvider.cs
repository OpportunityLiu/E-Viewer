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
        public override string ToString() => string.Join("\n", userList);

        public static string[] FromString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<string>();
            }

            return value.Split(crlf, StringSplitOptions.RemoveEmptyEntries);
        }

        private static readonly char[] crlf = new[] { '\r', '\n' };

        internal ExcludedUploadersSettingProvider()
        {
            userList = new ObservableList<string>();
            userList.VectorChanged += userList_VectorChanged;
        }

        public event BindableVectorChangedEventHandler VectorChanged;
        private void userList_VectorChanged(IBindableObservableVector vector, object e)
        {
            VectorChanged?.Invoke(this, e);
            OnPropertyChanged(nameof(Count));
        }

        internal override void DataChanged(Dictionary<string, string> settings)
        {
            settings.TryGetValue("xu", out var data);
            userList.Update(FromString(data));
        }

        internal override void ApplyChanges(Dictionary<string, string> settings)
        {
            settings["xu"] = ToString();
        }

        private readonly ObservableList<string> userList = new ObservableList<string>();

        public int Count => userList.Count;

        bool IList.IsReadOnly => ((IList)userList).IsReadOnly;
        bool ICollection<string>.IsReadOnly => ((ICollection<string>)userList).IsReadOnly;

        bool IList.IsFixedSize => ((IList)userList).IsFixedSize;

        bool ICollection.IsSynchronized => ((ICollection)userList).IsSynchronized;

        object ICollection.SyncRoot => ((ICollection)userList).SyncRoot;

        object IList.this[int index] { get => ((IList)userList)[index]; set => ((IList)userList)[index] = checkUser(value); }
        public string this[int index] { get => userList[index]; set => userList[index] = checkUser(value); }

        private static string checkUser(object value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var s = value.ToString().Trim();
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentException("Should not be empty", nameof(value));
            }

            if (s.IndexOfAny(crlf) >= 0)
            {
                throw new ArgumentException("Should not contain CR or LF", nameof(value));
            }

            return s;
        }

        public int IndexOf(string user) => userList.IndexOf(user);
        public void Insert(int index, string value) => userList.Insert(index, checkUser(value));
        public void RemoveAt(int index) => userList.RemoveAt(index);
        public void Add(string value) => userList.Add(checkUser(value));
        public void Clear() => userList.Clear();
        public bool Contains(string value) => userList.Contains(value);
        public void CopyTo(string[] array, int arrayIndex) => userList.CopyTo(array, arrayIndex);
        public bool Remove(string value) => userList.Remove(value);
        public IEnumerator<string> GetEnumerator() => userList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)userList).GetEnumerator();
        int IList.Add(object value) => ((IList)userList).Add(checkUser(value));
        bool IList.Contains(object value) => ((IList)userList).Contains(value);
        int IList.IndexOf(object value) => ((IList)userList).IndexOf(value);
        void IList.Insert(int index, object value) => ((IList)userList).Insert(index, checkUser(value));
        void IList.Remove(object value) => ((IList)userList).Remove(value);
        void ICollection.CopyTo(Array array, int index) => ((ICollection)userList).CopyTo(array, index);
    }
}
