using ExClient.Launch;

using System.Text;

namespace ExClient.Search
{
    public sealed class AdvancedSearchOptions
    {
        public AdvancedSearchOptions(
            bool searchName = true,
            bool searchTags = true,
            bool searchDescription = false,
            bool searchTorrentFilenames = false,
            bool galleriesWithTorrentsOnly = false,
            bool searchLowPowerTags = false,
            bool searchDownvotedTags = false,
            bool showExpungedGalleries = false,
            bool searchMinimumRating = false,
            int minimumRating = 2,
            bool searchPageCount = false,
            int minimumPageCount = 1,
            int maximumPageCount = 2000,
            bool disableDefaultLanguageFilters = false,
            bool disableDefaultUploaderFilters = false,
            bool disableDefaultTagsFilters = false)
        {
            SearchName = searchName;
            SearchTags = searchTags;
            SearchDescription = searchDescription;
            SearchTorrentFilenames = searchTorrentFilenames;
            GalleriesWithTorrentsOnly = galleriesWithTorrentsOnly;
            SearchLowPowerTags = searchLowPowerTags;
            SearchDownvotedTags = searchDownvotedTags;
            ShowExpungedGalleries = showExpungedGalleries;
            SearchMinimumRating = searchMinimumRating;
            MinimumRating = minimumRating;
            SearchPageCount = searchPageCount;
            MinimumPageCount = minimumPageCount;
            MaximumPageCount = maximumPageCount;
            DisableDefaultLanguageFilters = disableDefaultLanguageFilters;
            DisableDefaultUploaderFilters = disableDefaultUploaderFilters;
            DisableDefaultTagsFilters = disableDefaultTagsFilters;
        }

        public AdvancedSearchOptions Clone()
        {
            return (AdvancedSearchOptions)MemberwiseClone();
        }

        private const string ADVANCED_SEARCH_OPTIONS_TAG = "&advsearch=1";
        internal string ToSearchQuery()
        {
            var builder = new StringBuilder(ADVANCED_SEARCH_OPTIONS_TAG, 128);

            if (SearchName)
                append(_SearchNameTag);

            if (SearchTags)
                append(_SearchTagsTag);

            if (SearchDescription)
                append(_SearchDescriptionTag);

            if (SearchTorrentFilenames)
                append(_SearchTorrentFilenamesTag);

            if (GalleriesWithTorrentsOnly)
                append(_GalleriesWithTorrentsOnlyTag);

            if (SearchLowPowerTags)
                append(_SearchLowPowerTagsTag);

            if (SearchDownvotedTags)
                append(_SearchDownvotedTagsTag);

            if (ShowExpungedGalleries)
                append(_ShowExpungedGalleriesTag);

            if (SearchMinimumRating)
            {
                append(_SearchMinimumRatingTag);
                append(_MinimumRatingTag, MinimumRating);
            }

            if (SearchPageCount)
            {
                append(_SearchPageCountTag);
                append(_MinimumPageCountTag, MinimumPageCount);
                append(_MaximumPageCountTag, MaximumPageCount);
            }

            if (DisableDefaultLanguageFilters)
                append(_DisableDefaultLanguageFiltersTag);

            if (DisableDefaultUploaderFilters)
                append(_DisableDefaultUploaderFiltersTag);

            if (DisableDefaultTagsFilters)
                append(_DisableDefaultTagsFiltersTag);

            return builder.ToString();
            void append(string key, int value = 1)
            {
                builder
                    .Append('&')
                    .Append(key)
                    .Append('=')
                    .Append(value);
            }
        }

