using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using IReadOnlyList = System.Collections.Generic.IReadOnlyList<ExClient.Tagging.Tag>;

namespace ExClient.Tagging
{
    [DebuggerDisplay(@"\{Namespace = {Namespace} Count = {Count}\}")]
    public sealed class NamespaceTagCollection : ObservableCollectionBase, IReadOnlyList, IList
    {
        internal NamespaceTagCollection(TagCollection owner, int index)
        {
            this.Owner = owner;
            this.version = owner.version;
            this.index = index;
            owner.itemStateChanged += this.Owner_itemStateChanged; ;
        }

        private void Owner_itemStateChanged(int index)
        {
            if (this.version != this.Owner.version)
            {
                dispose();
                return;
            }
            var offset = this.Owner.offset[this.index];
            if (offset <= index && index < this.Owner.offset[this.index + 1])
            {
                var item = this.Owner.data[index];
                RaiseCollectionReplace(item, item, index - offset);
            }
        }

        private void dispose()
        {
            if (this.Owner == null)
                return;
            this.Owner.itemStateChanged -= this.Owner_itemStateChanged;
            this.Owner = null;
        }

        private readonly int index;

        private readonly int version;

        public TagCollection Owner { get; private set; }

        public Namespace Namespace => this.Owner.keys[this.index];

        public int Count => this.Owner.offset[this.index + 1] - this.Owner.offset[this.index];

        public bool IsFixedSize => true;

        public bool IsReadOnly => true;

        public bool IsSynchronized => false;

        public object SyncRoot => ((IList)this.Owner).SyncRoot;

        object IList.this[int index] { get => this[index]; set => throw new InvalidOperationException(); }

        public Tag this[int index] => this.Owner.data[this.Owner.offset[this.index] + index];

        public IEnumerator<Tag> GetEnumerator()
        {
            var end = this.Owner.offset[this.index + 1];
            for (var i = this.Owner.offset[this.index]; i < end; i++)
            {
                yield return this.Owner.data[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        int IList.Add(object value) => throw new InvalidOperationException();

        void IList.Clear() => throw new InvalidOperationException();

        bool IList.Contains(object value) => throw new InvalidOperationException();

        int IList.IndexOf(object value) => throw new InvalidOperationException();

        void IList.Insert(int index, object value) => throw new InvalidOperationException();

        void IList.Remove(object value) => throw new InvalidOperationException();

        void IList.RemoveAt(int index) => throw new InvalidOperationException();

        public void CopyTo(Array array, int index) => throw new InvalidOperationException();
    }
}
