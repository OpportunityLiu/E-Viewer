using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Newtonsoft.Json;

namespace ExClient
{
    public class Tag
    {
        private static readonly char[] split = new char[] { ':' };
        private static readonly string defaultNameSpace = "misc";
        

        internal Tag(Gallery owner, string content)
        {
            var splited = content.Split(split, 2);
            if(splited.Length == 2)
            {
                NameSpace = splited[0];
                Content = splited[1];
            }
            else
            {
                NameSpace = defaultNameSpace;
                Content = splited[0];
            }
            this.Owner = owner;
        }

        public Gallery Owner
        {
            get;
        }

        public string NameSpace
        {
            get;
        }

        public string Content
        {
            get;
        }

        private string getKeyWord()
        {
            var keyword = $"\"{Content}$\"";
            if(NameSpace != defaultNameSpace)
                keyword = $"{NameSpace}:{keyword}";
            return keyword;
        }

        public SearchResult Search()
        {
            return Owner.Owner.Search(getKeyWord());
        }

        public SearchResult Search(Category filter)
        {
            return Owner.Owner.Search(getKeyWord(), filter);
        }

        public SearchResult Search(Category filter, IAdvancedSearchOptions advancedSearch)
        {
            return Owner.Owner.Search(getKeyWord(), filter, advancedSearch);
        }

        public static Uri WikiUri
        {
            get;
        } = new Uri("https://ehwiki.org/wiki/");

        public Uri TagDefinationUri => new Uri(WikiUri, Content);

        public override string ToString()
        {
            if(NameSpace == defaultNameSpace)
                return Content;
            return $"{NameSpace}:{Content}";
        }
    }
}
