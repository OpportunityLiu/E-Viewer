using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient
{
    internal static class AdvancedSearchExtention
    {
        public static IDictionary<string, string> GetParamDictionary(this IAdvancedSearchOptions advancedSearch)
        {
            var args = new Dictionary<string, string>();
            if(advancedSearch == null)
                return args;
            var ad = advancedSearch;
            args.Add("advsearch", "1");

            args.Add("f_sname", ad.SearchName ? "1" : "0");
            args.Add("f_stags", ad.SearchTags ? "1" : "0");
            args.Add("f_sdesc", ad.SearchDescription ? "1" : "0");
            args.Add("f_storr", ad.SearchTorrentFilenames ? "1" : "0");
            args.Add("f_sto", ad.GalleriesWithTorrentsOnly ? "1" : "0");
            args.Add("f_sdt1", ad.SearchLowPowerTags ? "1" : "0");
            args.Add("f_sdt2", ad.SearchDownvotedTags ? "1" : "0");
            args.Add("f_sh", ad.ShowExpungedGalleries ? "1" : "0");

            args.Add("f_sr", ad.MinimumRating.HasValue ? "1" : "0");
            args.Add("f_srdd", ad.MinimumRating.GetValueOrDefault(2).ToString());

            return args;
        }
    }

    //advsearch=1
    public class AdvancedSearchOptions : IAdvancedSearchOptions
    {
        //f_sname=on
        public bool SearchName
        {
            get;
            set;
        } = true;

        //f_stags=on
        public bool SearchTags
        {
            get;
            set;
        } = true;

        //f_sdesc=on
        public bool SearchDescription
        {
            get; set;
        }

        //f_storr=on
        public bool SearchTorrentFilenames
        {
            get; set;
        }

        //f_sto=on
        public bool GalleriesWithTorrentsOnly
        {
            get; set;
        }

        //f_sdt1
        public bool SearchLowPowerTags
        {
            get; set;
        }

        //f_sdt2
        public bool SearchDownvotedTags
        {
            get; set;
        }

        //f_sh
        public bool ShowExpungedGalleries
        {
            get; set;
        }

        //f_sr=on
        //f_srdd=2
        public int? MinimumRating
        {
            get
            {
                return minRating;
            }
            set
            {
                if(value.HasValue)
                {
                    var r = value.Value;
                    if(r > 5)
                        minRating = 5;
                    else if(r < 2)
                        minRating = 2;
                    else
                        minRating = value;
                }
                else
                    minRating = value;
            }
        }

        private int? minRating;
    }

    public interface IAdvancedSearchOptions
    {
        //f_sname=on
        bool SearchName
        {
            get;
        }

        //f_stags=on
        bool SearchTags
        {
            get;
        }

        //f_sdesc=on
        bool SearchDescription
        {
            get;
        }

        //f_storr=on
        bool SearchTorrentFilenames
        {
            get;
        }

        //f_sto=on
        bool GalleriesWithTorrentsOnly
        {
            get;
        }

        //f_sdt1
        bool SearchLowPowerTags
        {
            get;
        }

        //f_sdt2
        bool SearchDownvotedTags
        {
            get;
        }

        //f_sh
        bool ShowExpungedGalleries
        {
            get;
        }

        //f_sr=on
        //f_srdd=2
        int? MinimumRating
        {
            get;
        }
    }
}
