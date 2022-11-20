using HtmlAgilityPack;

using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Collections;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

using Windows.Foundation;

namespace ExClient.Status
{
    public sealed class UserStatus : ObservableObject
    {
        private static readonly Uri infoUri = new Uri(Internal.DomainProvider.Eh.RootUri, "home.php");

        internal UserStatus() { }

        private static int deEntitizeAndParse(HtmlNode node)
        {
            return int.Parse(node.GetInnerText(), System.Globalization.NumberStyles.Integer);
        }

        private void analyzeToplists(HtmlNode toplistsDiv)
        {
            var table = toplistsDiv.Element("table").Descendants("table").FirstOrDefault();
            if (table is null)
            {
                toplists.Clear();
                return;
            }
            var newList = new List<ToplistItem>();
            foreach (var toplistRecord in table.Elements("tr"))
            {
                var rankNode = toplistRecord.Descendants("strong").FirstOrDefault();
                var listNode = toplistRecord.Descendants("a").FirstOrDefault();
                if (rankNode is null)
                    throw new InvalidOperationException("rankNode not found");
                if (listNode is null)
                    throw new InvalidOperationException("listNode not found");
                var rank = int.Parse(rankNode.GetInnerText().TrimStart('#'), System.Globalization.NumberStyles.Integer);
                var link = listNode.GetAttribute("href", default(Uri));
                var listID = int.Parse(link.Query.Split('=').Last(), System.Globalization.NumberStyles.Integer);
                newList.Add(new ToplistItem(rank, (ToplistName)listID));
            }
            toplists.Update(newList);
        }

        private void analyzeImageLimit(HtmlNode imageLimitDiv)
        {
            var values = imageLimitDiv.Descendants("strong").Select(deEntitizeAndParse).ToList();
            if (values.Count != 3)
                throw new InvalidOperationException("Wrong values.Count from analyzeImageLimit");
            ImageUsage = values[0];
            ImageUsageLimit = values[1];
            ImageUsageResetCost = values[2];
        }

        private void analyzeModPower(HtmlNode modPowerDiv)
        {
            ModerationPower = deEntitizeAndParse(modPowerDiv.Descendants("div").Last());
            var values = modPowerDiv.Descendants("td")
                .Where(n => n.GetAttribute("style", "").Contains("font-weight:bold"))
                .Select(n => n.GetInnerText())
                .Where(s => !string.IsNullOrWhiteSpace(s) && s[0] != '=')
                .Select(double.Parse)
                .ToList();
            if (values.Count != 8)
                throw new InvalidOperationException("Wrong values.Count from analyzeModPower");
            ModerationPowerBase = values[0];
            ModerationPowerAwards = values[1];
            ModerationPowerTagging = values[2];
            ModerationPowerLevel = values[3];
            ModerationPowerDonations = values[4];
            ModerationPowerForumActivity = values[5];
            ModerationPowerUploadsAndHatH = values[6];
            ModerationPowerAccountAge = values[7];
            ModerationPowerCaculated = values.Take(3).Sum() + Math.Min(values.Skip(3).Take(5).Sum(), 25);
        }

        public async Task RefreshAsync()
        {
            var doc = await Client.Current.HttpClient.GetDocumentAsync(infoUri);
            try
            {
                analyzeDoc(doc);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(LocalizedStrings.Resources.WrongApiResponse, ex);
            }
        }

        private void analyzeDoc(HtmlDocument doc)
        {
            if (doc is null)
                throw new ArgumentNullException(nameof(doc));
            var contentDivs = doc.DocumentNode
               .Element("html").Element("body")
               .Element("div", "stuffbox")
               .Elements("div", "homebox").ToList();
            if (contentDivs.Count != 5)
                throw new InvalidOperationException("Wrong `homebox` count");
            analyzeImageLimit(contentDivs[0]);
            var ehTrackerDiv = contentDivs[1];
            var totalGPGainedDiv = contentDivs[2];
            analyzeToplists(contentDivs[3]);
            analyzeModPower(contentDivs[4]);
        }

        #region Image Limits
        private int imageUsage;
        private int imageUsageLimit = 5000;
        private int imageUsageResetCost;
        public int ImageUsage
        {
            get => imageUsage; private set => Set(ref imageUsage, value);
        }
        public int ImageUsageLimit
        {
            get => imageUsageLimit; private set => Set(ref imageUsageLimit, value);
        }
        public int ImageUsageResetCost
        {
            get => imageUsageResetCost; private set => Set(ref imageUsageResetCost, value);
        }

        public IAsyncAction ResetImageUsageAsync()
        {
            return AsyncInfo.Run(async token =>
            {
                var p = Client.Current.HttpClient.PostAsync(infoUri, new KeyValuePair<string, string>("act", "limits"));
                token.Register(p.Cancel);
                var r = await p;
                var html = await r.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                try
                {
                    analyzeDoc(doc);
                }
                catch (Exception)
                {
                    await RefreshAsync();
                }
            });
        }
        #endregion

        #region Toplist
        private readonly ObservableList<ToplistItem> toplists = new ObservableList<ToplistItem>();
        public ObservableListView<ToplistItem> Toplists => toplists.AsReadOnly();
        #endregion

        #region Moderation Power
        private double moderationPower = 1;
        private double moderationPowerCaculated = 1;
        private double moderationPowerBase = 1;
        private double moderationPowerAwards;
        private double moderationPowerTagging;
        private double moderationPowerLevel;
        private double moderationPowerDonations;
        private double moderationPowerForumActivity;
        private double moderationPowerUploadsAndHatH;
        private double moderationPowerAccountAge;
        public double ModerationPower
        {
            get => moderationPower; private set => Set(ref moderationPower, value);
        }
        public double ModerationPowerCaculated
        {
            get => moderationPowerCaculated; private set => Set(ref moderationPowerCaculated, value);
        }
        public double ModerationPowerBase
        {
            get => moderationPowerBase; private set => Set(ref moderationPowerBase, value);
        }
        public double ModerationPowerAwards
        {
            get => moderationPowerAwards; private set => Set(ref moderationPowerAwards, value);
        }
        public double ModerationPowerTagging
        {
            get => moderationPowerTagging; private set => Set(ref moderationPowerTagging, value);
        }
        public double ModerationPowerLevel
        {
            get => moderationPowerLevel; private set => Set(ref moderationPowerLevel, value);
        }
        public double ModerationPowerDonations
        {
            get => moderationPowerDonations; private set => Set(ref moderationPowerDonations, value);
        }
        public double ModerationPowerForumActivity
        {
            get => moderationPowerForumActivity; private set => Set(ref moderationPowerForumActivity, value);
        }
        public double ModerationPowerUploadsAndHatH
        {
            get => moderationPowerUploadsAndHatH; private set => Set(ref moderationPowerUploadsAndHatH, value);
        }
        public double ModerationPowerAccountAge
        {
            get => moderationPowerAccountAge; private set => Set(ref moderationPowerAccountAge, value);
        }
        #endregion
    }
}
