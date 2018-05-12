using ExClient.Search;
using System;

namespace ExClient.Tagging
{
    [System.Diagnostics.DebuggerDisplay(@"[{Namespace}:{Content,nq}]")]
    public readonly struct Tag : IEquatable<Tag>, IComparable<Tag>, IComparable
    {
        // { method: "taggallery", apiuid: apiuid, apikey: apikey, gid: gid, token: token, tags: tagsSplitedWithComma, vote: 1or-1 };
        private static readonly char[] split = new char[] { ':' };

        public static Tag Parse(string content)
        {
            var splited = content.Split(split, 2);
            if (splited.Length == 2)
            {
                return new Tag(NamespaceExtention.Parse(splited[0]), splited[1]);
            }
            else
            {
                return new Tag(Namespace.Misc, content);
            }
        }

        public static bool TryParse(string content, out Tag result)
        {
            result = default;
            var splited = content.Split(split, 2);
            Namespace ns;
            if (splited.Length == 2)
            {
                content = splited[1];
                if (!NamespaceExtention.TryParse(splited[0], out ns))
                {
                    return false;
                }
            }
            else
            {
                ns = Namespace.Misc;
            }
            if (content.IsNullOrWhiteSpace())
            {
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
            if (!@namespace.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(@namespace));
            if (content.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(content));

            this.Namespace = @namespace;
            this.Content = content.Trim().ToLowerInvariant();
        }

        public Namespace Namespace { get; }

        public string Content { get; }

        public string ToSearchTerm()
        {
            if (Namespace != Namespace.Misc)
            {
                if (Content.IndexOf(' ') == -1)
                {
                    return $"{Namespace.ToSearchString()}:{Content}$";
                }
                else
                {
                    return $"{Namespace.ToSearchString()}:\"{Content}$\"";
                }
            }
            else
            {
                if (Content.IndexOf(' ') == -1)
                {
                    return $"{Content}$";
                }
                else
                {
                    return $"\"{Content}$\"";
                }
            }
        }

        public CategorySearchResult Search()
            => Client.Current.Search(ToSearchTerm());

        public CategorySearchResult Search(Category filter)
            => Client.Current.Search(ToSearchTerm(), filter);

        public CategorySearchResult Search(Category filter, AdvancedSearchOptions advancedSearch)
            => Client.Current.Search(ToSearchTerm(), filter, advancedSearch);

        public static Uri WikiUri { get; } = new Uri("https://ehwiki.org/wiki/");

        public Uri TagDefinitionUri => new Uri(WikiUri, Content);

        public override string ToString()
        {
            if (Namespace == Namespace.Misc)
                return Content;
            return $"{Namespace.ToShortString()}:{Content}";
        }

        public bool Equals(Tag other)
        {
            return this.Namespace == other.Namespace
                && this.Content == other.Content;
        }

        public override bool Equals(object obj)
        {
            if (obj is Tag o)
                return this.Equals(o);
            return false;
        }

        public static bool operator ==(Tag left, Tag right) => left.Equals(right);
        public static bool operator !=(Tag left, Tag right) => !left.Equals(right);

        public override int GetHashCode()
        {
            return unchecked(((int)Namespace * 958339) ^ Content.GetHashCode());
        }

        public int CompareTo(Tag other)
        {
            var c1 = this.Namespace - other.Namespace;
            if (c1 != 0)
                return c1;
            return this.Content.CompareTo(other.Content);
        }

        int IComparable.CompareTo(object obj) => obj is null ? 1 : CompareTo((Tag)obj);
    }
}
