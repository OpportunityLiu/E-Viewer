using ExClient.Api;
using ExClient.Galleries;
using ExClient.Internal;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Opportunity.MvvmUniverse.AsyncHelpers;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using static ExClient.Tagging.Namespace;

namespace ExClient.Tagging
{
    [DebuggerDisplay(@"\{{data.Length} tags in {keys.Length} namespaces\}")]
    public sealed class TagCollection
        : ObservableCollectionBase, IReadOnlyList<NamespaceTagCollection>, IList
    {
        private static readonly Namespace[] staticKeys = new[]
        {
            Reclass,
            Language,
            Parody,
            Character,
            Namespace.Group,
            Artist,
            Male,
            Female,
            Misc
        };

        private int getIndexOfKey(Namespace key)
        {
            for (var i = 0; i < this.keys.Length; i++)
            {
                if (this.keys[i] == key)
                    return i;
            }
            return -1;
        }

        public TagCollection(Gallery owner, IEnumerable<Tag> items)
        {
            this.Owner = owner;
            if (items == null)
                throw new ArgumentNullException(nameof(items));
            initOrReset(items.Select(t => (t, TagState.NormalPower)));
        }

        internal event Action<int> itemStateChanged;

        private void initOrReset(IEnumerable<(Tag tag, TagState ts)> items)
        {
            var rawData = items.OrderBy(t => t.tag.Namespace)
                // put low-power tags to the end
                .ThenByDescending(t => t.ts & TagState.NormalPower)
                .ThenBy(t => t.tag.Content)
                .ToArray();
            var data = new Tag[rawData.Length];
            var state = new TagState[rawData.Length];
            for (var i = 0; i < rawData.Length; i++)
            {
                (data[i], state[i]) = rawData[i];
            }
            if (this.data != null && this.state != null && this.data.SequenceEqual(data))
            {
                var notify = this.itemStateChanged;
                if (notify == null)
                {
                    this.state = state;
                    return;
                }
                for (var i = 0; i < this.state.Length; i++)
                {
                    var oldState = this.state[i];
                    var newState = state[i];
                    if (oldState == newState)
                        continue;
                    this.state[i] = newState;
                    notify(i);
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
                    continue;
                currentNs = cns;
                keys[currentIdx] = currentNs;
                offset[currentIdx] = i;
                currentIdx++;
            }
            offset[currentIdx] = data.Length;
            Array.Resize(ref keys, currentIdx);
            Array.Resize(ref offset, currentIdx + 1);
            this.data = data;
            this.state = state;
            this.keys = keys;
            this.offset = offset;
            RaiseCollectionReset();
            RaisePropertyChanged(nameof(Count), nameof(Items), "Groups");
        }

        internal Tag[] data;
        internal TagState[] state;

        internal Namespace[] keys;
        internal int[] offset;

        internal int version;

        public Gallery Owner { get; }

        public IReadOnlyList<Tag> Items => this.data;

        public int Count => this.keys.Length;

        bool IList.IsFixedSize => false;

        bool IList.IsReadOnly => true;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot => null;

        object IList.this[int index] { get => this[index]; set => throw new NotImplementedException(); }

        [IndexerName("Groups")]
        public NamespaceTagCollection this[int index]
        {
            get
            {
                if (unchecked((uint)index >= (uint)Count))
                    throw new IndexOutOfRangeException();
                return new NamespaceTagCollection(this, index);
            }
        }

        [IndexerName("Groups")]
        public RangedCollectionView<Tag> this[Namespace key]
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

        private RangedCollectionView<Tag> getValue(Namespace key)
        {
            var i = getIndexOfKey(key);
            if (i < 0)
            {
                if (key.IsDefined())
                    return RangedCollectionView<Tag>.Empty;
                else
                    throw new ArgumentOutOfRangeException(nameof(key));
            }
            return getValue(i);
        }

        private RangedCollectionView<Tag> getValue(int index)
        {
            return new RangedCollectionView<Tag>(this.data, this.offset[index], this.offset[index + 1] - this.offset[index]);
        }

        public IAsyncAction VoteAsync(Tag tag, VoteState command)
        {
            if (command != VoteState.Down && command != VoteState.Up)
                throw new ArgumentOutOfRangeException(nameof(command), LocalizedStrings.Resources.VoteOutOfRange);
            return voteAsync(new TagRequest(this, tag, command));
        }

        public IAsyncAction VoteAsync(IEnumerable<Tag> tags, VoteState command)
        {
            if (tags == null)
                throw new ArgumentNullException(nameof(tags));
            if (command != VoteState.Down && command != VoteState.Up)
                throw new ArgumentOutOfRangeException(nameof(command), LocalizedStrings.Resources.VoteOutOfRange);
            var req = new TagRequest(this, tags, command);
            if (string.IsNullOrWhiteSpace(req.Tags))
                return AsyncWrapper.CreateCompleted();
            return voteAsync(req);
        }

        private static Regex tagNotValid = new Regex(@"\s*The tag .+? is not currently valid\.\s*");
        private static Regex tagNeedNs = new Regex(@"\s*The tag .+? is not allowed in this namespace - requires male: or female:\s*");
        private static Regex tagCantVote = new Regex(@"\s*Cannot vote for tag\.\s*");

        private IAsyncAction voteAsync(TagRequest req)
        {
            return AsyncInfo.Run(async token =>
            {
                var res = await Client.Current.HttpClient.PostApiAsync(req);
                var r = JsonConvert.DeserializeObject<TagResponse>(res);
                if (r.Error != null)
                {
                    if (tagNotValid.IsMatch(r.Error))
                        throw new InvalidOperationException(LocalizedStrings.Resources.TagNotValid);
                    if (tagNeedNs.IsMatch(r.Error))
                        throw new InvalidOperationException(LocalizedStrings.Resources.TagNeedNamespace);
                    if (tagCantVote.IsMatch(r.Error))
                        throw new InvalidOperationException(LocalizedStrings.Resources.TagNoVotePremition);
                }
                r.CheckResponse();
                var doc = HtmlNode.CreateNode(r.TagPane);
                updateCore(doc);
            });
        }

        internal void Update(HtmlDocument doc)
        {
            var tablecontainer = doc.GetElementbyId("taglist");
            if (tablecontainer == null)
                return;
            var tableNode = tablecontainer.Element("table");
            if (tableNode == null)
                return;
            updateCore(tableNode);
        }

        private void updateCore(HtmlNode tableNode)
        {
            var query = tableNode.Descendants("div")
                .Select(node =>
                {
                    var a = node.Element("a");
                    var divid = node.Id;
                    var divstyle = node.GetAttributeValue("style", "opacity:1.0");
                    var divclass = node.GetAttributeValue("class", "gtl");
                    var aclass = a.GetAttributeValue("class", "");
                    var state = default(TagState);
                    switch (divclass)
                    {
                    case "gt":
                        state |= TagState.HighPower; break;
                    case "gtw":
                        state |= TagState.LowPower; break;
                    case "gtl":
                    default:
                        state |= TagState.NormalPower; break;
                    }
                    switch (divstyle)
                    {
                    case "opacity:0.4":
                        state |= TagState.Slave;
                        break;
                    case "opacity:1.0":
                    default:
                        break;
                    }
                    switch (aclass)
                    {
                    case "tup":
                        state |= TagState.Upvoted;
                        break;
                    case "tdn":
                        state |= TagState.Downvoted;
                        break;
                    }
                    var tag = divid.Substring(3).Replace('_', ' ');
                    return (Tag.Parse(tag), state);
                });
            initOrReset(query);
        }

        public struct Enumerator : IEnumerator<NamespaceTagCollection>
        {
            private TagCollection parent;
            private int i;
            private int version;

            internal Enumerator(TagCollection parent)
            {
                this.parent = parent;
                this.version = parent.version;
                this.i = 0;
                this.Current = default(NamespaceTagCollection);
            }

            public NamespaceTagCollection Current { get; private set; }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (this.version != this.parent.version)
                    throw new InvalidOperationException("Collection changed");
                var offset = this.parent.offset;
                var success = this.i < this.parent.keys.Length;
                if (success)
                    this.Current = new NamespaceTagCollection(this.parent, this.i);
                else
                    this.Current = default(NamespaceTagCollection);
                this.i++;
                return success;
            }

            public void Dispose()
            {
                this.i = int.MaxValue;
                this.Current = default(NamespaceTagCollection);
                this.parent = null;
            }

            public void Reset()
            {
                this.i = 0;
                this.Current = default(NamespaceTagCollection);
            }
        }

        public Enumerator GetEnumerator()
            => new Enumerator(this);

        IEnumerator<NamespaceTagCollection> IEnumerable<NamespaceTagCollection>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int IndexOf(Tag tag)
        {
            var nsindex = getIndexOfKey(tag.Namespace);
            if (nsindex < 0)
                return -1;
            for (var i = this.offset[nsindex]; i < this.offset[nsindex + 1]; i++)
            {
                if (this.data[i].Content == tag.Content)
                    return i;
            }
            return -1;
        }

        public TagState StateOf(int index)
        {
            if (index < 0 || index >= this.data.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (this.state == null)
                return TagState.NormalPower;
            return this.state[index];
        }

        public TagState StateOf(Tag tag)
        {
            var i = IndexOf(tag);
            if (i < 0)
                return TagState.NotPresented;
            return StateOf(i);
        }

        int IList.Add(object value) => throw new NotImplementedException();

        void IList.Clear() => throw new NotImplementedException();

        bool IList.Contains(object value) => false;

        int IList.IndexOf(object value) => -1;

        void IList.Insert(int index, object value) => throw new NotImplementedException();

        void IList.Remove(object value) => throw new NotImplementedException();

        void IList.RemoveAt(int index) => throw new NotImplementedException();

        void ICollection.CopyTo(Array array, int index) => throw new NotImplementedException();
    }
}
