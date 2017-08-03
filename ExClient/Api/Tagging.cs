using ExClient.Tagging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ExClient.Api
{
    internal sealed class TagRequest : GalleryRequest, IRequestOf<TagRequest, TagResponse>
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

    internal class TagResponse : ApiResponse, IResponseOf<TagRequest, TagResponse>
    {
        [JsonProperty("tagpane")]
        public string TagPane { get; set; }
    }
}
