using System;

namespace ExClient.HentaiVerse
{
    public sealed class MonsterEncounteredEventArgs : EventArgs
    {
        internal MonsterEncounteredEventArgs(Uri uri)
        {
            this.Uri = uri;
        }

        public Uri Uri { get; }
    }
}
