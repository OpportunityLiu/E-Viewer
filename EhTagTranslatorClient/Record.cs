using ExClient.Tagging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Windows.Storage.Streams;

namespace EhTagTranslatorClient
{
    [DebuggerDisplay(@"\{{Original} => {Translated.RawString}\}")]
    public class Record
    {
        private static Regex lineRegex = new Regex(
            $@"
            ^\s*(?<!\\)\|?\s*
            (?<{nameof(Original)}>.*?)
		    \s*(?<!\\)\|\s*
		    (?<{nameof(Translated)}>.*?)
		    \s*(?<!\\)\|\s*
		    (?<{nameof(Introduction)}>.*?)
		    \s*(?<!\\)\|\s*
		    (?<{nameof(ExternalLinks)}>.*?)
		    \s*(?<!\\)\|?\s*$", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

        public static IEnumerable<Record> Analyze(IInputStream stream, Namespace @namespace)
        {
            var skipcount = 0;
            var reader = new StreamReader(stream.AsStreamForRead());
            while (!reader.EndOfStream)
            {
                var r = AnalyzeLine(reader.ReadLine(), @namespace);
                if (r != null)
                {
                    if (skipcount >= 2)
                        yield return r;
                    else
                        skipcount++;
                }
            }
        }

        internal static Record AnalyzeLine(string line, Namespace @namespace)
        {
            var match = lineRegex.Match(line);
            if (!match.Success)
                return null;
            var ori = match.Groups[nameof(Original)].Value;
            if (string.IsNullOrEmpty(ori))
                return null;
            var tra = match.Groups[nameof(Translated)].Value;
            var intro = match.Groups[nameof(Introduction)].Value;
            var link = match.Groups[nameof(ExternalLinks)].Value;
            return new Record(@namespace, unescape(ori), unescape(tra), unescape(intro), unescape(link));
        }

        private static string unescape(string value)
        {
            if (value.Contains("<br>") || value.Contains(@"\"))
            {
                var sb = new StringBuilder(value);
                sb.Replace(@"\|", @"|");
                sb.Replace(@"\~", @"~");
                sb.Replace(@"\*", @"*");
                sb.Replace(@"\\", @"\");
                sb.Replace("<br>", Environment.NewLine);
                return sb.ToString();
            }
            return value;
        }

        internal Record() { }

        internal static Record Combine(Record r1, Record r2)
        {
            if (r1.Original != r2.Original)
                throw new InvalidOperationException();
            if (r1.Namespace != r2.Namespace)
                throw new InvalidOperationException();
            string original, translated, intro;
            original = r1.Original;
            if (r1.TranslatedRaw == r2.TranslatedRaw)
            {
                translated = r1.TranslatedRaw;
            }
            else
            {
                translated = $@"{r1.TranslatedRaw} | {r2.TranslatedRaw}";
            }
            if (string.IsNullOrWhiteSpace(r1.IntroductionRaw))
                intro = r2.IntroductionRaw;
            else if (string.IsNullOrWhiteSpace(r2.IntroductionRaw))
                intro = r1.IntroductionRaw;
            else
                intro = $"{r1.IntroductionRaw}<hr>{r2.IntroductionRaw}";
            return new Record(r1.Namespace, r1.Original, translated, intro, $"{r1.ExternalLinksRaw} {r2.ExternalLinksRaw}");
        }

        private Record(Namespace @namespace, string original, string translated, string introduction, string externalLinks)
        {
            this.Namespace = @namespace;
            this.Original = original;
            this.TranslatedRaw = translated;
            this.IntroductionRaw = introduction;
            this.TranslatedStr = this.Translated.Text;
            this.ExternalLinksRaw = externalLinks;
        }

        public string Original { get; internal set; }

        public string TranslatedRaw { get; internal set; }

        public string TranslatedStr { get; internal set; }

        public MarkdownText Translated => new MarkdownText(TranslatedRaw);

        public string IntroductionRaw { get; internal set; }

        public MarkdownText Introduction => new MarkdownText(IntroductionRaw);

        public string ExternalLinksRaw { get; internal set; }

        public IEnumerable<MarkdownLink> ExternalLinks => new MarkdownText(ExternalLinksRaw).Tokens.OfType<MarkdownLink>();

        public Namespace Namespace { get; internal set; }

        public Tag ToTag() => new Tag(this.Namespace, this.Original);
    }
}
