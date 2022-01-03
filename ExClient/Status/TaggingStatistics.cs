using HtmlAgilityPack;

using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Collections;

using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Windows.Foundation;

namespace ExClient.Status
{
    public sealed class TaggingStatistics : ObservableObject
    {
        internal TaggingStatistics() { }

        public double StartedAccuracy { get; set; } = double.NaN;
        public int StartedCount { get; set; } = 0;
        public double VotedAccuracy { get; set; } = double.NaN;
        public int VotedCount { get; set; } = 0;

        private readonly ObservableList<TaggingRecord> records = new ObservableList<TaggingRecord>();
        public ObservableListView<TaggingRecord> Records => records.AsReadOnly();

        private static readonly Regex _StatsRegex = new(@"^\s*([\d.]+%)\s*\((\d+)\)\s*$", RegexOptions.Compiled);

        public IAsyncAction RefreshAsync()
        {
            return AsyncInfo.Run(async token => await Task.Run(async () =>
            {
                var uid = Client.Current.UserId;
                if (uid < 0)
                    throw new InvalidOperationException("Hasn't log in");
                var getPage = Client.Current.HttpClient.GetDocumentAsync(new Uri($"https://e-hentai.org/tools.php?act=taglist&uid={uid}"));
                token.Register(getPage.Cancel);
                var page = await getPage;
                var body = page.DocumentNode.Element("html").Element("body");

                var tagstats = page.GetElementbyId("tagstats")?.Elements("tr")?.ToArray();
                if (tagstats != null && tagstats.Length == 3)
                {
                    var stared = _StatsRegex.Match(tagstats[1].LastChild?.GetInnerText() ?? "");
                    var voted = _StatsRegex.Match(tagstats[2].LastChild?.GetInnerText() ?? "");
                    if (stared.Success)
                    {
                        StartedAccuracy = _ParsePercetage(stared.Groups[1].Value);
                        StartedCount = int.Parse(stared.Groups[2].Value);
                    }
                    if (voted.Success)
                    {
                        VotedAccuracy = _ParsePercetage(voted.Groups[1].Value);
                        VotedCount = int.Parse(voted.Groups[2].Value);
                    }
                }

                var table = body.Element("table");
                if (table != null)
                    records.Update(table.Elements("tr").Skip(1).Select(item => new TaggingRecord(item)).ToList());
                else if(tagstats != null)
                    records.Clear();
                OnPropertyChanged(default(string));
            }, token));
        }

        private static double _ParsePercetage(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return double.NaN;
            if (str.EndsWith("%") && double.TryParse(str.Substring(0, str.Length - 1), out var r))
                return r / 100;
            return double.NaN;
        }
    }
}
