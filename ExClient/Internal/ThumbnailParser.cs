using HtmlAgilityPack;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExClient.Internal {
    internal static class ThumbnailParser {
        private static readonly Regex Url = new Regex(
            @"url\(\s*(?:(?<quote>['""])(?<uri>.*?)\k<quote>|(?<uri>[^'""\s()]+))\s*\)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

        public static ThumbnailSource Parse(HtmlNode page) {
            if (page == null)
                return null;
            var background = page.Descendants("div").FirstOrDefault(n =>
                n.GetAttributeValue("style", "").IndexOf("background", StringComparison.OrdinalIgnoreCase) >= 0);
            if (background != null)
                return ParseStyle(HtmlEntity.DeEntitize(background.GetAttributeValue("style", "")));
            var image = page.Descendants("img").FirstOrDefault();
            return TryUri(HtmlEntity.DeEntitize(image?.GetAttributeValue("src", "")), out var uri) ? new ThumbnailSource(uri) : null;
        }

        // Only the site's pixel-based, unscaled background subset is accepted. Unknown sprites stay placeholders.
        internal static ThumbnailSource ParseStyle(string style) {
            try { return ParseStyleCore(style); }
            catch (RegexMatchTimeoutException) { return null; }
        }

        private static ThumbnailSource ParseStyleCore(string style) {
            if (string.IsNullOrWhiteSpace(style) || style.Length > 4096 || style.Contains("\\")
                || style.Contains("/*") || style.Contains("!"))
                return null;
            var declarations = Split(style, ';');
            if (declarations == null)
                return null;
            string image = null;
            long? width = null, height = null;
            long x = 0, y = 0;
            foreach (var declaration in declarations) {
                if (string.IsNullOrWhiteSpace(declaration))
                    continue;
                var colon = declaration.IndexOf(':');
                if (colon < 1)
                    return null;
                var name = declaration.Substring(0, colon).Trim().ToLowerInvariant();
                var value = declaration.Substring(colon + 1).Trim();
                switch (name) {
                    case "width":
                    case "height":
                        if (!TryPixel(value, out var size) || size <= 0 || size > uint.MaxValue)
                            return null;
                        if (name == "width") width = size; else height = size;
                        break;
                    case "background":
                    case "background-image":
                        var matches = Url.Matches(value);
                        if (matches.Count != 1 || !TryUri(matches[0].Groups["uri"].Value.Trim(), out _))
                            return null;
                        image = matches[0].Groups["uri"].Value.Trim();
                        var rest = value.Remove(matches[0].Index, matches[0].Length).Trim();
                        if (name == "background-image") {
                            if (rest.Length != 0) return null;
                            break;
                        }
                        x = y = 0; // The shorthand resets position, regardless of declaration order.
                        var parts = Regex.Split(rest, @"\s+").Where(p => p.Length != 0
                            && !p.Equals("transparent", StringComparison.OrdinalIgnoreCase)
                            && !p.Equals("no-repeat", StringComparison.OrdinalIgnoreCase)).ToArray();
                        if (parts.Length != 0 && !TryPosition(parts, out x, out y))
                            return null;
                        break;
                    case "background-position":
                        if (!TryPosition(Regex.Split(value, @"\s+"), out x, out y))
                            return null;
                        break;
                    case "background-position-x":
                    case "background-position-y":
                        if (!TryOffset(value, out var offset)) return null;
                        if (name.EndsWith("-x", StringComparison.Ordinal)) x = offset; else y = offset;
                        break;
                    case "background-repeat":
                        if (!value.Equals("no-repeat", StringComparison.OrdinalIgnoreCase)) return null;
                        break;
                    case "background-size":
                        if (!value.Equals("auto", StringComparison.OrdinalIgnoreCase)
                            && !Regex.IsMatch(value, @"^auto\s+auto$", RegexOptions.IgnoreCase)) return null;
                        break;
                    case "background-color":
                        break;
                    default:
                        if (name.StartsWith("background-", StringComparison.Ordinal)) return null;
                        break;
                }
            }
            return image != null && width.HasValue && height.HasValue
                ? new ThumbnailSource(new Uri(image), new ThumbnailRegion(x, y, width.Value, height.Value)) : null;
        }

        private static bool TryPosition(string[] parts, out long x, out long y) {
            x = y = 0;
            return parts.Length == 2 && TryOffset(parts[0], out x) && TryOffset(parts[1], out y);
        }

        private static bool TryOffset(string value, out long offset) {
            offset = 0;
            if (!TryPixel(value, out var css) || css > 0 || css < -(long)uint.MaxValue)
                return false;
            offset = -css;
            return true;
        }

        private static bool TryPixel(string value, out long result) {
            var px = value.EndsWith("px", StringComparison.OrdinalIgnoreCase);
            var number = px ? value.Substring(0, value.Length - 2) : value;
            return long.TryParse(number, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out result)
                && (px || result == 0);
        }

        private static bool TryUri(string value, out Uri uri) => Uri.TryCreate(value, UriKind.Absolute, out uri)
            && (uri.Scheme == "https" || uri.Scheme == "http");

        private static List<string> Split(string value, char separator) {
            var result = new List<string>();
            var start = 0;
            var depth = 0;
            var quote = '\0';
            for (var i = 0; i < value.Length; i++) {
                var c = value[i];
                if (quote != '\0') {
                    if (c == quote) quote = '\0';
                } else if (c == '\'' || c == '"') {
                    quote = c;
                } else if (c == '(') {
                    depth++;
                } else if (c == ')') {
                    if (--depth < 0) return null;
                } else if (c == separator && depth == 0) {
                    result.Add(value.Substring(start, i - start));
                    start = i + 1;
                }
            }
            if (depth != 0 || quote != '\0') return null;
            result.Add(value.Substring(start));
            return result;
        }
    }
}
