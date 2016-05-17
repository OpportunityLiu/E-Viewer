using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Windows.Foundation;
using HtmlAgilityPack;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

namespace ExClient
{
    public partial class Client
    {
        public IAsyncOperation<SearchResult> SearchAsync(string keyWord, Category filter, IAdvancedSearchOptions advancedSearch)
        {
            return SearchResult.SearchAsync(this, keyWord, filter, advancedSearch);
        }

        public IAsyncOperation<SearchResult> SearchAsync(string keyWord, Category filter)
        {
            return SearchAsync(keyWord, filter, null);
        }

        public IAsyncOperation<SearchResult> SearchAsync(string keyWord)
        {
            return SearchAsync(keyWord, Category.Unspecified);
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
