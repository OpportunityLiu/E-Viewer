using System;

namespace ExClient.Search
{
    //advsearch=1
    public sealed class AdvancedSearchOptions : IEquatable<AdvancedSearchOptions>
    {
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
            bool skipMasterTags = false)
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
        }

        internal AdvancedSearchOptions(ulong data)
        {
            this.data = data;
        }

        private ulong data;

        internal ulong Data => this.data;

        private bool getData(int pos)
        {
            unchecked
            {
                return ((this.data >> (pos + offset)) & 1) == 1;
            }
        }

        private void setData(int pos, bool value)
        {
            unchecked
            {
                pos += offset;
                if (value)
                {
                    this.data |= (ushort)(1 << pos);
                }
                else
                {
                    this.data &= (ushort)~(1 << pos);
                }
            }
        }

        private const int offset = 2;

        //f_sname=on
        public bool SearchName
        {
            get => !getData(0);
            set => setData(0, !value);
        }

        //f_stags=on
        public bool SearchTags
        {
            get => !getData(1);
            set => setData(1, !value);
        }

        //f_sdesc=on
        public bool SearchDescription
        {
            get => getData(2);
            set => setData(2, value);
        }

        //f_storr=on
        public bool SearchTorrentFilenames
        {
            get => getData(3);
            set => setData(3, value);
        }

        //f_sto=on
        public bool GalleriesWithTorrentsOnly
        {
            get => getData(4);
            set => setData(4, value);
        }

        //f_sdt1
        public bool SearchLowPowerTags
        {
            get => getData(5);
            set => setData(5, value);
        }

        //f_sdt2
        public bool SearchDownvotedTags
        {
            get => getData(6);
            set => setData(6, value);
        }

        //f_sh
        public bool ShowExpungedGalleries
        {
            get => getData(7);
            set => setData(7, value);
        }

        //f_sr
        public bool SearchMinimumRating
        {
            get => getData(8);
            set => setData(8, value);
        }

        //skip_mastertags
        public bool SkipMasterTags
        {
            get => getData(9);
            set => setData(9, value);
        }

        //f_srdd
        public int MinimumRating
        {
            get => (int)(this.data & 0b11UL) + 2;
            set
            {
                if (value > 5)
                    value = 5;
                else if (value < 2)
                    value = 2;
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
                return this.Equals(other);
            return false;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return this.data.GetHashCode();
        }

        public bool Equals(AdvancedSearchOptions other)
        {
            if (other == null)
                return false;
            return this.data == other.data;
        }
    }
}
