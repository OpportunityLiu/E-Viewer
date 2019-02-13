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
                this.toplists.Clear();
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
            this.toplists.Update(newList);
        }

        private void analyzeImageLimit(HtmlNode imageLimitDiv)
        {
            var values = imageLimitDiv.Descendants("strong").Select(deEntitizeAndParse).ToList();
            if (values.Count != 4)
                throw new InvalidOperationException("Wrong values.Count from analyzeImageLimit");
            this.ImageUsage = values[0];
            this.ImageUsageLimit = values[1];
            this.ImageUsageRegenerateRatePerMinute = values[2];
            this.ImageUsageResetCost = values[3];
        }

        private void analyzeModPower(HtmlNode modPowerDiv)
        {
            this.ModerationPower = deEntitizeAndParse(modPowerDiv.Descendants("div").Last());
            var values = modPowerDiv.Descendants("td")
                .Where(n => n.GetAttribute("style", "").Contains("font-weight:bold"))
                .Select(n => n.GetInnerText())
                .Where(s => !string.IsNullOrWhiteSpace(s) && s[0] != '=')
                .Select(double.Parse)
                .ToList();
            if (values.Count != 8)
                throw new InvalidOperationException("Wrong values.Count from analyzeModPower");
            this.ModerationPowerBase = values[0];
            this.ModerationPowerAwards = values[1];
            this.ModerationPowerTagging = values[2];
            this.ModerationPowerLevel = values[3];
            this.ModerationPowerDonations = values[4];
            this.ModerationPowerForumActivity = values[5];
            this.ModerationPowerUploadsAndHatH = values[6];
            this.ModerationPowerAccountAge = values[7];
            this.ModerationPowerCaculated = values.Take(3).Sum() + Math.Min(values.Skip(3).Take(5).Sum(), 25);
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
               .Element("html").Element("body").Element("div")
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
        private int imageUsageRegenerateRatePerMinute = 3;
        private int imageUsageResetCost;
        public int ImageUsage
        {
            get => this.imageUsage; private set => Set(ref this.imageUsage, value);
        }
        public int ImageUsageLimit
        {
            get => this.imageUsageLimit; private set => Set(ref this.imageUsageLimit, value);
        }
        public int ImageUsageRegenerateRatePerMinute
        {
            get => this.imageUsageRegenerateRatePerMinute; private set => Set(ref this.imageUsageRegenerateRatePerMinute, value);
        }
        public int ImageUsageResetCost
        {
            get => this.imageUsageResetCost; private set => Set(ref this.imageUsageResetCost, value);
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
        public ObservableListView<ToplistItem> Toplists => this.toplists.AsReadOnly();
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
            get => this.moderationPower; private set => Set(ref this.moderationPower, value);
        }
        public double ModerationPowerCaculated
        {
            get => this.moderationPowerCaculated; private set => Set(ref this.moderationPowerCaculated, value);
        }
        public double ModerationPowerBase
        {
            get => this.moderationPowerBase; private set => Set(ref this.moderationPowerBase, value);
        }
        public double ModerationPowerAwards
        {
            get => this.moderationPowerAwards; private set => Set(ref this.moderationPowerAwards, value);
        }
        public double ModerationPowerTagging
        {
            get => this.moderationPowerTagging; private set => Set(ref this.moderationPowerTagging, value);
        }
        public double ModerationPowerLevel
        {
            get => this.moderationPowerLevel; private set => Set(ref this.moderationPowerLevel, value);
        }
        public double ModerationPowerDonations
        {
            get => this.moderationPowerDonations; private set => Set(ref this.moderationPowerDonations, value);
        }
        public double ModerationPowerForumActivity
        {
            get => this.moderationPowerForumActivity; private set => Set(ref this.moderationPowerForumActivity, value);
        }
        public double ModerationPowerUploadsAndHatH
        {
            get => this.moderationPowerUploadsAndHatH; private set => Set(ref this.moderationPowerUploadsAndHatH, value);
        }
        public double ModerationPowerAccountAge
        {
            get => this.moderationPowerAccountAge; private set => Set(ref this.moderationPowerAccountAge, value);
        }
        #endregion
    }
}
