using Opportunity.MvvmUniverse;

using Windows.Foundation;

namespace ExClient.Tagging
{
    public sealed class GalleryTag : ObservableObject
    {
        internal GalleryTag(TagCollection owner, Tag content, TagState state)
        {
            this.owner = owner;
            Content = content;
            this.state = state;
        }

        private readonly TagCollection owner;

        public Tag Content { get; }

        private TagState state;
        public TagState State { get => state; internal set => Set(ref state, value); }

        public IAsyncAction VoteAsync(Api.VoteState command)
        {
            if (command == Api.VoteState.Default)
            {
                if (state.HasFlag(TagState.Downvoted))
                {
                    return owner.VoteAsync(Content, Api.VoteState.Up);
                }
                else if (state.HasFlag(TagState.Upvoted))
                {
                    return owner.VoteAsync(Content, Api.VoteState.Down);
                }
            }
            return owner.VoteAsync(Content, command);
        }
    }
}
