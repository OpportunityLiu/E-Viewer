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

        public string RawString { get; }

        private string text;
        public string Text
            => System.Threading.LazyInitializer.EnsureInitialized(ref this.text, ()
                => string.Concat(this.Tokens.OfType<MarkdownString>()));

        private static readonly Regex analyzer = new Regex(@"
((?<image>!)|(?<link>))
\[
  (?<alt>.*?)
\]
\(
  (?:\s*?
    (?<url>\S+?)
    \s*?
    (
      ""(?<title>.*?)""\s*?
    |
    )
  )
\)", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

        private IReadOnlyList<MarkdownToken> tokens;
        public IReadOnlyList<MarkdownToken> Tokens
            => System.Threading.LazyInitializer.EnsureInitialized(ref this.tokens, ()
                => this.analyze().ToList().AsReadOnly());

        private IEnumerable<MarkdownToken> analyze()
        {
            var matches = analyzer.Matches(this.RawString);
            if (matches.Count == 0)
            {
                yield return new MarkdownString(this.RawString);
                yield break;
            }
            var currentPos = 0;
            foreach (var match in matches.Cast<Match>())
            {
                if (currentPos != match.Index)
                    yield return new MarkdownString(this.RawString.Substring(currentPos, match.Index - currentPos));
                var alt = match.Groups["alt"].Value;
                var url = match.Groups["url"].Value;
                var title = match.Groups["title"].Value;
                if (match.Groups["image"].Success)
                {
                    if (url == "#" && !string.IsNullOrEmpty(title))
                        yield return new MarkdownImage(alt, title);
                    else
                        yield return new MarkdownImage(alt, url);
                }
                else
                {
                    yield return new MarkdownLink(alt, url);
                }
                currentPos = match.Index + match.Length;
            }
            if (currentPos != this.RawString.Length)
                yield return new MarkdownString(this.RawString.Substring(currentPos));
        }
    }

    [System.Diagnostics.DebuggerDisplay(@"{GetType()}{ToString()}")]
    public abstract class MarkdownToken : Windows.Foundation.IStringable
    {
    }

    public class MarkdownString : MarkdownToken
    {
        internal MarkdownString(string str)
        {
            this.String = str ?? "";
        }

        public string String { get; }

        public override string ToString()
        {
            return this.String;
        }
    }

    public sealed class MarkdownImage : MarkdownToken
    {
        internal MarkdownImage(string alternateText, string imageUri)
        {
            this.AlternateText = alternateText;
            this.ImageUri = new Uri(imageUri);
        }

        public string AlternateText { get; }

        public Uri ImageUri { get; }

        public override string ToString()
        {
            return this.AlternateText;
        }
    }

    public sealed class MarkdownLink : MarkdownString
    {
        internal MarkdownLink(string text, string linkUri)
            : base(text)
        {
            this.LinkUri = new Uri(linkUri);
        }

        public Uri LinkUri { get; }
    }
}
