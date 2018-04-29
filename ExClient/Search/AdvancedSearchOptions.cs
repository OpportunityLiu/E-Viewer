using ExClient.Launch;
using System;
using System.Text;

namespace ExClient.Search
{
    public sealed class AdvancedSearchOptions : IEquatable<AdvancedSearchOptions>
    {
        private const string AdvancedSearchOptionsTag = "advsearch";

        public AdvancedSearchOptions() { }

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
            bool skipMasterTags = false,
            bool disableDefaultLanguageFilters = false,
            bool disableDefaultUploaderFilters = false)
        {
            this.SearchName = searchName;
            this.SearchTags = searchTags;
            this.SearchDescription = searchDescription;
            this.SearchTorrentFilenames = searchTorrentFilenames;
            this.GalleriesWithTorrentsOnly = galleriesWithTorrentsOnly;
            this.SearchLowPowerTags = searchLowPowerTags;
            this.SearchDownvotedTags = searchDownvotedTags;
            this.ShowExpungedGalleries = showExpungedGalleries;
            this.SearchMinimumRating = searchMinimumRating;
            this.MinimumRating = minimumRating;
            this.SkipMasterTags = skipMasterTags;
            this.DisableDefaultLanguageFilters = disableDefaultLanguageFilters;
            this.DisableDefaultUploaderFilters = disableDefaultUploaderFilters;
        }

        internal AdvancedSearchOptions(ulong data)
        {
            this.data = data;
        }

        internal string ToSearchQuery()
        {
            if (this.data == default)
            {
                return "&advsearch=1&f_sname=1&f_stags=1";
            }

            var builder = new StringBuilder(128);
            if (SkipMasterTags)
            {
                append(SkipMasterTagsTag);
            }

            append(AdvancedSearchOptionsTag);
            if (SearchName)
            {
                append(SearchNameTag);
            }

            if (SearchTags)
            {
                append(SearchTagsTag);
            }

            if (SearchDescription)
            {
                append(SearchDescriptionTag);
            }

            if (SearchTorrentFilenames)
            {
                append(SearchTorrentFilenamesTag);
            }

            if (GalleriesWithTorrentsOnly)
            {
                append(GalleriesWithTorrentsOnlyTag);
            }

            if (SearchLowPowerTags)
            {
                append(SearchLowPowerTagsTag);
            }

            if (SearchDownvotedTags)
            {
                append(SearchDownvotedTagsTag);
            }

            if (ShowExpungedGalleries)
            {
                append(ShowExpungedGalleriesTag);
            }

            if (SearchMinimumRating)
            {
                append(SearchMinimumRatingTag);
                append(MinimumRatingTag, MinimumRating);
            }
            if (DisableDefaultLanguageFilters)
            {
                append(DisableDefaultLanguageFiltersTag);
            }

            if (DisableDefaultUploaderFilters)
            {
                append(DisableDefaultUploaderFiltersTag);
            }

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
            var advanced = new AdvancedSearchOptions();
            var advancedEnabled = data.Queries.GetBoolean(AdvancedSearchOptionsTag);
            if (advancedEnabled)
            {
                advanced.SearchName = data.Queries.GetBoolean(SearchNameTag);
                advanced.SearchTags = data.Queries.GetBoolean(SearchTagsTag);
                advanced.SearchDescription = data.Queries.GetBoolean(SearchDescriptionTag);
                advanced.SearchTorrentFilenames = data.Queries.GetBoolean(SearchTorrentFilenamesTag);
                advanced.GalleriesWithTorrentsOnly = data.Queries.GetBoolean(GalleriesWithTorrentsOnlyTag);
                advanced.SearchLowPowerTags = data.Queries.GetBoolean(SearchLowPowerTagsTag);
                advanced.SearchDownvotedTags = data.Queries.GetBoolean(SearchDownvotedTagsTag);
                advanced.ShowExpungedGalleries = data.Queries.GetBoolean(ShowExpungedGalleriesTag);
                advanced.SearchMinimumRating = data.Queries.GetBoolean(SearchMinimumRatingTag);
                advanced.MinimumRating = data.Queries.GetInt32(MinimumRatingTag);
                advanced.DisableDefaultLanguageFilters = data.Queries.GetBoolean(DisableDefaultLanguageFiltersTag);
                advanced.DisableDefaultUploaderFilters = data.Queries.GetBoolean(DisableDefaultUploaderFiltersTag);
            }
            advanced.SkipMasterTags = data.Queries.GetBoolean(SkipMasterTagsTag);
            return advanced;
        }

        private ulong data;

        internal ulong Data => this.data;

        private bool getData(int pos)
        {
            unchecked
            {
                return ((this.data >> (pos + offset)) & 1UL) == 1UL;
            }
        }

        private void setData(int pos, bool value)
        {
            unchecked
            {
                pos += offset;
                if (value)
                {
                    this.data |= 1UL << pos;
                }
                else
                {
                    this.data &= ~(1UL << pos);
                }
            }
        }

        private const int offset = 2;

        private const string SearchNameTag = "f_sname";
        public bool SearchName
        {
            get => !getData(0);
            set => setData(0, !value);
        }

        private const string SearchTagsTag = "f_stags";
        public bool SearchTags
        {
            get => !getData(1);
            set => setData(1, !value);
        }

        private const string SearchDescriptionTag = "f_sdesc";
        public bool SearchDescription
        {
            get => getData(2);
            set => setData(2, value);
        }

        private const string SearchTorrentFilenamesTag = "f_storr";
        public bool SearchTorrentFilenames
        {
            get => getData(3);
            set => setData(3, value);
        }

        private const string GalleriesWithTorrentsOnlyTag = "f_sto";
        public bool GalleriesWithTorrentsOnly
        {
            get => getData(4);
            set => setData(4, value);
        }

        private const string SearchLowPowerTagsTag = "f_sdt1";
        public bool SearchLowPowerTags
        {
            get => getData(5);
            set => setData(5, value);
        }

        private const string SearchDownvotedTagsTag = "f_sdt2";
        public bool SearchDownvotedTags
        {
            get => getData(6);
            set => setData(6, value);
        }

        private const string ShowExpungedGalleriesTag = "f_sh";
        public bool ShowExpungedGalleries
        {
            get => getData(7);
            set => setData(7, value);
        }

        private const string SearchMinimumRatingTag = "f_sr";
        public bool SearchMinimumRating
        {
            get => getData(8);
            set => setData(8, value);
        }

        private const string SkipMasterTagsTag = "skip_mastertags";
        public bool SkipMasterTags
        {
            get => getData(9);
            set => setData(9, value);
        }

        private const string DisableDefaultLanguageFiltersTag = "f_sfl";
        public bool DisableDefaultLanguageFilters
        {
            get => getData(10);
            set => setData(10, value);
        }

        private const string DisableDefaultUploaderFiltersTag = "f_sfu";
        public bool DisableDefaultUploaderFilters
        {
            get => getData(11);
            set => setData(11, value);
        }

        private const string MinimumRatingTag = "f_srdd";
        public int MinimumRating
        {
            get => (int)(this.data & 0b11UL) + 2;
            set
            {
                if (value > 5)
                {
                    value = 5;
                }
                else if (value < 2)
                {
                    value = 2;
                }

                value -= 2;
                unchecked
                {
                    this.data &= ~0b11UL;
                    this.data |= (uint)value;
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is AdvancedSearchOptions other)
            {
                return this.Equals(other);
            }

            return false;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return this.data.GetHashCode();
        }

        public bool Equals(AdvancedSearchOptions other)
        {
            if (other is null)
            {
                return false;
            }

            return this.data == other.data;
        }
    }
}
