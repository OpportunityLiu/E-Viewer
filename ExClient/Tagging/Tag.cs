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
            if (content.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(content));
            var splited = content.Split(split, 2);
            if (splited.Length == 2)
            {
                return new Tag(NamespaceExtention.Parse(splited[0]), splited[1]);
            }
            else
            {
                return new Tag(Namespace.Temp, content);
            }
        }

        public static bool TryParse(string content, out Tag result)
        {
            result = default;
            if (content.IsNullOrWhiteSpace())
                return false;

            var splited = content.Split(split, 2);
            if (splited.Length == 2)
            {
                if (!NamespaceExtention.TryParse(splited[0], out var ns))
                    return false;
                result = new Tag(ns, splited[1]);
                return true;
            }
            else
            {
                result = new Tag(Namespace.Temp, content);
                return true;
            }
        }

        public Tag(Namespace @namespace, string content)
        {
            if (!@namespace.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(@namespace));
            if (content.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(content));

            Namespace = @namespace;
            Content = content.Trim().ToLowerInvariant();
        }

        public Namespace Namespace { get; }

        public string Content { get; }

        public string ToSearchTerm()
        {
            if (Namespace != Namespace.Temp)
            {
                if (Content.IndexOf(' ') == -1)
                {
                    return $"{Namespace.ToSearchString(true)}:{Content}$";
                }
                else
                {
                    return $"{Namespace.ToSearchString(true)}:\"{Content}$\"";
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

        public AdvancedSearchResult Search()
            => Client.Current.Search(ToSearchTerm());

        public AdvancedSearchResult Search(Category filter)
            => Client.Current.Search(ToSearchTerm(), filter);

        public AdvancedSearchResult Search(Category filter, AdvancedSearchOptions advancedSearch)
            => Client.Current.Search(ToSearchTerm(), filter, advancedSearch);

        public static Uri WikiUri { get; } = new Uri("https://ehwiki.org/wiki/");

        public Uri TagDefinitionUri => new Uri(WikiUri, Content);

        public override string ToString()
        {
            if (Namespace == Namespace.Temp)
                return Content;
            return $"{Namespace.ToSearchString(true)}:{Content}";
        }

        public bool Equals(Tag other)
            => Namespace == other.Namespace
            && Content == other.Content;

        public override bool Equals(object obj) => obj is Tag t && Equals(t);

        public static bool operator ==(in Tag left, in Tag right) => left.Equals(right);
        public static bool operator !=(in Tag left, in Tag right) => !left.Equals(right);

        public override int GetHashCode()
        {
            return unchecked(((int)Namespace * 958339) ^ Content.GetHashCode());
        }

        public int CompareTo(Tag other)
        {
            var c1 = Namespace - other.Namespace;
            if (c1 != 0)
                return c1;
            return Content.CompareTo(other.Content);
        }

        int IComparable.CompareTo(object obj) => obj is null ? 1 : CompareTo((Tag)obj);
    }
}
