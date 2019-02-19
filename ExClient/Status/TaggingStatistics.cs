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

        public int Count { get; set; } = 0;
        public double StartedAccuracy { get; set; } = double.NaN;
        public int StartedCount { get; set; } = 0;
        public double VotedAccuracy { get; set; } = double.NaN;
        public int VotedCount { get; set; } = 0;

        private readonly ObservableList<TaggingRecord> records = new ObservableList<TaggingRecord>();
        public ObservableListView<TaggingRecord> Records => records.AsReadOnly();

        private static readonly Regex regex = new Regex(@"Tags:\s*(\d+)\s*\(\d+\s*recent\)\s*Started Accuracy\s*=\s*(\S+)\s*of\s*(\d+)\s*Voted Accuracy\s*=\s*(\S+)\s*of\s*(\d+)", RegexOptions.Compiled);

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

                var overall = body.Element("div").Elements("div").Last().GetInnerText();
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
                if (table != null)
                    records.Update(table.Elements("tr").Skip(1).Select(item => new TaggingRecord(item)).ToList());
                else if (match.Success)
                    records.Clear();
                OnPropertyChanged(default(string));
            }, token));
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
