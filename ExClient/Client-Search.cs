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
        public IAsyncOperation<SearchResult> SearchAsync(string keyWord, Category filter)
        {
            return SearchResult.SearchAsync(this, keyWord ?? "", filter);
        }

        public IAsyncOperation<SearchResult> SearchAsync(string keyWord)
        {
            return SearchAsync(keyWord ?? "", Category.Unspecified);
        }
    }
}
