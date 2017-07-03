using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace ExClient.Tagging
{
    [DebuggerDisplay(@"\{Namespace = {Namespace} Count = {Count}\}")]
    public sealed class NamespaceTagCollection : IReadOnlyList<GalleryTag>, IList
    {
        internal NamespaceTagCollection(TagCollection owner, int index)
        {
            this.Owner = owner;
            this.version = owner.Version;
            this.groupIndex = index;
        }

        private readonly int groupIndex;

        private readonly int version;

        public TagCollection Owner { get; }

        public Namespace Namespace => this.Owner.Keys[this.groupIndex];

        public int Count => this.Owner.Offset[this.groupIndex + 1] - this.Owner.Offset[this.groupIndex];

        public bool IsFixedSize => true;

        public bool IsReadOnly => true;

        public bool IsSynchronized => false;

        public object SyncRoot => ((IList)this.Owner).SyncRoot;

        object IList.this[int index] { get => this[index]; set => throw new InvalidOperationException(); }

        public GalleryTag this[int index] => this.Owner.Data[this.Owner.Offset[this.groupIndex] + index];

        public IEnumerator<GalleryTag> GetEnumerator()
        {
            var end = this.Owner.Offset[this.groupIndex + 1];
            for (var i = this.Owner.Offset[this.groupIndex]; i < end; i++)
            {
                yield return this.Owner.Data[i];
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
