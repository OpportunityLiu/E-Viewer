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
            this.Data = items.OrderBy(t => t.Namespace).ToArray();
            this.Offset = new int[staticKeys.Length + 1];
            this.keys = new Namespace[staticKeys.Length];
            var currentIdx = 0;
            var currentNs = Unknown;
            for(var i = 0; i < this.Data.Length; i++)
            {
                var current = this.Data[i];
                if(currentNs == current.Namespace)
                    continue;
                currentNs = current.Namespace;
                this.keys[currentIdx] = currentNs;
                this.Offset[currentIdx] = i;
                currentIdx++;
            }
            this.Offset[currentIdx] = this.Data.Length;
            Array.Resize(ref this.keys, currentIdx);
            Array.Resize(ref this.Offset, currentIdx + 1);
            Items = new ReadOnlyTagList(this);
        }

        internal readonly Tag[] Data;
        internal readonly int[] Offset;
        private readonly Namespace[] keys;

        public IReadOnlyList Items { get; }

        private sealed class ReadOnlyTagList : IReadOnlyList
        {
            private readonly TagCollection owner;

            internal ReadOnlyTagList(TagCollection owner)
            {
                this.owner = owner;
            }

            public Tag this[int index] => this.owner.Data[index];

            public int Count => this.owner.Data.Length;

            public IEnumerator<Tag> GetEnumerator()
            {
                var data = this.owner.Data;
                for(var i = 0; i < data.Length; i++)
                {
                    yield return data[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

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

        public IReadOnlyList this[Namespace key]
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
                    return new RangedCollectionView<Tag>(this.Data, 0, 0);
                else
                    throw new ArgumentOutOfRangeException(nameof(key));
            }
            return getValue(i);
        }

        private RangedCollectionView<Tag> getValue(int index)
        {
            return new RangedCollectionView<Tag>(this.Data, this.Offset[index], this.Offset[index + 1] - this.Offset[index]);
        }

        public IEnumerator<NamespaceTagCollection> GetEnumerator()
        {
            for(var i = 0; i < this.keys.Length; i++)
            {
                yield return new NamespaceTagCollection(this.keys[i], new RangedCollectionView<Tag>(this.Data, this.Offset[i], this.Offset[i + 1] - this.Offset[i]));
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [DebuggerDisplay(@"\{Namespace = {Namespace} Count = {Count}\}")]
    public sealed class NamespaceTagCollection : IReadOnlyList
    {
        internal NamespaceTagCollection(Namespace @namespace, RangedCollectionView<Tag> data)
        {
            this.Namespace = @namespace;
            this.data = data;
        }

        public Namespace Namespace { get; }

        public int Count => this.data.Count;

        public Tag this[int index] => this.data[index];

        private RangedCollectionView<Tag> data;

        public IEnumerator<Tag> GetEnumerator() => this.data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
