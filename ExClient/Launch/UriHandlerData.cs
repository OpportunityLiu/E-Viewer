using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ExClient.Launch
{
    internal sealed class UriHandlerData
    {
        public UriHandlerData(Uri uri)
        {
            this.Uri = uri;
            this.Paths = uri.AbsolutePath.Split(split0, StringSplitOptions.RemoveEmptyEntries);
            if(this.Paths.Count != 0)
                this.Path0 = this.Paths[0].ToLowerInvariant();
            else
                this.Path0 = "";
            this.queriesLoader = new Lazy<IReadOnlyDictionary<string, string>>(this.getQueries);
        }

        public Uri Uri { get; }
        public IReadOnlyList<string> Paths { get; }
        public string Path0 { get; }

        private Lazy<IReadOnlyDictionary<string, string>> queriesLoader;
        public IReadOnlyDictionary<string, string> Queries => this.queriesLoader.Value;

        private static readonly char[] split0 = "/".ToCharArray();
        private static readonly char[] split1 = "&".ToCharArray();
        private static readonly char[] split2 = "=".ToCharArray();
        private static IReadOnlyDictionary<string, string> empty = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

        private IReadOnlyDictionary<string, string> getQueries()
        {
            var query = this.Uri.Query;
            if(string.IsNullOrWhiteSpace(query) || query.Length <= 1 || query[0] != '?')
                return empty;
            query = query.Substring(1);
            var divided = query.Split(split1, StringSplitOptions.RemoveEmptyEntries);
            return new ReadOnlyDictionary<string, string>((from item in divided
                                                           select item.Split(split2, 2, StringSplitOptions.None))
                   .ToDictionary(i => i[0], i => i[1].Unescape()));
        }
    }
}
