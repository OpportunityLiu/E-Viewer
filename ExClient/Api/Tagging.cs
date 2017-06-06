using ExClient.Tagging;
using ExClient.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient.Api
{
    internal sealed class TagRequest : GalleryRequest
    {
        private TagRequest(TagCollection collection, string tags, VoteCommand vote)
            : base(collection.Owner)
        {
            this.Vote = vote;
            if (tags.Length >= 200)
                throw new ArgumentException(LocalizedStrings.Resources.TagsTooLong, nameof(tags));
            this.Tags = tags;
        }

        public TagRequest(TagCollection collection, IEnumerable<Tag> tags, VoteCommand vote)
            : this(collection, string.Join(",", tags), vote) { }

        public TagRequest(TagCollection collection, Tag tag, VoteCommand vote)
            : this(collection, tag.ToString(), vote) { }

        [JsonProperty("vote")]
        public VoteCommand Vote { get; }

        [JsonProperty("tags")]
        public string Tags { get; }

        public override string Method => "taggallery";
    }

    internal class TagResponse : ApiResponse
    {
        [JsonProperty("tagpane")]
        public string TagPane { get; set; }
    }
}
