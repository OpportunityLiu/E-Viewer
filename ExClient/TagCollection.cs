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
            for(int i = 0; i < keys.Length; i++)
            {
                if(keys[i] == key)
                    return i;
            }
            return -1;
        }

        public TagCollection(IEnumerable<Tag> items)
        {
            data = items.OrderBy(t => t.Namespace).ToArray();
            offset = new int[staticKeys.Length + 1];
            keys = new Namespace[staticKeys.Length];
            var currentIdx = 0;
            var currentNs = Unknown;
            for(int i = 0; i < data.Length; i++)
            {
                var current = data[i];
                if(currentNs == current.Namespace)
                    continue;
                currentNs = current.Namespace;
                keys[currentIdx] = currentNs;
                offset[currentIdx] = i;
                currentIdx++;
            }
            offset[currentIdx] = data.Length;
            Array.Resize(ref keys, currentIdx);
            Array.Resize(ref offset, currentIdx + 1);
            Items = new ReadOnlyTagList(this);
        }

        internal readonly Tag[] data;
        internal readonly int[] offset;
        private readonly Namespace[] keys;

        public ReadOnlyTagList Items { get; }

        public int Count => keys.Length;

        public NamespaceTagCollection this[int index]
        {
            get
            {
                if(unchecked((uint)index >= (uint)Count))
                    throw new IndexOutOfRangeException();
                return new NamespaceTagCollection(keys[index], this.getValue(index));
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
                if(Enum.IsDefined(typeof(Namespace), key))
                    return new RangedCollectionView<Tag>(data, 0, 0);
                else
                    throw new ArgumentOutOfRangeException(nameof(key));
            }
            return getValue(i);
        }

        private RangedCollectionView<Tag> getValue(int index)
        {
            return new RangedCollectionView<Tag>(data, offset[index], offset[index + 1] - offset[index]);
        }

        public IEnumerator<NamespaceTagCollection> GetEnumerator()
        {
            for(int i = 0; i < keys.Length; i++)
            {
                yield return new NamespaceTagCollection(keys[i], new RangedCollectionView<Tag>(data, offset[i], offset[i + 1] - offset[i]));
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public sealed class ReadOnlyTagList : IReadOnlyList
    {
        private readonly TagCollection owner;

        internal ReadOnlyTagList(TagCollection owner)
        {
            this.owner = owner;
        }

        public Tag this[int index] => owner.data[index];

        public int Count => owner.data.Length;

        public IEnumerator<Tag> GetEnumerator()
        {
            var data = owner.data;
            for(int i = 0; i < data.Length; i++)
            {
                yield return data[i];
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

        public int Count => data.Count;

        public Tag this[int index] => data[index];

        private RangedCollectionView<Tag> data;

        public IEnumerator<Tag> GetEnumerator() => data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
