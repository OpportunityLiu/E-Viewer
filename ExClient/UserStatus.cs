using HtmlAgilityPack;
using Opportunity.MvvmUniverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ExClient
{
    public class UserStatus : ObservableObject
    {
        private static readonly Uri infoUri = new Uri(Internal.UriProvider.Eh.RootUri, "home.php");

        internal UserStatus() { }

        public IAsyncAction RefreshAsync()
        {
            return AsyncInfo.Run(async token =>
            {
                var doc = await Client.Current.HttpClient.GetDocumentAsync(infoUri);
                var contentDivs = doc.DocumentNode
                    .Element("html").Element("body").Element("div").Elements("div")
                    .Where(d => d.GetAttributeValue("class", "") == "homebox").ToList();
                int deEntitizeAndParse(HtmlNode node)
                {
                    return int.Parse(HtmlEntity.DeEntitize(node.InnerText));
                }

                {
                    var imageLimitDiv = contentDivs[0];
                    var values = imageLimitDiv.Descendants("strong").Select(deEntitizeAndParse).ToList();
                    this.ImageUsage = values[0];
                    this.ImageUsageLimit = values[1];
                    this.ImageUsageRegenerateRatePerMinute = values[2];
                    this.ImageUsageResetCost = values[3];
                }
                var ehTrackerDiv = contentDivs[1];
                var totalGPGainedDiv = contentDivs[2];
                var toplistsDiv = contentDivs[3];
                {
                    var modPowerDiv = contentDivs[4];
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
        //private IReadOnlyList<TopListItem> topLists;
        //public IReadOnlyList<TopListItem> TopLists
        //{
        //    get => topLists; set => Set(ref topLists, value);
        //}
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
