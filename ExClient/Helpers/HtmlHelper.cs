using System;
using System.Collections.Generic;
using System.Linq;

namespace HtmlAgilityPack
{
    internal static class HtmlHelper
    {
        public static HtmlNode Element(this HtmlNode node, string name, string className)
        {
            return node.Elements(name, className).FirstOrDefault();
        }

        public static IEnumerable<HtmlNode> Elements(this HtmlNode node, string name, string className)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            return node.Elements(name).Where(n => n.HasClass(className));
        }

        public static string GetAttribute(this HtmlNode node, string attributeName, string def = null)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            return HtmlEntity.DeEntitize(node.GetAttributeValue(attributeName, null)) ?? def;
        }

        public static int GetAttribute(this HtmlNode node, string attributeName, int def = default)
        {
            var r = node.GetAttribute(attributeName, default(string));
            if (r == null)
                return def;
            if (int.TryParse(r, out var intP))
                return intP;
            return def;
        }

        public static bool GetAttribute(this HtmlNode node, string attributeName, bool def = default)
        {
            var r = node.GetAttribute(attributeName, default(string));
            if (r == null)
                return def;
            return true;
        }

        public static Uri GetAttribute(this HtmlNode node, string attributeName, Uri def = default)
            => GetAttribute(node, attributeName, ExClient.Client.Current.Uris.RootUri, def);

        public static Uri GetAttribute(this HtmlNode node, string attributeName, Uri baseUri, Uri def = default)
        {
            var r = node.GetAttribute(attributeName, default(string));
            if (r == null)
                return def;
            return new Uri(baseUri, r);
        }

        public static T GetAttribute<T>(this HtmlNode node, string attributeName, T def = default)
        {
            var r = node.GetAttribute(attributeName, default(string));
            if (r == null)
                return def;
            return (T)Convert.ChangeType(r, typeof(T));
        }

        public static string GetInnerText(this HtmlNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            return HtmlEntity.DeEntitize(node.InnerText);
        }
    }
}