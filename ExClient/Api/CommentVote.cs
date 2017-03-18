using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient.Api
{
    internal class CommentVote : ApiRequest
    {
        public CommentVote(Comment comment, VoteCommand vote)
        {
            if(vote != VoteCommand.Down && vote != VoteCommand.Up)
                throw new ArgumentOutOfRangeException(nameof(vote));
            var gallery = comment.Owner.Owner;
            this.GalleryId = gallery.Id;
            this.GalleryToken = gallery.Token;
            this.Id = comment.Id;
            this.Vote = vote;
        }

        public override string Method => "votecomment";

        [JsonProperty("comment_vote")]
        public VoteCommand Vote { get; }

        [JsonProperty("comment_id")]
        public int Id { get; }

        [JsonProperty("gid")]
        public long GalleryId { get; }

        [JsonProperty("token")]
        public string GalleryToken { get; }
    }

    public enum VoteCommand
    {
        Default = 0,
        Up = 1,
        Down = -1
    }
}
