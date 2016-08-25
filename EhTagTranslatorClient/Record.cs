using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using System.Diagnostics;
using NameSpace = ExClient.NameSpace;

namespace EhTagTranslatorClient
{
    [DebuggerDisplay(@"\{{Original} -> {Translated.RawString}\}")]
    public class Record
    {
        private static Regex lineRegex = new Regex(
            $@"^\s*(?<!\\)\|?\s*
            (?<{nameof(Original)}>.*?)
		    \s*(?<!\\)\|\s*
		    (?<{nameof(Translated)}>.*?)
		    \s*(?<!\\)\|\s*
		    (?<{nameof(Introduction)}>.*?)
		    \s*(?<!\\)\|?\s*$", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

        public static IEnumerable<Record> Analyze(IInputStream stream, NameSpace nameSpace)
        {
            using(stream)
            {
                var reader = new StreamReader(stream.AsStreamForRead());
                while(!reader.EndOfStream)
                {
                    var r = AnalyzeLine(reader.ReadLine(), nameSpace);
                    if(r != null)
                        yield return r;
                }
            }
        }

        internal static Record AnalyzeLine(string line, NameSpace nameSpace)
        {
            var match = lineRegex.Match(line);
            if(!match.Success)
                return null;
            var ori = match.Groups[nameof(Original)].Value;
            if(string.IsNullOrEmpty(ori))
                return null;
            var tra = match.Groups[nameof(Translated)].Value;
            var intro = match.Groups[nameof(Introduction)].Value;
            return new Record(nameSpace, unescape(ori), unescape(tra), unescape(intro));
        }

        private static string unescape(string value)
        {
            var sb = new StringBuilder(value);
            sb.Replace(@"\|", @"|");
            sb.Replace(@"\\", @"\");
            sb.Replace("<br>", Environment.NewLine);
            return sb.ToString();
        }

        private Record(NameSpace nameSpace, string original, string translated, string introduction)
        {
            this.NameSpace = nameSpace;
            this.Original = original;
            this.Translated = new MarkdownText(translated);
            this.Introduction = new MarkdownText(introduction);
        }

        public string Original
        {
            get;
        }

        public MarkdownText Translated
        {
            get;
        }

        public MarkdownText Introduction
        {
            get;
        }

        public NameSpace NameSpace
        {
            get;
        }
    }
}
