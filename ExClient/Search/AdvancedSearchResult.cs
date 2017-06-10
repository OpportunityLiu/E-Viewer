using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace ExClient.Search
{
    public sealed class AdvancedSearchResult : CategorySearchResult
    {
        internal static AdvancedSearchResult Search(string keyword, Category category, AdvancedSearchOptions advancedSearch)
        {
            return new AdvancedSearchResult(keyword, category, advancedSearch);
        }

        public override Uri SearchUri { get; }

        private AdvancedSearchResult(string keyword, Category category, AdvancedSearchOptions advancedSearch)
            : base(keyword, category)
        {
            this.advSearchData = advancedSearch.Data;
            this.SearchUri = new Uri($"{base.SearchUri.OriginalString}&{new HttpFormUrlEncodedContent(this.AdvancedSearch.AsEnumerable())}");
        }

        private readonly ushort advSearchData;
        public AdvancedSearchOptions AdvancedSearch => new AdvancedSearchOptions(advSearchData);
    }
}
