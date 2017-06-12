using Opportunity.MvvmUniverse.Collections;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using IReadOnlyList = System.Collections.Generic.IReadOnlyList<ExClient.Tagging.Tag>;
using System;

namespace ExClient.Tagging
{
    public sealed class TagCollection
    {
        [DebuggerDisplay(@"\{Namespace = {Namespace} Count = {Count}\}")]
        public sealed class NamespaceTagCollection : ObservableCollectionBase, IReadOnlyList, IList
        {

            internal NamespaceTagCollection(TagCollection owner, int index)
            {
                this.Owner = owner;
                this.index = index;
            }

            internal void RaiseChangeAt(int index)
            {
                this.RaiseCollectionReplace(this[index], this[index], index);
            }

            private readonly int index;

            public TagCollection Owner { get; }

            public Namespace Namespace => this.Owner.keys[this.index];

            public int Count => this.Owner.offset[this.index + 1] - this.Owner.offset[this.index];

            public bool IsFixedSize => true;

            public bool IsReadOnly => true;

            public bool IsSynchronized => false;

            public object SyncRoot => ((IList)this.Owner).SyncRoot;

            object IList.this[int index] { get => this.data[index]; set => throw new InvalidOperationException(); }

            public Tag this[int index] => this.data[index];

            public RangedCollectionView<Tag>.RangedCollectionViewEnumerator GetEnumerator() => this.data.GetEnumerator();

            IEnumerator<Tag> IEnumerable<Tag>.GetEnumerator() => GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            int IList.Add(object value) => throw new InvalidOperationException();

            void IList.Clear() => throw new InvalidOperationException();

            bool IList.Contains(object value) => ((IList)this.data).Contains(value);

            int IList.IndexOf(object value) => ((IList)this.data).IndexOf(value);

            void IList.Insert(int index, object value) => throw new InvalidOperationException();

            void IList.Remove(object value) => throw new InvalidOperationException();

            void IList.RemoveAt(int index) => throw new InvalidOperationException();

            public void CopyTo(Array array, int index) => ((IList)this.data).CopyTo(array, index);
        }
    }
}
