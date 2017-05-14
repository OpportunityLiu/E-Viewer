using ExClient;
using ExClient.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExViewer.ViewModels
{
    public class AutoCompletion
    {
        public static void SplitKeyword(string input, out Namespace lastwordNs, out string lastword, out string previous)
        {
            if (string.IsNullOrEmpty(input))
            {
                previous = "";
                lastword = null;
                lastwordNs = Namespace.Unknown;
                return;
            }
            var quoteCount = input.Count(c => c == '"');
            var lastterm = default(string);
            if (quoteCount == 0)
            {
                var index = input.LastIndexOf(' ') + 1;
                lastterm = input.Substring(index);
                previous = input.Substring(0, input.Length - lastterm.Length);
            }
            else if (quoteCount % 2 == 0)
            {
                if (input[input.Length - 1] != '"')
                {
                    var qp = input.LastIndexOf('"');
                    var sp = input.LastIndexOf(' ', input.Length - 1, input.Length - qp);
                    if (sp != -1)
                    {
                        lastterm = input.Substring(sp + 1);
                        previous = input.Substring(0, input.Length - lastterm.Length);
                    }
                    else
                    {
                        lastterm = input.Substring(qp + 1);
                        previous = input.Substring(0, input.Length - lastterm.Length) + " ";
                    }
                }
                else
                {
                    lastterm = default(string);
                    previous = input;
                }
            }
            else
            {
                var qp = input.LastIndexOf('"');
                var sp = input.LastIndexOf(' ', qp, qp + 1);
                if (qp == 0)
                {
                    previous = "";
                    lastterm = input.Substring(qp + 1).Trim();
                }
                else if (sp == -1)
                {
                    previous = "";
                    lastterm = input;
                }
                else
                {
                    previous = input.Substring(0, sp + 1);
                    lastterm = input.Substring(sp + 1);
                }
            }
            if (string.IsNullOrEmpty(lastterm))
            {
                lastword = null;
                lastwordNs = Namespace.Unknown;
                return;
            }
            if (lastterm[0] == '-')
            {
                lastterm = lastterm.Substring(1);
                previous = previous + "-";
            }
            var splited = lastterm.Split(tagSplit, 2);
            if (splited.Length == 1)
            {
                lastwordNs = Namespace.Misc;
                lastword = lastterm;
            }
            else
            {
                if (!NamespaceExtention.TryParse(splited[0], out lastwordNs))
                    lastwordNs = Namespace.Unknown;
                lastword = splited[1];
            }
            lastword = lastword.Trim(wordTrim);
        }

        private static readonly char[] wordTrim = new[] { '"', '$', ' ' };
        private static readonly char[] tagSplit = new[] { ':' };

        private AutoCompletion(string content)
        {
            this.Content = content;
        }

        public override string ToString()
        {
            return this.Content;
        }

        public string Content { get; private set; }

        internal static IEnumerable<AutoCompletion> GetCompletions(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return getCompletionsWithEmptyInput();
            var quoteCount = input.Count(c => c == '\"');
            if (quoteCount % 2 == 0)
                return getCompletionsWithQuoteFinished(input);
            else
                return getCompletionsWithQuoteUnfinished(input);
        }

        static AutoCompletion()
        {
            var ns = Enum.GetNames(typeof(Namespace)).ToList();
            ns.Remove(Namespace.Misc.ToString());
            ns.Remove(Namespace.Unknown.ToString());
            for (var i = 0; i < ns.Count; i++)
            {
                ns[i] = ns[i].ToLowerInvariant();
            }
            ns.Add("uploader");
            namedNamespaces = ns.AsReadOnly();
        }

        private static readonly IReadOnlyList<string> namedNamespaces;

        private static IEnumerable<AutoCompletion> getCompletionsWithEmptyInput()
        {
            yield break;
        }

        private static IEnumerable<AutoCompletion> getCompletionsWithQuoteUnfinished(string input)
        {
            var lastChar = input[input.Length - 1];
            switch (lastChar)
            {
            case ' ':
            case ':':
            case '"':
                yield break;
            case '$':
                yield return new AutoCompletion($"{input}\"");
                yield break;
            case '-':
            default:
                yield return new AutoCompletion($"{input}\"");
                yield return new AutoCompletion($"{input}$\"");
                yield break;
            }
        }

        private static IEnumerable<AutoCompletion> getCompletionsWithQuoteFinished(string input)
        {
            var lastChar = input[input.Length - 1];
            switch (lastChar)
            {
            case ' ':
            case '-':
                // Too many results
                //foreach(var item in namedNamespaces)
                //{
                //    yield return new AutoCompletion($"{input}{item}:");
                //}
                yield break;
            case ':':
                yield return new AutoCompletion($"{input}\"");
                yield break;
            case '"':
            case '$':
                yield break;
            default:
                var index = input.LastIndexOf(' ') + 1;
                var lastTerm = input.Substring(index);
                if (lastTerm.Length > 0 && lastTerm.All(c => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')))
                {
                    var beforeLastTerm = input.Substring(0, input.Length - lastTerm.Length);
                    foreach (var item in namedNamespaces)
                    {
                        if (item.StartsWith(lastTerm, StringComparison.OrdinalIgnoreCase))
                            yield return new AutoCompletion($"{beforeLastTerm}{item}:");
                    }
                }
                yield break;
            }
        }
    }
}