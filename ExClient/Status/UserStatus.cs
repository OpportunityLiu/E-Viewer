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
    public class UserStatus : ObservableObject
    {
        private static readonly Uri infoUri = new Uri(Internal.UriProvider.Eh.RootUri, "home.php");

        internal UserStatus() { }

        private static int deEntitizeAndParse(HtmlNode node)
        {
            return int.Parse(node.InnerText.DeEntitize());
        }

        private void analyzeToplists(HtmlNode toplistsDiv)
        {
            var table = toplistsDiv.Element("table").Descendants("table").FirstOrDefault();
            if (table == null)
            {
                this.toplists.Clear();
                return;
            }
            var newList = new List<ToplistItem>();
            var toremove = new List<int>(Enumerable.Range(0, this.toplists.Count));
            foreach (var toplistRecord in table.Elements("tr"))
            {
                var rankNode = toplistRecord.Descendants("strong").FirstOrDefault();
                var listNode = toplistRecord.Descendants("a").FirstOrDefault();
                if (rankNode == null || listNode == null)
                    continue;
                if (!int.TryParse(rankNode.InnerText.DeEntitize().TrimStart('#'), out var rank))
                    continue;
                var link = new Uri(listNode.GetAttributeValue("href", "").DeEntitize());
                if (!int.TryParse(link.Query.Split('=').Last(), out var listID))
                    continue;
                newList.Add(new ToplistItem(rank, (ToplistName)listID));
            }
            this.toplists.Update(newList);
        }

        private void analyzeImageLimit(HtmlNode imageLimitDiv)
        {
            var values = imageLimitDiv.Descendants("strong").Select(deEntitizeAndParse).ToList();
            this.ImageUsage = values[0];
            this.ImageUsageLimit = values[1];
            this.ImageUsageRegenerateRatePerMinute = values[2];
            this.ImageUsageResetCost = values[3];
        }

        private void analyzeModPower(HtmlNode modPowerDiv)
        {
            this.ModerationPower = deEntitizeAndParse(modPowerDiv.Descendants("div").Last());
            var values = modPowerDiv.Descendants("td")
                .Where(n => n.GetAttributeValue("style", "") == "font-weight:bold")
                .Select(n => n.InnerText.DeEntitize())
                .Where(s => !string.IsNullOrWhiteSpace(s) && s[0] != '=')
                .Select(double.Parse)
                .ToList();
            this.ModerationPowerBase = values[0];
            this.ModerationPowerAwards = values[1];
            this.ModerationPowerTagging = values[2];
            this.ModerationPowerLevel = values[3];
            this.ModerationPowerDonations = values[4];
            this.ModerationPowerForumActivity = values[5];
            this.ModerationPowerUploadsAndHatH = values[6];
            this.ModerationPowerAccountAge = values[7];
        }

        public IAsyncAction RefreshAsync()
        {
            return AsyncInfo.Run(async token => await Task.Run(async () =>
            {
                this.toplists.Clear();
                var doc = await Client.Current.HttpClient.GetDocumentAsync(infoUri);
                var contentDivs = doc.DocumentNode
                    .Element("html").Element("body").Element("div").Elements("div")
                    .Where(d => d.GetAttributeValue("class", "") == "homebox").ToList();

                analyzeImageLimit(contentDivs[0]);
                var ehTrackerDiv = contentDivs[1];
                var totalGPGainedDiv = contentDivs[2];
                analyzeToplists(contentDivs[3]);
                analyzeModPower(contentDivs[4]);
            }, token));
        }

        #region Image Limits
        private int imageUsage;
        private int imageUsageLimit = 5000;
        private int imageUsageRegenerateRatePerMinute = 3;
        private int imageUsageResetCost;
        public int ImageUsage
        {
            get => imageUsage; set => Set(ref imageUsage, value);
        }
        public int ImageUsageLimit
        {
            get => imageUsageLimit; set => Set(ref imageUsageLimit, value);
        }
        public int ImageUsageRegenerateRatePerMinute
        {
            get => imageUsageRegenerateRatePerMinute; set => Set(ref imageUsageRegenerateRatePerMinute, value);
        }
        public int ImageUsageResetCost
        {
            get => imageUsageResetCost; set => Set(ref imageUsageResetCost, value);
        }

        public IAsyncAction ResetImageUsageAsync()
        {
            // TODO:
            throw new NotImplementedException();
        }
        #endregion

        #region Toplist
        private ObservableList<ToplistItem> toplists = new ObservableList<ToplistItem>();
        public ObservableListView<ToplistItem> Toplists => this.toplists.AsReadOnly();
        #endregion

        #region Moderation Power
        private double moderationPower = 1;
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
            get => moderationPower; set => Set(ref moderationPower, value);
        }
        public double ModerationPowerBase
        {
            get => moderationPowerBase; set => Set(ref moderationPowerBase, value);
        }
        public double ModerationPowerAwards
        {
            get => moderationPowerAwards; set => Set(ref moderationPowerAwards, value);
        }
        public double ModerationPowerTagging
        {
            get => moderationPowerTagging; set => Set(ref moderationPowerTagging, value);
        }
        public double ModerationPowerLevel
        {
            get => moderationPowerLevel; set => Set(ref moderationPowerLevel, value);
        }
        public double ModerationPowerDonations
        {
            get => moderationPowerDonations; set => Set(ref moderationPowerDonations, value);
        }
        public double ModerationPowerForumActivity
        {
            get => moderationPowerForumActivity; set => Set(ref moderationPowerForumActivity, value);
        }
        public double ModerationPowerUploadsAndHatH
        {
            get => moderationPowerUploadsAndHatH; set => Set(ref moderationPowerUploadsAndHatH, value);
        }
        public double ModerationPowerAccountAge
        {
            get => moderationPowerAccountAge; set => Set(ref moderationPowerAccountAge, value);
        }
        #endregion
    }
}
