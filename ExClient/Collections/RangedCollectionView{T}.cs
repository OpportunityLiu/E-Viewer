using System;
using System.Collections;
using System.Collections.Generic;

namespace ExClient.Collections
{
    public struct RangedCollectionView<T> : IReadOnlyList<T>
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

        public RangedCollectionViewEnumerator GetEnumerator()
        {
            return new RangedCollectionViewEnumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct RangedCollectionViewEnumerator : IEnumerator<T>
        {
            internal RangedCollectionViewEnumerator(RangedCollectionView<T> parent)
            {
                this.parent = parent;
                this.currentPosition = parent.StartIndex - 1;
            }

            private RangedCollectionView<T> parent;
            private int currentPosition;

            public T Current => this.parent.items[this.currentPosition];

            object IEnumerator.Current => this.Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                this.currentPosition++;
                return this.currentPosition < this.parent.StartIndex + this.parent.Count;
            }

            public void Reset()
            {
                this.currentPosition = this.parent.StartIndex - 1;
            }
        }
    }

}