        internal static AdvancedSearchOptions ParseUri(UriHandlerData data)
        {
            if (data.Queries.GetBoolean("advsearch"))
            {
                return new AdvancedSearchOptions(
                    searchName: data.Queries.GetBoolean(_SearchNameTag),
                    searchTags: data.Queries.GetBoolean(_SearchTagsTag),
                    searchDescription: data.Queries.GetBoolean(_SearchDescriptionTag),
                    searchTorrentFilenames: data.Queries.GetBoolean(_SearchTorrentFilenamesTag),
                    galleriesWithTorrentsOnly: data.Queries.GetBoolean(_GalleriesWithTorrentsOnlyTag),
                    searchLowPowerTags: data.Queries.GetBoolean(_SearchLowPowerTagsTag),
                    searchDownvotedTags: data.Queries.GetBoolean(_SearchDownvotedTagsTag),
                    showExpungedGalleries: data.Queries.GetBoolean(_ShowExpungedGalleriesTag),
                    searchMinimumRating: data.Queries.GetBoolean(_SearchMinimumRatingTag),
                    minimumRating: data.Queries.GetInt32(_MinimumRatingTag),
                    searchPageCount: data.Queries.GetBoolean(_SearchPageCountTag),
                    minimumPageCount: data.Queries.GetInt32(_MinimumPageCountTag),
                    maximumPageCount: data.Queries.GetInt32(_MaximumPageCountTag),
                    disableDefaultLanguageFilters: data.Queries.GetBoolean(_DisableDefaultLanguageFiltersTag),
                    disableDefaultUploaderFilters: data.Queries.GetBoolean(_DisableDefaultUploaderFiltersTag),
                    disableDefaultTagsFilters: data.Queries.GetBoolean(_DisableDefaultTagsFiltersTag)
                );
            }
            else
                return new AdvancedSearchOptions();
        }

        private const string _SearchNameTag = "f_sname";
        public bool SearchName { get; set; }

        private const string _SearchTagsTag = "f_stags";
        public bool SearchTags { get; set; }

        private const string _SearchDescriptionTag = "f_sdesc";
        public bool SearchDescription { get; set; }

        private const string _SearchTorrentFilenamesTag = "f_storr";
        public bool SearchTorrentFilenames { get; set; }

        private const string _GalleriesWithTorrentsOnlyTag = "f_sto";
        public bool GalleriesWithTorrentsOnly { get; set; }

        private const string _SearchLowPowerTagsTag = "f_sdt1";
        public bool SearchLowPowerTags { get; set; }

        private const string _SearchDownvotedTagsTag = "f_sdt2";
        public bool SearchDownvotedTags { get; set; }

        private const string _ShowExpungedGalleriesTag = "f_sh";
        public bool ShowExpungedGalleries { get; set; }

        private const string _SearchMinimumRatingTag = "f_sr";
        public bool SearchMinimumRating { get; set; }

        private const string _MinimumRatingTag = "f_srdd";
        private int _MinimumRating = 2;
        public int MinimumRating
        {
            get => _MinimumRating;
            set
            {
                if (value > 5)
                    value = 5;
                else if (value < 2)
                    value = 2;
                _MinimumRating = value;
            }
        }

        private const string _SearchPageCountTag = "f_sp";
        public bool SearchPageCount { get; set; }

        private const string _MinimumPageCountTag = "f_spf";
        private int _MinimumPageCount = 1;
        public int MinimumPageCount
        {
            get => _MinimumPageCount;
            set
            {
                if (value < 1)
                    value = 1;
                else if (value > 2000)
                    value = 2000;
                _MinimumPageCount = value;
            }
        }

        private const string _MaximumPageCountTag = "f_spt";
        private int _MaximumPageCount = 2000;
        public int MaximumPageCount
        {
            get => _MaximumPageCount;
            set
            {
                if (value < 1)
                    value = 1;
                else if (value > 2000)
                    value = 2000;
                _MaximumPageCount = value;
            }
        }

        private const string _DisableDefaultLanguageFiltersTag = "f_sfl";
        public bool DisableDefaultLanguageFilters { get; set; }

        private const string _DisableDefaultUploaderFiltersTag = "f_sfu";
        public bool DisableDefaultUploaderFilters { get; set; }

        private const string _DisableDefaultTagsFiltersTag = "f_sft";
        public bool DisableDefaultTagsFilters { get; set; }
    }
}
