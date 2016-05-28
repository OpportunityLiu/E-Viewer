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
        public SearchResult Search(string keyWord, Category category, AdvancedSearchOptions advancedSearch)
        {
            return SearchResult.Search(this, keyWord, category, advancedSearch);
        }

        public SearchResult Search(string keyWord, Category category)
        {
            return Search(keyWord, category, null);
        }

        public SearchResult Search(string keyWord)
        {
            return Search(keyWord, Category.Unspecified);
        }
    }
}
