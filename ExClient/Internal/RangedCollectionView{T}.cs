using System;
using System.Collections;
using System.Collections.Generic;

namespace ExClient.Internal
{
    internal class RangedCollectionView<T> : IReadOnlyList<T>
    {
        public RangedCollectionView(IReadOnlyList<T> items, int startIndex, int count)
        {
            if(items == null)
                throw new ArgumentNullException(nameof(items));
            if(unchecked((uint)startIndex > (uint)items.Count))
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if(count < 0 || startIndex + count > items.Count)
                throw new ArgumentOutOfRangeException(nameof(count));
            this.items = items;
            this.StartIndex = startIndex;
            this.Count = count;
        }

        private readonly IReadOnlyList<T> items;

        public T this[int index]
        {
            get
            {
                if(unchecked((uint)index >= (uint)this.Count))
                    throw new IndexOutOfRangeException();
                return this.items[this.StartIndex + index];
            }
        }

        public int Count { get; }
        public int StartIndex { get; }

        public IEnumerator<T> GetEnumerator()
        {
            for(var i = this.StartIndex; i < this.StartIndex + this.Count; i++)
                yield return this.items[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}