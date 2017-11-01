using ExClient.Api;
using HtmlAgilityPack;
using Opportunity.MvvmUniverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ExClient.Galleries.Rating
{
    public class RatingStatus : ObservableObject
    {
        private readonly Gallery owner;

        internal RatingStatus(Gallery owner)
        {
            this.owner = owner;
        }

        private double averageScore;
        public double AverageScore { get => averageScore; internal set => Set(ref averageScore, value); }

        private Score? userScore;
        public Score? UserScore { get => userScore; private set => Set(ref userScore, value); }

        private long ratingCount = -1;
        public long RatingCount { get => ratingCount; private set => Set(ref ratingCount, value); }

        private static Regex regAvg = new Regex(@"var\s+average_rating\s*=\s*([\.\d]+)", RegexOptions.Compiled);
        private static Regex regDisp = new Regex(@"var\s+display_rating\s*=\s*([\.\d]+)", RegexOptions.Compiled);

        internal void AnalyzeDocument(HtmlDocument doc)
        {
            var avgS = 0d;
            var avg = regAvg.Match(doc.DocumentNode.OuterHtml);
            if (avg.Success)
                avgS = double.Parse(avg.Groups[1].Value);

            var disp = regDisp.Match(doc.DocumentNode.OuterHtml);
            var dispS = 0d;
            if (disp.Success)
                dispS = double.Parse(disp.Groups[1].Value);

            var rImage = doc.GetElementbyId("rating_image");
            var rImageClass = rImage?.GetAttributeValue("class", null);

            var ratingCount = -1L;
            var rCount = doc.GetElementbyId("rating_count");
            if (rCount != null && uint.TryParse(rCount.InnerText.DeEntitize(), out var c))
                ratingCount = c;

            analyzeData(avgS, dispS, rImageClass, ratingCount);
        }

        private static Regex regPos = new Regex(@"background-position\s*:\s*([-\d]+)\s*px\s+([-\d]+)\s*px", RegexOptions.Compiled);

        internal void AnalyzeNode(HtmlNode ratingImageDivNode)
        {
            var divClass = ratingImageDivNode?.GetAttributeValue("class", null);
            if (divClass == null)
                return;
            if (!divClass.Contains("ir"))
                return;
            if (!userRated(divClass))
                return;
            var style = ratingImageDivNode.GetAttributeValue("style", "");
            var pos = regPos.Match(style);
            if (!pos.Success)
                return;
            var x = int.Parse(pos.Groups[1].Value);
            var y = int.Parse(pos.Groups[2].Value);
            this.UserScore = (Score)((x + 80) / 8 - (y < -10 ? 1 : 0));
        }

        private static bool userRated(string classes) => (classes.Contains("irr") || classes.Contains("irg") || classes.Contains("irb"));

        private void analyzeData(double averageScore, double displayScore, string ratingImageClass, long ratingCount)
        {
            this.AverageScore = averageScore;
            this.RatingCount = ratingCount;

            if (ratingImageClass != null && userRated(ratingImageClass))
                this.UserScore = displayScore.ToScore();
            else
                this.UserScore = null;

        }

        public IAsyncAction RatingAsync(Score rating)
        {
            return AsyncInfo.Run(async token =>
            {
                var reqInfo = new RatingRequest(this.owner, rating);
                var r = reqInfo.GetResponseAsync();
                token.Register(r.Cancel);
                var result = await r;
                analyzeData(result.AverageScore, result.UserScore, result.RatingImageClass, result.RatingCount);
            });
        }
    }
}
