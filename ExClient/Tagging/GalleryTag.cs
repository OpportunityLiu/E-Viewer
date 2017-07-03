using Opportunity.MvvmUniverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
