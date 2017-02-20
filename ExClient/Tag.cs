using ExClient.Models;
using Newtonsoft.Json;
using System;
using System.Text;

namespace ExClient
{
    [Flags]
    public enum Namespace
    {
        Unknown = 0,

        Reclass = 1,
        Language = 2,
        Parody = 4,
        Character = 8,
        Group = 16,
        Artist = 32,
        Male = 64,
        Female = 128,
        Misc = 256
    }

    public static class NamespaceExtention
    {
        public static string ToFriendlyNameString(this Namespace that)
        {
            if(Enum.IsDefined(typeof(Namespace), that))
                return LocalizedStrings.Namespace.GetString(that.ToString());
            else
            {
                var represent = new StringBuilder(that.ToString());
                foreach(var item in Enum.GetNames(typeof(Namespace)))
                {
                    represent.Replace(item, LocalizedStrings.Namespace.GetString(item));
                }
                return represent.ToString();
            }
        }

        public static bool IsValid(this Namespace that)
        {
            return that != Namespace.Unknown && Enum.IsDefined(typeof(Namespace), that);
        }
    }

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

        private string getKeyWord()
        {
            if(Namespace != Namespace.Misc)
                return $"{Namespace.ToString().ToLowerInvariant()}:\"{Content}$\"";
            else
                return $"\"{Content}$\"";
        }

        public SearchResult Search()
        {
            return getClient().Search(getKeyWord());
        }

        public SearchResult Search(Category filter)
        {
            return getClient().Search(getKeyWord(), filter);
        }

        public SearchResult Search(Category filter, AdvancedSearchOptions advancedSearch)
        {
            return getClient().Search(getKeyWord(), filter, advancedSearch);
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
