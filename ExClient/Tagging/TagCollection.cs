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
                    continue;
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
            RaiseCollectionReset();
            RaisePropertyChanged(nameof(Count), nameof(Items), "Groups");
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
        public RangedCollectionView<GalleryTag> this[Namespace key]
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

        private RangedCollectionView<GalleryTag> getValue(Namespace key)
        {
            var i = getIndexOfKey(key);
            if (i < 0)
            {
                if (key.IsDefined())
                    return RangedCollectionView<GalleryTag>.Empty;
                else
                    throw new ArgumentOutOfRangeException(nameof(key));
            }
            return getValue(i);
        }

        private RangedCollectionView<GalleryTag> getValue(int index)
        {
            return new RangedCollectionView<GalleryTag>(this.Data, this.Offset[index], this.Offset[index + 1] - this.Offset[index]);
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
        private static Regex tagNeedNs = new Regex(@"The tag ""(.+?)"" is not allowed in this namespace - requires (.+:)");
        private static string[] tagNeedNsSplit = new[] { "or", ",", " " };
        private static Regex tagInBlackList = new Regex(@"The tag (.+?) cannot be used");
        private static Regex tagVetoed = new Regex(@"The tag (.+?) has been vetoed on this gallery");
        private static Regex tagCantVote = new Regex(@"Cannot vote for tag");

        private static void myCheckResponse(TagResponse r)
        {
            if (r.Error == null && r.LogIn == null)
                return;
            var validMatch = tagNotValid.Match(r.Error);
            if (validMatch.Success)
            {
                throw new InvalidOperationException(string.Format(LocalizedStrings.Resources.TagNotValid, validMatch.Groups[1].Value));
            }
            var needNsMatch = tagNeedNs.Match(r.Error);
            if (needNsMatch.Success)
            {
                var ns = needNsMatch.Groups[2].Value.Split(tagNeedNsSplit, StringSplitOptions.RemoveEmptyEntries);
                var tag = needNsMatch.Groups[1].Value;
                switch (ns.Length)
                {
                case 1:
                    throw new InvalidOperationException(string.Format(LocalizedStrings.Resources.TagNeedNamespace1, tag, ns[0]));
                case 2:
                    throw new InvalidOperationException(string.Format(LocalizedStrings.Resources.TagNeedNamespace2, tag, ns[0], ns[1]));
                case 3:
                    throw new InvalidOperationException(string.Format(LocalizedStrings.Resources.TagNeedNamespace3, tag, ns[0], ns[1], ns[2]));
                case 4:
                    throw new InvalidOperationException(string.Format(LocalizedStrings.Resources.TagNeedNamespace4, tag, ns[0], ns[1], ns[2], ns[3]));
                case 5:
                    throw new InvalidOperationException(string.Format(LocalizedStrings.Resources.TagNeedNamespace5, tag, ns[0], ns[1], ns[2], ns[3], ns[4]));
                case 6:
                    throw new InvalidOperationException(string.Format(LocalizedStrings.Resources.TagNeedNamespace6, tag, ns[0], ns[1], ns[2], ns[3], ns[4], ns[5]));
                case 7:
                    throw new InvalidOperationException(string.Format(LocalizedStrings.Resources.TagNeedNamespace7, tag, ns[0], ns[1], ns[2], ns[3], ns[4], ns[5], ns[6]));
                case 8:
                    throw new InvalidOperationException(string.Format(LocalizedStrings.Resources.TagNeedNamespace8, tag, ns[0], ns[1], ns[2], ns[3], ns[4], ns[5], ns[6], ns[7]));
                }
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
            r.CheckResponse();
        }

        private IAsyncAction voteAsync(TagRequest req)
        {
            return AsyncInfo.Run(async token =>
            {
                var res = await req.GetResponseAsync(true);
                var doc = HtmlNode.CreateNode(res.TagPane);
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

        public int IndexOf(GalleryTag tag)
        {
            var nsindex = getIndexOfKey(tag.Content.Namespace);
            if (nsindex < 0)
                return -1;
            for (var i = this.Offset[nsindex]; i < this.Offset[nsindex + 1]; i++)
            {
                if (this.Data[i].Content == tag.Content)
                    return i;
            }
            return -1;
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
