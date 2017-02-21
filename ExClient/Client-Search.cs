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
        public SearchResult Search(string keyword, Category category, AdvancedSearchOptions advancedSearch)
        {
            return SearchResult.Search(this, keyword, category, advancedSearch);
        }

        public SearchResult Search(string keyword, Category category)
        {
            return Search(keyword, category, null);
        }

        public SearchResult Search(string keyword)
        {
            return Search(keyword, Category.Unspecified);
        }
    }
}
