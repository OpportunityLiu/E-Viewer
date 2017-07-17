using System;

namespace ExClient.Search
{
    public sealed class AdvancedSearchResult : CategorySearchResult
    {
        internal static AdvancedSearchResult Search(string keyword, Category category, AdvancedSearchOptions advancedSearch)
        {
            return new AdvancedSearchResult(keyword, category, advancedSearch);
        }

        private string getQueryString()
        {
            var adv = new AdvancedSearchOptions(this.advSearchData);
            return $"&advsearch=1" +
                $"{(adv.SearchName ? "&f_sname=1" : "")}" +
                $"{(adv.SearchTags ? "&f_stags=1" : "")}" +
                $"{(adv.SearchDescription ? "&f_sdesc=1" : "")}" +
                $"{(adv.SearchTorrentFilenames ? "&f_storr=1" : "")}" +
                $"{(adv.GalleriesWithTorrentsOnly ? "&f_sto=1" : "")}" +
                $"{(adv.SearchLowPowerTags ? "&f_sdt1=1" : "")}" +
                $"{(adv.SearchDownvotedTags ? "&f_sdt2=1" : "")}" +
                $"{(adv.ShowExpungedGalleries ? "&f_sh=1" : "")}" +
                $"{(adv.SearchMinimumRating ? "&f_sr=1&f_srdd=" + adv.MinimumRating.ToString() : "")}";
        }

        private AdvancedSearchResult(string keyword, Category category, AdvancedSearchOptions advancedSearch)
            : base(keyword, category)
        {
            if (advancedSearch != null)
                this.advSearchData = advancedSearch.Data;
            this.SearchUri = this.advSearchData == default(ushort)
                ? base.SearchUri
                : new Uri(base.SearchUri.OriginalString + getQueryString());
        }

        private readonly ushort advSearchData;

        public AdvancedSearchOptions AdvancedSearch => new AdvancedSearchOptions(this.advSearchData);

        public override Uri SearchUri { get; }
    }
}
