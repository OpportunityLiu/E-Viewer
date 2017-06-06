using ExClient.Commenting;
using ExClient.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient.Api
{
    internal abstract class CommentRequest : GalleryRequest
    {
        public CommentRequest(Comment comment)
            : base(comment.Owner.Owner)
        {
            var gallery = comment.Owner.Owner;
            this.Id = comment.Id;
        }

        [JsonProperty("comment_id")]
        public int Id { get; }
    }

    internal sealed class CommentVote : CommentRequest
    {
        public CommentVote(Comment comment, VoteCommand vote)
            : base(comment)
        {
            if (vote != VoteCommand.Down && vote != VoteCommand.Up)
                throw new ArgumentOutOfRangeException(nameof(vote));
            this.Vote = vote;
        }

        public override string Method => "votecomment";

        [JsonProperty("comment_vote")]
        public VoteCommand Vote { get; }
    }

    internal sealed class CommentEdit : CommentRequest
    {
        public CommentEdit(Comment comment)
            : base(comment)
        {
        }

        public override string Method => "geteditcomment";
    }

    internal class CommentResponse : ApiResponse
    {
        [JsonProperty("comment_id")]
        public int Id { get; set; }

    }

    internal class CommentVoteResponse : CommentResponse
    {
        [JsonProperty("comment_score")]
        public int Score { get; set; }
        [JsonProperty("comment_vote")]
        public VoteCommand Vote { get; set; }
    }

    internal class CommentEditResponse : CommentResponse
    {
        [JsonProperty("editable_comment")]
        public string Editable { get; set; }
    }
}
