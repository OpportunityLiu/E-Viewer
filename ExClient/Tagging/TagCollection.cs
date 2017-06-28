using ExClient.Api;
using ExClient.Galleries;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Opportunity.MvvmUniverse.AsyncHelpers;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            for (var i = 0; i < this.Keys.Length; i++)
            {
                if (this.Keys[i] == key)
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

        internal event Action<int> ItemStateChanged;

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
            if (this.Data != null && this.State != null && this.Data.SequenceEqual(data))
            {
                var notify = this.ItemStateChanged;
                if (notify == null)
                {
                    this.State = state;
                    return;
                }
                for (var i = 0; i < this.State.Length; i++)
                {
                    var oldState = this.State[i];
                    var newState = state[i];
                    if (oldState == newState)
                        continue;
                    this.State[i] = newState;
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
            this.Data = data;
            this.State = state;
            this.Keys = keys;
            this.Offset = offset;
            this.Version++;
            this.ItemStateChanged?.Invoke(-1);
            RaiseCollectionReset();
            RaisePropertyChanged(nameof(Count), nameof(Items), "Groups");
        }

        internal Tag[] Data;
        internal TagState[] State;

        internal Namespace[] Keys;
        internal int[] Offset;

        internal int Version;

        public Gallery Owner { get; }

        public IReadOnlyList<Tag> Items => this.Data;

        public int Count => this.Keys.Length;

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
            return new RangedCollectionView<Tag>(this.Data, this.Offset[index], this.Offset[index + 1] - this.Offset[index]);
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

        // See https://ehwiki.org/wiki/Technical_Issues#Gallery_Tagging
        // Here are mostly used ones.
        private static Regex tagNotValid = new Regex(@"The tag (.+?) is not currently valid");
        private static Regex tagNeedNs = new Regex(@"The tag ""(.+?)"" is not allowed in this namespace - requires (.+?) or (.+?:)");
        private static Regex tagNeedNs1 = new Regex(@"The tag ""(.+?)"" is not allowed in this namespace - requires (.+?:)");
        private static Regex tagInBlackList = new Regex(@"The tag (.+?) cannot be used");
        private static Regex tagVetoed = new Regex(@"The tag (.+?) has been vetoed on this gallery");
        private static Regex tagCantVote = new Regex(@"Cannot vote for tag");

        private IAsyncAction voteAsync(TagRequest req)
        {
            return AsyncInfo.Run(async token =>
            {
                var res = await Client.Current.HttpClient.PostApiAsync(req);
                var r = JsonConvert.DeserializeObject<TagResponse>(res);
                if (r.Error != null)
                {
                    var validMatch = tagNotValid.Match(r.Error);
                    if (validMatch.Success)
                    {
                        throw new InvalidOperationException(string.Format(LocalizedStrings.Resources.TagNotValid, validMatch.Groups[1].Value));
                    }
                    var needNsMatch = tagNeedNs.Match(r.Error);
                    if (needNsMatch.Success)
                    {
                        throw new InvalidOperationException(string.Format(LocalizedStrings.Resources.TagNeedNamespaceMul, needNsMatch.Groups[1].Value, needNsMatch.Groups[2].Value, needNsMatch.Groups[3].Value));
                    }
                    var needNsMatch1 = tagNeedNs1.Match(r.Error);
                    if (needNsMatch1.Success)
                    {
                        throw new InvalidOperationException(string.Format(LocalizedStrings.Resources.TagNeedNamespace, needNsMatch1.Groups[1].Value, needNsMatch1.Groups[2].Value));
                    }
                    var vetoedMatch = tagVetoed.Match(r.Error);
                    if (vetoedMatch.Success)
                    {
                        throw new InvalidOperationException(string.Format(LocalizedStrings.Resources.TagVetoedForGallery, vetoedMatch.Groups[1].Value));
                    }
                    var blacklistMatch = tagInBlackList.Match(r.Error);
                    if (blacklistMatch.Success)
                    {
                        throw new InvalidOperationException(string.Format(LocalizedStrings.Resources.TagInBlackList, blacklistMatch.Groups[1].Value));
                    }
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
                this.version = parent.Version;
                this.i = 0;
                this.Current = default(NamespaceTagCollection);
            }

            public NamespaceTagCollection Current { get; private set; }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (this.version != this.parent.Version)
                    throw new InvalidOperationException("Collection changed");
                var offset = this.parent.Offset;
                var success = this.i < this.parent.Keys.Length;
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
            for (var i = this.Offset[nsindex]; i < this.Offset[nsindex + 1]; i++)
            {
                if (this.Data[i].Content == tag.Content)
                    return i;
            }
            return -1;
        }

        public TagState StateOf(int index)
        {
            if (index < 0 || index >= this.Data.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (this.State == null)
                return TagState.NormalPower;
            return this.State[index];
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
