using ExClient.Api;

namespace ExClient.Commenting
{
    public enum CommentStatus
    {
        None,
        Votable,
        VotedUp,
        VotedDown,
        Editable
    }

    public static class CommentStatusExtension
    {
        public static VoteState AsVoteState(this CommentStatus status)
        {
            switch (status)
            {
            case CommentStatus.VotedUp:
                return VoteState.Up;
            case CommentStatus.VotedDown:
                return VoteState.Down;
            default:
                return VoteState.Default;
            }
        }
    }
}
