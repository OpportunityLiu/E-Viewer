using System;
using System.Text;

namespace ExClient
{
    [System.Diagnostics.DebuggerDisplay(@"[{Namespace}:{Content}]")]
    public struct Tag : IEquatable<Tag>
    {
        // { method: "taggallery", apiuid: apiuid, apikey: apikey, gid: gid, token: token, tags: tagsSplitedWithComma, vote: 1or-1 };
        private static readonly char[] split = new char[] { ':' };

        public static Tag Parse(string content)
        {
            var splited = content.Split(split, 2);
            if(splited.Length == 2)
                return new Tag(NamespaceExtention.Parse(splited[0]), splited[1]);
            else
                return new Tag(Namespace.Misc, content);
        }

        public static bool TryParse(string content, out Tag result)
        {
            result = default(Tag);
            var splited = content.Split(split, 2);
            if(splited.Length == 2)
            {
                if(NamespaceExtention.TryParse(splited[0], out var ns))
                {
                    result = new Tag(ns, splited[1]);
                    return true;
                }
                return false;
            }
            else
            {
                result = new Tag(Namespace.Misc, content);
                return true;
            }
        }

        public Tag(Namespace @namespace, string content)
        {
            if(!@namespace.IsDefined())
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

        public string ToSearchTerm()
        {
            if(Namespace != Namespace.Misc)
                return $"{Namespace.ToString().ToLowerInvariant()}:\"{Content}$\"";
            else
                return $"\"{Content}$\"";
        }

        public SearchResult Search()
        {
            return Client.Current.Search(ToSearchTerm());
        }

        public SearchResult Search(Category filter)
        {
            return Client.Current.Search(ToSearchTerm(), filter);
        }

        public SearchResult Search(Category filter, AdvancedSearchOptions advancedSearch)
        {
            return Client.Current.Search(ToSearchTerm(), filter, advancedSearch);
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

        public bool Equals(Tag other)
        {
            return this.Namespace == other.Namespace && string.Equals(this.Content, other.Content, StringComparison.OrdinalIgnoreCase);
            throw new NotImplementedException();
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if(obj is Tag o)
                return this.Equals(o);
            return false;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return unchecked((int)Namespace * StringComparer.OrdinalIgnoreCase.GetHashCode(Content));
        }
    }
}
