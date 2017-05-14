using Opportunity.MvvmUniverse.Collections;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using IReadOnlyList = System.Collections.Generic.IReadOnlyList<ExClient.Tagging.Tag>;

namespace ExClient.Tagging
{
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

        IEnumerator<Tag> IEnumerable<Tag>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
