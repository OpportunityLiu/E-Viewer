using HtmlAgilityPack;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Web.Http;

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
            return AsyncInfo.Run(async token =>
            {
                this.toplists.Clear();
                var getDoc = Client.Current.HttpClient.GetDocumentAsync(infoUri);
                token.Register(getDoc.Cancel);
                var doc = await getDoc;
                analyzeDoc(doc);
            });
        }

        private bool analyzeDoc(HtmlDocument doc)
        {
            if (doc == null)
                return false;
            var contentDivs = doc.DocumentNode
               .Element("html").Element("body").Element("div").Elements("div")
               .Where(d => d.GetAttributeValue("class", "") == "homebox").ToList();
            if (contentDivs.Count != 5)
                return false;

            analyzeImageLimit(contentDivs[0]);
            var ehTrackerDiv = contentDivs[1];
            var totalGPGainedDiv = contentDivs[2];
            analyzeToplists(contentDivs[3]);
            analyzeModPower(contentDivs[4]);
            return true;
        }

        #region Image Limits
        private int imageUsage;
        private int imageUsageLimit = 5000;
        private int imageUsageRegenerateRatePerMinute = 3;
        private int imageUsageResetCost;
        public int ImageUsage
        {
            get => this.imageUsage; set => Set(ref this.imageUsage, value);
        }
        public int ImageUsageLimit
        {
            get => this.imageUsageLimit; set => Set(ref this.imageUsageLimit, value);
        }
        public int ImageUsageRegenerateRatePerMinute
        {
            get => this.imageUsageRegenerateRatePerMinute; set => Set(ref this.imageUsageRegenerateRatePerMinute, value);
        }
        public int ImageUsageResetCost
        {
            get => this.imageUsageResetCost; set => Set(ref this.imageUsageResetCost, value);
        }

        public IAsyncAction ResetImageUsageAsync()
        {
            return AsyncInfo.Run(async token =>
            {
                var content = new HttpFormUrlEncodedContent(new[] { KeyValuePair.Create("act", "limits") });
                var p = Client.Current.HttpClient.PostAsync(infoUri, content);
                token.Register(p.Cancel);
                var r = await p;
                var html = await r.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                if (!analyzeDoc(doc))
                    await RefreshAsync();
            });
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
            get => this.moderationPower; set => Set(ref this.moderationPower, value);
        }
        public double ModerationPowerBase
        {
            get => this.moderationPowerBase; set => Set(ref this.moderationPowerBase, value);
        }
        public double ModerationPowerAwards
        {
            get => this.moderationPowerAwards; set => Set(ref this.moderationPowerAwards, value);
        }
        public double ModerationPowerTagging
        {
            get => this.moderationPowerTagging; set => Set(ref this.moderationPowerTagging, value);
        }
        public double ModerationPowerLevel
        {
            get => this.moderationPowerLevel; set => Set(ref this.moderationPowerLevel, value);
        }
        public double ModerationPowerDonations
        {
            get => this.moderationPowerDonations; set => Set(ref this.moderationPowerDonations, value);
        }
        public double ModerationPowerForumActivity
        {
            get => this.moderationPowerForumActivity; set => Set(ref this.moderationPowerForumActivity, value);
        }
        public double ModerationPowerUploadsAndHatH
        {
            get => this.moderationPowerUploadsAndHatH; set => Set(ref this.moderationPowerUploadsAndHatH, value);
        }
        public double ModerationPowerAccountAge
        {
            get => this.moderationPowerAccountAge; set => Set(ref this.moderationPowerAccountAge, value);
        }
        #endregion
    }
}
