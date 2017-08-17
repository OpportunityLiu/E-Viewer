using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace ExClient.Status
{
    public sealed class TaggingStatistics : ObservableObject
    {
        internal TaggingStatistics() { }

        public int Count { get; set; }
        public double StartedAccuracy { get; set; }
        public int StartedCount { get; set; }
        public double VotedAccuracy { get; set; }
        public int VotedCount { get; set; }

        private readonly ObservableList<TaggingRecord> records = new ObservableList<TaggingRecord>();
        public ObservableListView<TaggingRecord> Records => this.records.AsReadOnly();

        private static readonly Regex regex = new Regex(@"Tags:\s*(\d+)\s*\(\d+\s*recent\)\s*Started Accuracy\s*=\s*(\S+)\s*of\s*(\d+)\s*Voted Accuracy\s*=\s*(\S+)\s*of\s*(\d+)", RegexOptions.Compiled);

        public IAsyncAction RefreshAsync()
        {
            return AsyncInfo.Run(async token =>
            {
                var uid = Client.Current.UserID;
                if (uid < 0)
                    throw new InvalidOperationException("Hasn't log in");
                var page = await Client.Current.HttpClient.GetDocumentAsync(new Uri($"https://e-hentai.org/tools.php?act=taglist&uid={uid}"));
                var body = page.DocumentNode.Element("html").Element("body");

                var overall = HtmlEntity.DeEntitize(body.Element("div").Elements("div").Last().InnerText);
                var match = regex.Match(overall);
                if (match.Success)
                {
                    Count = int.Parse(match.Groups[1].Value);
                    StartedAccuracy = parsePercetage(match.Groups[2].Value);
                    StartedCount = int.Parse(match.Groups[3].Value);
                    VotedAccuracy = parsePercetage(match.Groups[4].Value);
                    VotedCount = int.Parse(match.Groups[5].Value);
                }

                var table = body.Element("table");
                this.records.Clear();
                if (table != null)
                {
                    this.records.AddRange(table.Elements("tr").Skip(1).Select(t => new TaggingRecord(t)));
                }
                OnPropertyChanged(default(string));
            });
        }

        private static double parsePercetage(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return double.NaN;
            if (str.EndsWith("%") && double.TryParse(str.Substring(0, str.Length - 1), out var r))
                return r / 100;
            return double.NaN;
        }
    }
}
