using ExClient.Api;

namespace ExClient.Galleries.Commenting
{
    public enum CommentStatus
    {
        None = 0b0000,
        Votable = 0b0001,
        VotedUp = 0b0011,
        VotedDown = 0b0101,
        Editable = 0b1000
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
