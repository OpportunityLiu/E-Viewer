using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient
{
    //advsearch=1
    public class AdvancedSearchOptions
    {
        internal IEnumerable<KeyValuePair<string, string>> AsEnumerable()
        {
            yield return new KeyValuePair<string, string>("advsearch", "1");

            yield return new KeyValuePair<string, string>("f_sname", SearchName ? "1" : "0");
            yield return new KeyValuePair<string, string>("f_stags", SearchTags ? "1" : "0");
            yield return new KeyValuePair<string, string>("f_sdesc", SearchDescription ? "1" : "0");
            yield return new KeyValuePair<string, string>("f_storr", SearchTorrentFilenames ? "1" : "0");
            yield return new KeyValuePair<string, string>("f_sto", GalleriesWithTorrentsOnly ? "1" : "0");
            yield return new KeyValuePair<string, string>("f_sdt1", SearchLowPowerTags ? "1" : "0");
            yield return new KeyValuePair<string, string>("f_sdt2", SearchDownvotedTags ? "1" : "0");
            yield return new KeyValuePair<string, string>("f_sh", ShowExpungedGalleries ? "1" : "0");

            yield return new KeyValuePair<string, string>("f_sr", SearchMinimumRating ? "1" : "0");
            yield return new KeyValuePair<string, string>("f_srdd", MinimumRating.ToString());
        }

        public AdvancedSearchOptions Clone(bool isReadOnly)
        {
            return new AdvancedSearchOptions(isReadOnly) { data = this.data };
        }

        public AdvancedSearchOptions()
        {
        }

        private AdvancedSearchOptions(bool isReadOnly)
        {
            this.IsReadOnly = isReadOnly;
        }

        [JsonIgnore]
        public bool IsReadOnly
        {
            get;
        }

        private ushort data;

        private bool getData(int pos)
        {
            unchecked
            {
                return ((data >> (pos + offset)) & 1) == 1;
            }
        }

        private void setData(int pos, bool value)
        {
            checkWritable();
            unchecked
            {
                pos += offset;
                if(value)
                {
                    data |= (ushort)(1 << pos);
                }
                else
                {
                    data &= (ushort)~(1 << pos);
                }
            }
        }

        private void checkWritable()
        {
            if(IsReadOnly)
                throw new InvalidOperationException("The instanse is read only.");
        }

        private const int offset = 2;

        //f_sname=on
        public bool SearchName
        {
            get
            {
                return !getData(0);
            }
            set
            {
                setData(0, !value);
            }
        }

        //f_stags=on
        public bool SearchTags
        {
            get
            {
                return !getData(1);
            }
            set
            {
                setData(1, !value);
            }
        }

        //f_sdesc=on
        public bool SearchDescription
        {
            get
            {
                return getData(2);
            }
            set
            {
                setData(2, value);
            }
        }

        //f_storr=on
        public bool SearchTorrentFilenames
        {
            get
            {
                return getData(3);
            }
            set
            {
                setData(3, value);
            }
        }

        //f_sto=on
        public bool GalleriesWithTorrentsOnly
        {
            get
            {
                return getData(4);
            }
            set
            {
                setData(4, value);
            }
        }

        //f_sdt1
        public bool SearchLowPowerTags
        {
            get
            {
                return getData(5);
            }
            set
            {
                setData(5, value);
            }
        }

        //f_sdt2
        public bool SearchDownvotedTags
        {
            get
            {
                return getData(6);
            }
            set
            {
                setData(6, value);
            }
        }

        //f_sh
        public bool ShowExpungedGalleries
        {
            get
            {
                return getData(7);
            }
            set
            {
                setData(7, value);
            }
        }

        //f_sr
        public bool SearchMinimumRating
        {
            get
            {
                return getData(8);
            }
            set
            {
                setData(8, value);
            }
        }

        //f_srdd
        public int MinimumRating
        {
            get
            {
                return (data & 3) + 2;
            }
            set
            {
                checkWritable();
                if(value > 5)
                    value = 5;
                else if(value < 2)
                    value = 2;
                value -= 2;
                unchecked
                {
                    data &= (ushort)~3;
                    data |= (ushort)value;
                }
            }
        }

        public override bool Equals(object obj)
        {
            if(obj == null || !(obj is AdvancedSearchOptions))
            {
                return false;
            }
            var other = (AdvancedSearchOptions)obj;
            return this.data == other.data;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return data.GetHashCode();
        }
    }
}
