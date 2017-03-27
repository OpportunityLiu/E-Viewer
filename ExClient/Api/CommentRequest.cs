using ExClient.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient.Api
{
    internal abstract class CommentRequest : ApiRequest
    {
        public CommentRequest(Comment comment)
        {
            var gallery = comment.Owner.Owner;
            this.GalleryId = gallery.Id;
            this.GalleryToken = gallery.Token.TokenToString();
            this.Id = comment.Id;
        }

        [JsonProperty("comment_id")]
        public int Id { get; }

        [JsonProperty("gid")]
        public long GalleryId { get; }

        [JsonProperty("token")]
        public string GalleryToken { get; }
    }

    internal sealed class CommentVote : CommentRequest
    {
        public CommentVote(Comment comment, VoteCommand vote)
            : base(comment)
        {
            if(vote != VoteCommand.Down && vote != VoteCommand.Up)
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

    public enum VoteCommand
    {
        Default = 0,
        Up = 1,
        Down = -1
    }
}
