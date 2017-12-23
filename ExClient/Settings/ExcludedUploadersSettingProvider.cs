using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient.Settings
{
    public class ExcludedUploadersSettingProvider
        : SettingProvider, INotifyCollectionChanged, IList<long>, ICollection<long>, IEnumerable<long>, IEnumerable, IReadOnlyList<long>, IReadOnlyCollection<long>, IList, ICollection
    {
        internal ExcludedUploadersSettingProvider()
        {
            this.uidList = new ObservableList<long>();
            this.uidList.CollectionChanged += this.UidList_CollectionChanged;
        }

        private void UidList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.CollectionChanged?.Invoke(this, e);
        }

        private readonly ObservableList<long> uidList = new ObservableList<long>();

        public int Count => this.uidList.Count;

        bool IList.IsReadOnly => ((IList)this.uidList).IsReadOnly;
        bool ICollection<long>.IsReadOnly => ((ICollection<long>)this.uidList).IsReadOnly;

        bool IList.IsFixedSize => ((IList)this.uidList).IsFixedSize;

        bool ICollection.IsSynchronized => ((ICollection)this.uidList).IsSynchronized;

        object ICollection.SyncRoot => ((ICollection)this.uidList).SyncRoot;

        object IList.this[int index] { get => ((IList)this.uidList)[index]; set => ((IList)this.uidList)[index] = value; }
        public long this[int index] { get => this.uidList[index]; set => this.uidList[index] = value; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        internal override string GetCookieContent()
        {
            if (this.uidList.Count == 0)
                return null;
            return "xu_" + string.Join('x', this.uidList);
        }

        private static long checkUserID(long userID)
        {
            if (userID <= 0)
                throw new ArgumentOutOfRangeException(nameof(userID));
            return userID;
        }

        private static long checkUserID(object userID)
        {
            if (!(userID is long u))
                throw new ArgumentException("Wrong type, System.Int64 needed.", nameof(userID));
            return checkUserID(u);
        }

        public int IndexOf(long userID) => this.uidList.IndexOf(userID);
        public void Insert(int index, long userID) => this.uidList.Insert(index, checkUserID(userID));
        public void RemoveAt(int index) => this.uidList.RemoveAt(index);
        public void Add(long userID) => this.uidList.Add(checkUserID(userID));
        public void Clear() => this.uidList.Clear();
        public bool Contains(long userID) => this.uidList.Contains(userID);
        public void CopyTo(long[] array, int arrayIndex) => this.uidList.CopyTo(array, arrayIndex);
        public bool Remove(long userID) => this.uidList.Remove(userID);
        public IEnumerator<long> GetEnumerator() => this.uidList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.uidList).GetEnumerator();
        int IList.Add(object value) => ((IList)this.uidList).Add(checkUserID(value));
        bool IList.Contains(object value) => ((IList)this.uidList).Contains(value);
        int IList.IndexOf(object value) => ((IList)this.uidList).IndexOf(value);
        void IList.Insert(int index, object value) => ((IList)this.uidList).Insert(index, checkUserID(value));
        void IList.Remove(object value) => ((IList)this.uidList).Remove(value);
        void ICollection.CopyTo(Array array, int index) => ((ICollection)this.uidList).CopyTo(array, index);
    }
}
