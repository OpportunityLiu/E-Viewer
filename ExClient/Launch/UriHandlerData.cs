using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Windows.Foundation;

namespace ExClient.Launch
{
    internal sealed class UriHandlerData
    {
        public UriHandlerData(Uri uri)
        {
            this.Uri = uri ?? throw new ArgumentNullException(nameof(uri));
            this.Paths = uri.Segments.Skip(1)
                .Select(s => s.EndsWith("/") ? s.Substring(0, s.Length - 1) : s)
                .ToArray();
            if (this.Paths.Count != 0)
                this.Path0 = this.Paths[0].ToLowerInvariant();
            else
                this.Path0 = "";
        }

        public Uri Uri { get; }
        public IReadOnlyList<string> Paths { get; }
        public string Path0 { get; }

        private WwwFormUrlDecoder queries;
        public WwwFormUrlDecoder Queries
            => LazyInitializer.EnsureInitialized(ref this.queries, () => new WwwFormUrlDecoder(Uri.Query.CoalesceNullOrWhiteSpace("?")));
    }
}
