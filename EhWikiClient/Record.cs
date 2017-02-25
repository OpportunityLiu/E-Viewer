using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace EhWikiClient
{
    [System.Diagnostics.DebuggerDisplay(@"\{{Title} -> {Japanese}\}")]
    public class Record
    {
#pragma warning disable CS0649
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
                    public string str;
                }
            }
        }
#pragma warning restore CS0649

        private static Regex reg = new Regex(@"^\s?Japanese\s?:\s?(?<Value>.+?)\s?$", RegexOptions.Multiline | RegexOptions.Compiled);

        [JsonConstructor]
        internal Record(string t, string j)
        {
            this.Title = t;
            this.Japanese = j;
        }

        private Record(string title, string japanese, string html)
            : this(title, japanese)
        {
            this.DetialHtml = html;
        }

        internal static Record Load(string json)
        {
            var res = JsonConvert.DeserializeObject<Response>(json);
            if(res.parse == null)
                return null;
            var str = Windows.Data.Html.HtmlUtilities.ConvertToText(res.parse.text.str);
            var match = reg.Match(str);
            var j = (string)null;
            if(match.Success)
                j = match.Groups["Value"].Value;
            return new Record(res.parse.title, j, res.parse.text.str);
        }

        [JsonProperty("t")]
        public string Title
        {
            get;
        }

        [JsonProperty("j")]
        public string Japanese
        {
            get;
        }

        [JsonIgnore]
        public string DetialHtml
        {
            get;
            private set;
        }
    }
}
