using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient
{
    //advsearch=1
    public class AdvancedSearchOptions
    {
        public IDictionary<string, string> GetParamDictionary()
        {
            var args = new Dictionary<string, string>();

            args.Add("advsearch", "1");

            args.Add("f_sname", SearchName ? "1" : "0");
            args.Add("f_stags", SearchTags ? "1" : "0");
            args.Add("f_sdesc", SearchDescription ? "1" : "0");
            args.Add("f_storr", SearchTorrentFilenames ? "1" : "0");
            args.Add("f_sto", GalleriesWithTorrentsOnly ? "1" : "0");
            args.Add("f_sdt1", SearchLowPowerTags ? "1" : "0");
            args.Add("f_sdt2", SearchDownvotedTags ? "1" : "0");
            args.Add("f_sh", ShowExpungedGalleries ? "1" : "0");

            args.Add("f_sr", SearchMinimumRating ? "1" : "0");
            args.Add("f_srdd", MinimumRating.ToString());

            return args;
        }

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

        //f_sh
        public bool SearchMinimumRating
        {
            get; set;
        }

        //f_sr=on
        //f_srdd=2
        public int MinimumRating
        {
            get
            {
                return minRating;
            }
            set
            {
                if(value > 5)
                    minRating = 5;
                else if(value < 2)
                    minRating = 2;
                else
                    minRating = value;
            }
        }

        private int minRating = 2;
    }
}
