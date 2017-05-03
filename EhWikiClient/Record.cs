using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using System.Collections.Generic;

namespace EhWikiClient
{
    [System.Diagnostics.DebuggerDisplay(@"\{{Title} => {Japanese}\}")]
    public class Record
    {
        internal Record() { }

        private Record(string title, string japanese, string description, string html, TagType type)
            : this(true)
        {
            this.Title = title;
            this.Japanese = japanese;
            this.Description = description;
            this.DetialHtml = html;
            this.Type = type;
        }

        private Record(bool isValid)
        {
            this.IsValid = isValid;
            this.LastUpdate = DateTimeOffset.UtcNow;
        }

#pragma warning disable CS0649
#pragma warning disable IDE1006 // 命名样式
        private class Response
        {
            public Parse parse;
            public class Parse
            {
                public string title;
                public Text text;
                public class Text
                {
                    [JsonProperty("*")]
                    public string Str;
                }

                public IReadOnlyList<Catogory> categories;
                public class Catogory
                {
                    public string sortkey;
                    [JsonProperty("*")]
                    public string Value;
                }
            }
        }
#pragma warning restore IDE1006 // 命名样式
#pragma warning restore CS0649

        private static Regex reg = new Regex(@"^\s?Japanese\s?:\s?(?<Value>.+?)\s?$", RegexOptions.Multiline | RegexOptions.Compiled);
        private static Regex regd = new Regex(@"^\s?Description\s?:\s?(?<Value>.+?)\s?$", RegexOptions.Multiline | RegexOptions.Compiled);

        internal static Record Load(string json)
        {
            var res = JsonConvert.DeserializeObject<Response>(json);
            if (res.parse == null)
                return new Record(false);
            var str = Windows.Data.Html.HtmlUtilities.ConvertToText(res.parse.text.Str);
            var match = reg.Matches(str);
            var j = (string)null;
            if (match.Count > 0)
                j = string.Join(" | ", match.Cast<Match>().Select(m => m.Groups["Value"].Value));
            var matchd = regd.Match(str);
            var d = (string)null;
            if (matchd.Success)
                d = matchd.Groups["Value"].Value;
            var type = TagType.Unknown;
            if (res.parse.categories != null)
            {
                foreach (var item in res.parse.categories)
                {
                    switch (item.Value)
                    {
                        case "Tag": type |= TagType.Fetish; break;
                        case "Series_Tag": type |= TagType.Series; break;
                        case "Language_Tag": type |= TagType.Language; break;
                        case "Character_Tag": type |= TagType.Character; break;
                        case "Creator_Tag": type |= TagType.Creator; break;
                    }
                }
            }
           return new Record(res.parse.title, j, d, res.parse.text.Str, type);
        }

        internal void Update(Record record)
        {
            this.Japanese = record.Japanese;
            this.Description = record.Description;
            this.IsValid = record.IsValid;
            this.LastUpdate = DateTimeOffset.UtcNow;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Title
        {
            get;
            internal set;
        }

        public string Japanese
        {
            get;
            internal set;
        }

        public string Description
        {
            get;
            internal set;
        }

        public TagType Type
        {
            get;
            internal set;
        }

        private long lastUpdate;

        [NotMapped]
        public DateTimeOffset LastUpdate
        {
            get => DateTimeOffset.FromUnixTimeMilliseconds(lastUpdate);
            set => this.lastUpdate = value.ToUnixTimeMilliseconds();
        }

        public bool IsValid { get; internal set; }

        [NotMapped]
        public string DetialHtml
        {
            get;
            private set;
        }
    }
}
