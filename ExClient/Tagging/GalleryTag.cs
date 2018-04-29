using Opportunity.MvvmUniverse;
using Windows.Foundation;

namespace ExClient.Tagging
{
    public sealed class GalleryTag : ObservableObject
    {
        internal GalleryTag(TagCollection owner, Tag content, TagState state)
        {
            this.owner = owner;
            this.Content = content;
            this.state = state;
        }

        private readonly TagCollection owner;

        public Tag Content { get; }

        private TagState state;
        public TagState State { get => this.state; internal set => Set(ref this.state, value); }

        public IAsyncAction VoteAsync(Api.VoteState command)
        {
            if (command == Api.VoteState.Default)
            {
                if (this.state.HasFlag(TagState.Downvoted))
                {
                    return this.owner.VoteAsync(this.Content, Api.VoteState.Up);
                }
                else if (this.state.HasFlag(TagState.Upvoted))
                {
                    return this.owner.VoteAsync(this.Content, Api.VoteState.Down);
                }
            }
            return this.owner.VoteAsync(this.Content, command);
        }
    }
}
