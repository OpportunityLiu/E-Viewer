using ExClient.Tagging;
using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using Windows.Foundation;

namespace ExClient.Launch
{
    internal sealed class SearchUploaderAndTagHandler : SearchHandlerBase
    {
        /// <summary>
        /// Unescape twice for special usage.
        /// </summary>
        /// <param name="value">string to unescape</param>
        /// <returns>unescaped string</returns>
        private static string unescape2(string value)
        {
            value = Uri.UnescapeDataString(value);
            value = value.Replace('+', ' ');
            value = Uri.UnescapeDataString(value);
            return value.Trim();
        }
        private static readonly char[] trim = new[] { '$' };

        public override bool CanHandle(UriHandlerData data)
        {
            if (data.Paths.Count != 2)
            {
                return false;
            }

            switch (data.Path0)
            {
            case "tag":
                return Tag.TryParse(unescape2(data.Paths[1]).TrimEnd(trim), out _);
            case "uploader":
                return true;
            default:
                return false;
            }
        }

        public override IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data)
        {
            var v = unescape2(data.Paths[1]);
            var category = GetCategory(data);
            var advanced = GetAdvancedSearchOptions(data);
            switch (data.Path0)
            {
            case "tag":
                return AsyncOperation<LaunchResult>.CreateCompleted(new SearchLaunchResult(Tag.Parse(v.TrimEnd(trim)).Search(category, advanced)));
            case "uploader":
                return AsyncOperation<LaunchResult>.CreateCompleted(new SearchLaunchResult(Client.Current.Search(v, null, category, advanced)));
            }
            return AsyncOperation<LaunchResult>.CreateFault(new NotSupportedException("Unsupported uri."));
        }
    }
}
