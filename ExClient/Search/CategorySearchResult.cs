using ExClient.Launch;
using System;
using System.Collections.Generic;
using Windows.Web.Http;

namespace ExClient.Search
{
    public abstract class CategorySearchResult : SearchResult
    {
        private static readonly SearchHandlerBase[] _Handlers = new SearchHandlerBase[]
        {
            SearchHandler.Instance,
            SearchCategoryHandler.Instance,
            SearchUploaderAndTagHandler.Instance,
            WatchedHandler.Instance,
        };

        public static bool TryParse(Uri uri, out CategorySearchResult result)
        {
            result = default;
            if (uri is null)
                return false;
            var data = new UriHandlerData(uri);
            foreach (var handler in _Handlers)
            {
                if (!handler.CanHandle(data))
                    continue;
                result = (CategorySearchResult)handler.Handle(data).Data;
                return true;
            }
            return false;
        }

        public static CategorySearchResult Parse(Uri uri)
        {
            if (TryParse(uri, out var r))
                return r;
            throw new FormatException($"Failed to parse uri `{uri}` as CategorySearchResult");
        }

        public static Uri SearchBaseUri => Client.Current.Uris.RootUri;

        public static readonly Category DefaultFliter = Category.All;

        protected CategorySearchResult(string keyword, Category category)
            : base(keyword)
        {
            if (category == Category.Unspecified)
            {
                category = DefaultFliter;
            }

            Category = category;
        }

        public Category Category { get; }

        public override Uri SearchUri => new Uri(SearchBaseUri, $"?{new HttpFormUrlEncodedContent(_GetUriQuery())}");

        private IEnumerable<KeyValuePair<string, string>> _GetUriQuery()
        {
            yield return new KeyValuePair<string, string>("f_search", Keyword);
            yield return new KeyValuePair<string, string>("f_cats", (Category.All - Category).ToString());
        }
    }
}
