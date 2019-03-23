using ExClient.Tagging;
using Newtonsoft.Json;
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
    public class Record
    {
        internal Record() { }

        [JsonConstructor]
        internal Record(string name, string intro)
        {
            Translated = name;
            Introduction = intro;
        }

        internal Record(Namespace @namespace, string raw, string name, string intro)
            : this(name, intro)
        {
            Namespace = @namespace;
            Original = raw;
        }

        internal static Record Combine(Record r1, Record r2)
        {
            if (r1.Original != r2.Original)
            {
                throw new InvalidOperationException();
            }

            if (r1.Namespace != r2.Namespace)
            {
                throw new InvalidOperationException();
            }

            string translated, intro;
            if (r1.Translated == r2.Translated)
            {
                translated = r1.Translated;
            }
            else
            {
                translated = $@"{r1.Translated} | {r2.Translated}";
            }
            if (string.IsNullOrWhiteSpace(r1.Introduction))
            {
                intro = r2.Introduction;
            }
            else if (string.IsNullOrWhiteSpace(r2.Introduction))
            {
                intro = r1.Introduction;
            }
            else
            {
                intro = $"{r1.Introduction}{Environment.NewLine}{Environment.NewLine}{r2.Introduction}";
            }

            return new Record(r1.Namespace, r1.Original, translated, intro);
        }

        public string Original { get; internal set; }

        public string Translated { get; internal set; }

        public string Introduction { get; internal set; }

        public Namespace Namespace { get; internal set; }


        public Tag ToTag() => new Tag(Namespace, Original);
    }
}
