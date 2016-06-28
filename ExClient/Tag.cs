using ExClient.Models;
using Newtonsoft.Json;
using System;

namespace ExClient
{
    public enum NameSpace
    {
        Reclass,
        Language,
        Parody,
        Character,
        Group,
        Artist,
        Male,
        Female,
        Misc
    }

    public class Tag
    {
        // { method: "taggallery", apiuid: apiuid, apikey: apikey, gid: gid, token: token, tags: tagsSplitedWithComma, vote: 1or-1 };
        private static readonly char[] split = new char[] { ':' };

        internal Tag(Gallery owner, string content)
        {
            var splited = content.Split(split, 2);
            if(splited.Length == 2)
            {
                NameSpace = (NameSpace)Enum.Parse(typeof(NameSpace), splited[0], true);
                Content = splited[1];
            }
            else
            {
                Content = splited[0];
            }
            this.Owner = owner;
        }

        public Gallery Owner
        {
            get;
            internal set;
        }

        public NameSpace NameSpace
        {
            get;
            internal set;
        }

        public string Content
        {
            get;
            internal set;
        }

        private string getKeyWord()
        {
            var keyword = $"\"{Content}$\"";
            if(NameSpace != NameSpace.Misc)
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

        public SearchResult Search(Category filter, AdvancedSearchOptions advancedSearch)
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
            if(NameSpace == NameSpace.Misc)
                return Content;
            return $"{NameSpace}:{Content}";
        }
    }
}
