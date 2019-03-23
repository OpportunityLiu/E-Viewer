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
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
            Paths = uri.Segments.Skip(1)
                .Select(s => s.EndsWith("/") ? s.Substring(0, s.Length - 1) : s)
                .ToArray();
            if (Paths.Count != 0)
                Path0 = Paths[0].ToLowerInvariant();
            else
                Path0 = "";
        }

        public Uri Uri { get; }
        public IReadOnlyList<string> Paths { get; }
        public string Path0 { get; }

        private WwwFormUrlDecoder queries;
        public WwwFormUrlDecoder Queries
            => LazyInitializer.EnsureInitialized(ref queries, () => new WwwFormUrlDecoder(Uri.Query.CoalesceNullOrWhiteSpace("?")));
    }
}
