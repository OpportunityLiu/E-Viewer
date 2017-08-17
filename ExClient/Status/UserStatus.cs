using HtmlAgilityPack;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;

namespace ExClient.Status
{
    public class UserStatus : ObservableObject
    {
        private static readonly Uri infoUri = new Uri(Internal.UriProvider.Eh.RootUri, "home.php");

        internal UserStatus() { }

        private static int deEntitizeAndParse(HtmlNode node)
        {
            return int.Parse(HtmlEntity.DeEntitize(node.InnerText));
        }

        private void analyzeTopList(HtmlNode toplistsDiv)
        {
            var table = toplistsDiv.Element("table").Descendants("table").FirstOrDefault();
            if (table == null)
            {
                this.topLists.Clear();
                return;
            }
            var toremove = new List<int>(Enumerable.Range(0, this.topLists.Count));
            foreach (var toplistRecord in table.Elements("tr"))
            {
                var rankNode = toplistRecord.Descendants("strong").FirstOrDefault();
                var listNode = toplistRecord.Descendants("a").FirstOrDefault();
                if (rankNode == null || listNode == null)
                    continue;
                if (!int.TryParse(HtmlEntity.DeEntitize(rankNode.InnerText).TrimStart('#'), out var rank))
                    continue;
                var link = new Uri(HtmlEntity.DeEntitize(listNode.GetAttributeValue("href", "")));
                if (!int.TryParse(link.Query.Split('=').Last(), out var listID))
                    continue;
                var item = new TopListItem(rank, (TopListName)listID);
                var replaced = false;
                for (var i = 0; i < this.topLists.Count; i++)
                {
                    if (this.topLists[i].Name == item.Name)
                    {
                        this.topLists[i] = item;
                        replaced = true;
                        toremove.Remove(i);
                        break;
                    }
                }
                if (!replaced)
                {
                    this.topLists.Add(item);
                }
            }

            for (var i = toremove.Count - 1; i >= 0; i--)
            {
                this.topLists.RemoveAt(i);
            }
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
                .Select(n => HtmlEntity.DeEntitize(n.InnerText))
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
                var doc = await Client.Current.HttpClient.GetDocumentAsync(infoUri);
                var contentDivs = doc.DocumentNode
                    .Element("html").Element("body").Element("div").Elements("div")
                    .Where(d => d.GetAttributeValue("class", "") == "homebox").ToList();

                analyzeImageLimit(contentDivs[0]);
                var ehTrackerDiv = contentDivs[1];
                var totalGPGainedDiv = contentDivs[2];
                analyzeTopList(contentDivs[3]);
                analyzeModPower(contentDivs[4]);
            });
        }

        #region Image Limits
        private int imageUsage;
        private int imageUsageLimit;
        private int imageUsageRegenerateRatePerMinute;
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
            throw null;
        }
        #endregion

        #region TopList
        private ObservableList<TopListItem> topLists = new ObservableList<TopListItem>();
        public ObservableListView<TopListItem> TopLists => this.topLists.AsReadOnly();
        #endregion

        #region Moderation Power
        private double moderationPower;
        private double moderationPowerBase;
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
