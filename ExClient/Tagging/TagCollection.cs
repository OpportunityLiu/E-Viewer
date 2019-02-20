using ExClient.Api;
using ExClient.Galleries;
using HtmlAgilityPack;
using Opportunity.Helpers.Universal.AsyncHelpers;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using static ExClient.Tagging.Namespace;

namespace ExClient.Tagging
{
    public sealed class TagCollection
        : ObservableCollectionBase<NamespaceTagCollection>, IReadOnlyList<NamespaceTagCollection>, IList
    {
        private static readonly Namespace[] staticKeys = new[]
        {
            Reclass,
            Language,
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
            for (var i = 0; i < this.Keys.Length; i++)
            {
                if (this.Keys[i] == key)
                {
                    return i;
                }
            }
            return -1;
        }

        public TagCollection(Gallery owner, IEnumerable<Tag> items)
        {
            this.Owner = owner;
            if (items is null)
                throw new ArgumentNullException(nameof(items));
            initOrReset(items.Select(t => (t, TagState.NormalPower)));
        }

        private void initOrReset(IEnumerable<(Tag tag, TagState ts)> items)
        {
            var rawData = items.OrderBy(t => t.tag.Namespace)
                // put low-power tags to the end
                .ThenByDescending(t => t.ts & TagState.NormalPower)
                .ThenBy(t => t.tag.Content)
                .ToList();
            var data = new Tag[rawData.Count];
            var state = new TagState[rawData.Count];
            for (var i = 0; i < rawData.Count; i++)
            {
                (data[i], state[i]) = rawData[i];
            }
            if (this.Data != null && this.Data.Select(d => d.Content).SequenceEqual(data))
            {
                for (var i = 0; i < this.Data.Length; i++)
                {
                    this.Data[i].State = state[i];
                }
                return;
            }
            var keys = new Namespace[staticKeys.Length];
            var offset = new int[staticKeys.Length + 1];
            var currentIdx = 0;
            var currentNs = Unknown;
            for (var i = 0; i < data.Length; i++)
            {
                var cns = data[i].Namespace;
                if (currentNs == cns)
                {
                    continue;
                }

                currentNs = cns;
                keys[currentIdx] = currentNs;
                offset[currentIdx] = i;
                currentIdx++;
            }
            offset[currentIdx] = data.Length;
            Array.Resize(ref keys, currentIdx);
            Array.Resize(ref offset, currentIdx + 1);
            var tagData = new GalleryTag[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                tagData[i] = new GalleryTag(this, data[i], state[i]);
            }
            this.Data = tagData;
            this.Keys = keys;
            this.Offset = offset;
            this.Version++;
            OnVectorReset();
            OnPropertyChanged(nameof(Count), nameof(Items), "Groups");
        }

        internal GalleryTag[] Data;

        internal Namespace[] Keys;
        internal int[] Offset;

        internal int Version;

        public Gallery Owner { get; }

        public IReadOnlyList<GalleryTag> Items => this.Data;

        public int Count => this.Keys.Length;

        bool IList.IsFixedSize => false;

        bool IList.IsReadOnly => true;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => this;

        object IList.this[int index] { get => this[index]; set => throw new InvalidOperationException(); }

        public NamespaceTagCollection this[int index]
        {
            get
            {
                if (unchecked((uint)index >= (uint)Count))
                {
                    throw new IndexOutOfRangeException();
                }

                return new NamespaceTagCollection(this, index);
            }
        }

        public RangedListView<GalleryTag> this[Namespace key]
        {
            get
            {
                try
                {
                    return getValue(key);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    throw new KeyNotFoundException("Key not found.", ex);
                }
            }
        }

        private RangedListView<GalleryTag> getValue(Namespace key)
        {
            var i = getIndexOfKey(key);
            if (i >= 0)
            {
                return getValue(i);
            }

            if (!key.IsDefined())
            {
                throw new ArgumentOutOfRangeException(nameof(key));
            }

            return RangedListView<GalleryTag>.Empty;
        }

        private RangedListView<GalleryTag> getValue(int index)
        {
            return new RangedListView<GalleryTag>(this.Data, this.Offset[index], this.Offset[index + 1] - this.Offset[index]);
        }

        public IAsyncAction VoteAsync(Tag tag, VoteState command)
        {
            if (command != VoteState.Down && command != VoteState.Up)
                throw new ArgumentOutOfRangeException(nameof(command), LocalizedStrings.Resources.VoteOutOfRange);
            return voteAsync(new TagRequest(this, tag, command));
        }

        public IAsyncAction VoteAsync(IEnumerable<Tag> tags, VoteState command)
        {
            if (tags is null)
                throw new ArgumentNullException(nameof(tags));
            if (command != VoteState.Down && command != VoteState.Up)
                throw new ArgumentOutOfRangeException(nameof(command), LocalizedStrings.Resources.VoteOutOfRange);
            var req = new TagRequest(this, tags, command);
            if (string.IsNullOrWhiteSpace(req.Tags))
                return AsyncAction.CreateCompleted();
            return voteAsync(req);
        }

        private IAsyncAction voteAsync(TagRequest req)
        {
            return AsyncInfo.Run(async token =>
            {
                var res = await req.GetResponseAsync(token);
                var doc = HtmlNode.CreateNode(res.TagPane);
                updateCore(doc);
            });
        }

        internal void Update(HtmlDocument doc)
        {
            var tablecontainer = doc.GetElementbyId("taglist");
            if (tablecontainer is null)
                return;
            var tableNode = tablecontainer.Element("table");
            if (tableNode is null)
                return;
            updateCore(tableNode);
        }

        private void updateCore(HtmlNode tableNode)
        {
            var query = tableNode.Descendants("div")
                .Select(node =>
                {
                    var state = default(TagState);

                    if (node.HasClass("gt"))
                        state |= TagState.HighPower;
                    else if (node.HasClass("gtw"))
                        state |= TagState.LowPower;
                    else // if(node.HasClass("gtl")
                        state |= TagState.NormalPower;

                    var divstyle = node.GetAttribute("style", "opacity:1.0");
                    if (divstyle.Contains("opacity:0.4"))
                        state |= TagState.Slave;
                    //else if(divstyle.Contains("opacity:1.0"))

                    var a = node.Element("a");
                    if (a.HasClass("tup"))
                        state |= TagState.Upvoted;
                    else if (a.HasClass("tdn"))
                        state |= TagState.Downvoted;

                    var tag = node.Id.Substring(3).Replace('_', ' ');
                    return (Tag.Parse(tag), state);
                });
            initOrReset(query);
        }

        public struct Enumerator : IEnumerator<NamespaceTagCollection>
        {
            private readonly TagCollection parent;
            private readonly int version;
            private int i;

            internal Enumerator(TagCollection parent)
            {
                this.parent = parent;
                this.version = parent.Version;
                this.i = -1;
            }

            public NamespaceTagCollection Current
            {
                get
                {
                    if (this.version != this.parent.Version)
                        throw new InvalidOperationException("Collection changed.");
                    if (this.i < 0)
                        throw new InvalidOperationException("Enumeration hasn't started.");
                    if (this.i >= this.parent.Keys.Length)
                        throw new InvalidOperationException("Enumeration has ended.");
                    return new NamespaceTagCollection(this.parent, this.i);
                }
            }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (this.version != this.parent.Version)
                    throw new InvalidOperationException("Collection changed.");
                var end = this.parent.Keys.Length;
                if (this.i >= end)
                    return false;
                this.i++;
                return this.i < end;
            }

            void IDisposable.Dispose()
            {
                this.i = int.MaxValue;
            }

            public void Reset()
            {
                this.i = 0;
            }
        }

        public Enumerator GetEnumerator()
            => new Enumerator(this);

        IEnumerator<NamespaceTagCollection> IEnumerable<NamespaceTagCollection>.GetEnumerator() => GetEnumerator();
    }
}
