using Opportunity.MvvmUniverse;

using Windows.Foundation;

namespace ExClient.Tagging
{
    [System.Diagnostics.DebuggerDisplay(@"({_State}){Content}")]
    public sealed class GalleryTag : ObservableObject
    {
        internal GalleryTag(TagCollection owner, Tag content, TagState state)
        {
            _Owner = owner;
            Content = content;
            _State = state;
        }

        private readonly TagCollection _Owner;

        public Tag Content { get; }

        private TagState _State;
        public TagState State { get => _State; internal set => Set(ref _State, value); }

        public IAsyncAction VoteAsync(Api.VoteState command)
        {
            if (command == Api.VoteState.Default)
            {
                if (_State.HasFlag(TagState.Downvoted))
                {
                    return _Owner.VoteAsync(Content, Api.VoteState.Up);
                }
                else if (_State.HasFlag(TagState.Upvoted))
                {
                    return _Owner.VoteAsync(Content, Api.VoteState.Down);
                }
            }
            return _Owner.VoteAsync(Content, command);
        }
    }
}
