using ExClient.Search;
using Opportunity.Helpers.Universal.AsyncHelpers;
using System.Collections.Generic;
using Windows.Foundation;

namespace ExClient.Launch
{
    internal abstract class SearchHandlerBase : UriHandler
    {
        protected static string UnescapeKeyword(string query)
        {
            return query.Replace("+", "").Replace("&", "");
        }

        protected static AdvancedSearchOptions GetAdvancedSearchOptions(UriHandlerData data)
            => AdvancedSearchOptions.ParseUri(data);

        public abstract SearchLaunchResult Handle(UriHandlerData data);

        public override IAsyncOperation<LaunchResult> HandleAsync(UriHandlerData data)
            => AsyncOperation<LaunchResult>.CreateCompleted(Handle(data));

        protected static Category GetCategory(UriHandlerData data)
        {
            var categoryVal = data.Queries.GetInt32("f_cats");
            if (categoryVal < 0) categoryVal = 0;
            if (categoryVal > (int)Category.All) categoryVal = (int)Category.All;
            return (Category)((int)Category.All - categoryVal);
        }

        protected static string GetKeyword(UriHandlerData data)
        {
            return UnescapeKeyword(data.Queries.GetString("f_search") ?? "");
        }
    }
}
