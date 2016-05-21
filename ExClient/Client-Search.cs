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
        public SearchResult Search(string keyWord, Category filter, IAdvancedSearchOptions advancedSearch)
        {
            return SearchResult.Search(this, keyWord, filter, advancedSearch);
        }

        public SearchResult Search(string keyWord, Category filter)
        {
            return Search(keyWord, filter, null);
        }

        public SearchResult Search(string keyWord)
        {
            return Search(keyWord, Category.Unspecified);
        }
    }
}
