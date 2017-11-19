using ExClient.Tagging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ExClient.Api
{
    internal sealed class TagRequest : GalleryRequest<TagResponse>
    {
        private TagRequest(TagCollection collection, string tags, VoteState vote)
            : base(collection.Owner)
        {
            this.Vote = vote;
            if (tags.Length >= 200)
                throw new ArgumentException(LocalizedStrings.Resources.TagsTooLong, nameof(tags));
            this.Tags = tags;
        }

        public TagRequest(TagCollection collection, IEnumerable<Tag> tags, VoteState vote)
            : this(collection, string.Join(",", tags), vote) { }

        public TagRequest(TagCollection collection, Tag tag, VoteState vote)
            : this(collection, tag.ToString(), vote) { }

        [JsonProperty("vote")]
        public VoteState Vote { get; }

        [JsonProperty("tags")]
        public string Tags { get; }

        public override string Method => "taggallery";
    }

    internal class TagResponse : ApiResponse
    {
        [JsonProperty("tagpane")]
        public string TagPane { get; set; }

        // See https://ehwiki.org/wiki/Technical_Issues#Gallery_Tagging
        // Here are mostly used ones.
        private static Regex tagNotValid = new Regex(@"The tag (.+?) is not currently valid");
        private static Regex tagNeedNs = new Regex(@"The tag ""(.+?)"" is not allowed\. Use (.+)");
        private static string[] tagNeedNsSplit = new[] { "or", "," };
        private static Regex tagInBlackList = new Regex(@"The tag (.+?) cannot be used");
        private static Regex tagVetoed = new Regex(@"The tag (.+?) has been vetoed on this gallery");
        private static Regex tagCantVote = new Regex(@"Cannot vote for tag");
        private static Regex tagsEmpty = new Regex(@"No tags to add\.");

        protected override void CheckResponseOverride(ApiRequest request)
        {
            if (Error == null)
                return;
            var validMatch = tagNotValid.Match(Error);
            if (validMatch.Success)
            {
                throw new InvalidOperationException(string.Format(LocalizedStrings.Resources.TagNotValid, validMatch.Groups[1].Value));
            }
            var needNsMatch = tagNeedNs.Match(Error);
            if (needNsMatch.Success)
            {
                var ns = needNsMatch.Groups[2].Value.Split(tagNeedNsSplit, StringSplitOptions.RemoveEmptyEntries);
                for (var i = 0; i < ns.Length; i++)
                    ns[i] = ns[i].Trim();
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
            var vetoedMatch = tagVetoed.Match(Error);
            if (vetoedMatch.Success)
            {
                throw new InvalidOperationException(string.Format(LocalizedStrings.Resources.TagVetoedForGallery, vetoedMatch.Groups[1].Value));
            }
            var blacklistMatch = tagInBlackList.Match(Error);
            if (blacklistMatch.Success)
            {
                throw new InvalidOperationException(string.Format(LocalizedStrings.Resources.TagInBlackList, blacklistMatch.Groups[1].Value));
            }
            if (tagCantVote.IsMatch(Error))
                throw new InvalidOperationException(LocalizedStrings.Resources.TagNoVotePremition);
            if (tagsEmpty.IsMatch(Error))
                throw new InvalidOperationException(LocalizedStrings.Resources.TagVoteCollectionEmpty);
        }
    }
}
