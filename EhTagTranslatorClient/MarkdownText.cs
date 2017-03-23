using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EhTagTranslatorClient
{
    [System.Diagnostics.DebuggerDisplay(@"MD{RawString}")]
    public sealed class MarkdownText
    {
        internal MarkdownText(string rawString)
        {
            this.RawString = rawString;
        }

        public string RawString
        {
            get;
        }

        private string text;

        public string Text => 
            System.Threading.LazyInitializer.EnsureInitialized(ref text, () 
                => Analyze()
                    .Where(t => t is MarkdownString)
                    .Aggregate("", (old, t) => old + t.ToString()));

        private static readonly Regex analyzer = new Regex(@"!\[(?<alt>.*?)\]\((?:(?<url>[^#].*?)|(?:#\s+""(?<url>.*?)""))\)", RegexOptions.Compiled);

        public IEnumerable<MarkdownToken> Analyze()
        {
            var matches = analyzer.Matches(this.RawString);
            if(matches.Count == 0)
            {
                yield return new MarkdownString(this.RawString);
                yield break;
            }
            var currentPos = 0;
            foreach(Match match in matches)
            {
                if(currentPos != match.Index)
                    yield return new MarkdownString(this.RawString.Substring(currentPos, match.Index - currentPos));
                yield return new MarkdownImage(match.Groups["alt"].Value, match.Groups["url"].Value);
                currentPos = match.Index + match.Length;
            }
            if(currentPos != this.RawString.Length)
                yield return new MarkdownString(this.RawString.Substring(currentPos));
        }
    }

    [System.Diagnostics.DebuggerDisplay(@"{GetType()}{ToString()}")]
    public class MarkdownToken
    {
    }

    public class MarkdownString : MarkdownToken
    {
        internal MarkdownString(string str)
        {
            this.String = str;
        }

        public string String
        {
            get;
        }

        public override string ToString()
        {
            return this.String;
        }
    }

    public class MarkdownImage : MarkdownToken
    {
        internal MarkdownImage(string alternateText, string imageUri)
        {
            this.AlternateText = alternateText;
            this.ImageUri = new Uri(imageUri);
        }

        public string AlternateText
        {
            get;
        }

        public Uri ImageUri
        {
            get;
        }

        public override string ToString()
        {
            return this.AlternateText;
        }
    }
}
