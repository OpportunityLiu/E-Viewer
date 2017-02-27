using System;
using System.Text;

namespace ExClient
{
    public sealed class Tag
    {
        // { method: "taggallery", apiuid: apiuid, apikey: apikey, gid: gid, token: token, tags: tagsSplitedWithComma, vote: 1or-1 };
        private static readonly char[] split = new char[] { ':' };

        public static Tag Parse(string content)
        {
            var splited = content.Split(split, 2);
            if(splited.Length == 2)
                return new Tag((Namespace)Enum.Parse(typeof(Namespace), splited[0], true), splited[1]);
            else
                return new Tag(Namespace.Misc, content);

        }

        public Tag(Namespace @namespace, string content)
        {
            if(!@namespace.IsValid())
                throw new ArgumentOutOfRangeException(nameof(@namespace));
            if(string.IsNullOrWhiteSpace(content))
                throw new ArgumentNullException(nameof(content));
            this.Namespace = @namespace;
            this.Content = content.Trim();
        }

        public Namespace Namespace
        {
            get;
        }

        public string Content
        {
            get;
        }

        private Client getClient() => Client.Current;

        private string getKeyword()
        {
            if(Namespace != Namespace.Misc)
                return $"{Namespace.ToString().ToLowerInvariant()}:\"{Content}$\"";
            else
                return $"\"{Content}$\"";
        }

        public SearchResult Search()
        {
            return getClient().Search(getKeyword());
        }

        public SearchResult Search(Category filter)
        {
            return getClient().Search(getKeyword(), filter);
        }

        public SearchResult Search(Category filter, AdvancedSearchOptions advancedSearch)
        {
            return getClient().Search(getKeyword(), filter, advancedSearch);
        }

        public static Uri WikiUri
        {
            get;
        } = new Uri("https://ehwiki.org/wiki/");

        public Uri TagDefinitionUri => new Uri(WikiUri, Content);

        public override string ToString()
        {
            if(Namespace == Namespace.Misc)
                return Content;
            return $"{Namespace}:{Content}";
        }
    }
}
