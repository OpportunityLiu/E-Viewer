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
            Owner = owner;
            version = owner.Version;
            groupIndex = index;
        }

        private readonly int groupIndex;

        private readonly int version;

        public TagCollection Owner { get; }

        public Namespace Namespace => Owner.Keys[groupIndex];

        public int Count => Owner.Offset[groupIndex + 1] - Owner.Offset[groupIndex];

        bool IList.IsFixedSize => true;
        bool IList.IsReadOnly => true;
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => ((IList)Owner).SyncRoot;

        object IList.this[int index] { get => this[index]; set => throw new InvalidOperationException(); }

        public GalleryTag this[int index] => Owner.Data[Owner.Offset[groupIndex] + index];

        public IEnumerator<GalleryTag> GetEnumerator()
        {
            var end = Owner.Offset[groupIndex + 1];
            for (var i = Owner.Offset[groupIndex]; i < end; i++)
                yield return Owner.Data[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Contains(GalleryTag item) => IndexOf(item) >= 0;

        public int IndexOf(GalleryTag item)
        {
            var i = 0;
            foreach (var ii in this)
            {
                if (ii == item)
                    return i;
                i++;
            }
            return -1;
        }

        int IList.Add(object value) => throw new InvalidOperationException();
        void IList.Clear() => throw new InvalidOperationException();
        bool IList.Contains(object value) => Contains(value.TryCast<GalleryTag>());
        int IList.IndexOf(object value) => IndexOf(value.TryCast<GalleryTag>());
        void IList.Insert(int index, object value) => throw new InvalidOperationException();
        void IList.Remove(object value) => throw new InvalidOperationException();
        void IList.RemoveAt(int index) => throw new InvalidOperationException();
        public void CopyTo(Array array, int index)
        {
            if (!(array is object[] arr))
                throw new ArgumentException("Wrong type of array", nameof(array));
            if (index + Count > arr.Length)
                throw new ArgumentException("Not enough space for copying", nameof(array));
            foreach (var item in this)
            {
                arr[index] = item;
                index++;
            }
        }
    }
}
