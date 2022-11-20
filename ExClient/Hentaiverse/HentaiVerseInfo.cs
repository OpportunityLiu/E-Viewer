using HtmlAgilityPack;

using Opportunity.Helpers.Universal.AsyncHelpers;

using System;
using System.Linq;
using System.Text.RegularExpressions;

using Windows.Foundation;
using Windows.Web.Http;

namespace ExClient.HentaiVerse
{
    public static class HentaiVerseInfo
    {
        public static Uri RootUri { get; } = new Uri("https://hentaiverse.org/");

        public static Uri LogOnUri => new Uri(RootUri, $"login.php?ipb_member_id={Client.Current.UserId}&ipb_pass_hash={Client.Current.PassHash}");

        public static event EventHandler<RandomEncounterEventArgs> MonsterEncountered;
        public static event EventHandler<DawnOfDayRewardsEventArgs> DawnOfDayRewardsAwarded;

        public static bool IsEnabled { get; set; } = false;

        private static readonly Uri newsUri = new Uri("https://e-hentai.org/news.php");

        public static IAsyncActionWithProgress<HttpProgress> FetchAsync()
            => Client.Current.HttpClient.GetDocumentAsync(newsUri).ContinueWith(d =>
            {
                // if IsEnabled is true, it will be analyzed in AnalyzePage, in GetDocumentAsync.
                if (!IsEnabled)
                    analyze(d.GetResults());
            });

        private static void analyze(HtmlDocument doc)
        {
            var eventPane = doc.GetElementbyId("eventpane");
            if (eventPane is null)
                return;
            analyzeDOD(eventPane);
            analyzeRE(eventPane);
        }

        private static void analyzeRE(HtmlNode eventPane)
        {
            var div1 = eventPane.Element("div");
            if (div1 is null)
                return;
            var a = eventPane.Descendants("a").FirstOrDefault();
            if (a is null)
                return;
            var uri = a.GetAttribute("href", default(Uri));
            MonsterEncountered?.Invoke(Client.Current, new RandomEncounterEventArgs(uri));
        }

        private static void analyzeDOD(HtmlNode eventPane)
        {
            var p1 = eventPane.Element("p");
            if (p1 is null)
                return;
            if (!p1.GetInnerText().Contains("the dawn of a new day"))
                return;
            var pData = eventPane.LastChild;
            var data = new System.Collections.Generic.Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in reg.Matches(pData.InnerHtml).Cast<Match>())
            {
                var key = item.Groups["name"].Value;
                switch (key.ToLowerInvariant())
                {
                case "gp":
                case "gps":
                    key = "GP"; break;
                case "credit":
                case "credits":
                    key = "Credits"; break;
                case "hath":
                case "haths":
                    key = "Hath"; break;
                case "exp":
                    key = "EXP"; break;
                }
                data[key] = double.Parse(item.Groups["value"].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture);
            }
            if (data.IsEmpty())
                return;
            DawnOfDayRewardsAwarded?.Invoke(Client.Current, new DawnOfDayRewardsEventArgs(data));
        }

        private static readonly Regex reg = new Regex(@"<strong>(?<value>[^<]+)</strong>\s?\b(?<name>.+?)\b(!|\sand|,)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        internal static void AnalyzePage(HtmlDocument doc)
        {
            if (IsEnabled)
                analyze(doc);
        }
    }
}
