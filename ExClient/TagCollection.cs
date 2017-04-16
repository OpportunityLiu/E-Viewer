using ExClient.Collections;
using ExClient.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static ExClient.Namespace;
using IReadOnlyList = System.Collections.Generic.IReadOnlyList<ExClient.Tag>;

namespace ExClient
{
    [DebuggerDisplay(@"\{{data.Length} tags in {keys.Length} namespaces\}")]
    public sealed class TagCollection : IReadOnlyList<NamespaceTagCollection>
    {
        private static readonly Namespace[] staticKeys = new[]
        {
            Reclass,
            Namespace.Language,
            Parody,
            Character,
            Group,
            Artist,
            Male,
            Female,
            Misc
        };

        private int getIndexOfKey(Namespace key)
        {
            for(var i = 0; i < this.keys.Length; i++)
            {
                if(this.keys[i] == key)
                    return i;
            }
            return -1;
        }

        public TagCollection(IEnumerable<Tag> items)
        {
            this.data = items.OrderBy(t => t.Namespace).ToArray();
            this.offset = new int[staticKeys.Length + 1];
            this.keys = new Namespace[staticKeys.Length];
            var currentIdx = 0;
            var currentNs = Unknown;
            for(var i = 0; i < this.data.Length; i++)
            {
                var current = this.data[i];
                if(currentNs == current.Namespace)
                    continue;
                currentNs = current.Namespace;
                this.keys[currentIdx] = currentNs;
                this.offset[currentIdx] = i;
                currentIdx++;
            }
            this.offset[currentIdx] = this.data.Length;
            Array.Resize(ref this.keys, currentIdx);
            Array.Resize(ref this.offset, currentIdx + 1);
            Items = new RangedCollectionView<Tag>(this.data, 0, this.data.Length);
        }

        private readonly Tag[] data;
        private readonly int[] offset;
        private readonly Namespace[] keys;

        public RangedCollectionView<Tag> Items { get; }

        public int Count => this.keys.Length;

        public NamespaceTagCollection this[int index]
        {
            get
            {
                if(unchecked((uint)index >= (uint)Count))
                    throw new IndexOutOfRangeException();
                return new NamespaceTagCollection(this.keys[index], this.getValue(index));
            }
        }

        public RangedCollectionView<Tag> this[Namespace key]
        {
            get
            {
                try
                {
                    return getValue(key);
                }
                catch(ArgumentOutOfRangeException ex)
                {
                    throw new KeyNotFoundException("Key not found.", ex);
                }
            }
        }

        private RangedCollectionView<Tag> getValue(Namespace key)
        {
            var i = getIndexOfKey(key);
            if(i < 0)
            {
                if(key.IsDefined())
                    return new RangedCollectionView<Tag>(this.data, 0, 0);
                else
                    throw new ArgumentOutOfRangeException(nameof(key));
            }
            return getValue(i);
        }

        private RangedCollectionView<Tag> getValue(int index)
        {
            return new RangedCollectionView<Tag>(this.data, this.offset[index], this.offset[index + 1] - this.offset[index]);
        }

        public IEnumerator<NamespaceTagCollection> GetEnumerator()
        {
            for(var i = 0; i < this.keys.Length; i++)
            {
                yield return new NamespaceTagCollection(this.keys[i], new RangedCollectionView<Tag>(this.data, this.offset[i], this.offset[i + 1] - this.offset[i]));
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [DebuggerDisplay(@"\{Namespace = {Namespace} Count = {Count}\}")]
    public struct NamespaceTagCollection : IReadOnlyList
    {
        internal NamespaceTagCollection(Namespace @namespace, RangedCollectionView<Tag> data)
        {
            this.Namespace = @namespace;
            this.data = data;
        }

        private RangedCollectionView<Tag> data;

        public Namespace Namespace { get; }

        public int Count => this.data.Count;

        public Tag this[int index] => this.data[index];

        public RangedCollectionView<Tag>.RangedCollectionViewEnumerator GetEnumerator() => this.data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<Tag> IEnumerable<Tag>.GetEnumerator() => GetEnumerator();
    }
}
