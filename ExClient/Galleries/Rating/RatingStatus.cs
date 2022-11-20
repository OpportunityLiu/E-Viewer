using ExClient.Api;

using HtmlAgilityPack;

using Opportunity.MvvmUniverse;

using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;

using Windows.Foundation;

namespace ExClient.Galleries.Rating
{
    [DebuggerDisplay(@"\{{userScore} - {averageScore}({ratingCount})\}")]
    public class RatingStatus : ObservableObject
    {
        internal RatingStatus(Gallery owner)
        {
            Owner = owner;
        }

        private double _AverageScore;
        public double AverageScore { get => _AverageScore; internal set => Set(ref _AverageScore, value); }

        private Score _UserScore;
        public Score UserScore { get => _UserScore; private set => Set(ref _UserScore, value); }

        private long _RatingCount = -1;
        public long RatingCount { get => _RatingCount; private set => Set(ref _RatingCount, value); }

        public Gallery Owner { get; }

        private static Regex _RegAvg = new Regex(@"var\s+average_rating\s*=\s*([\.\d]+)", RegexOptions.Compiled);
        private static Regex _RegDisp = new Regex(@"var\s+display_rating\s*=\s*([\.\d]+)", RegexOptions.Compiled);

        internal void AnalyzeDocument(HtmlDocument doc)
        {
            var avgS = 0d;
            var avg = _RegAvg.Match(doc.DocumentNode.OuterHtml);
            if (avg.Success)
            {
                avgS = double.Parse(avg.Groups[1].Value);
            }

            var disp = _RegDisp.Match(doc.DocumentNode.OuterHtml);
            var dispS = 0d;
            if (disp.Success)
            {
                dispS = double.Parse(disp.Groups[1].Value);
            }

            var rImage = doc.GetElementbyId("rating_image");
            var rImageClass = rImage?.GetAttributeValue("class", null);

            var ratingCount = -1L;
            var rCount = doc.GetElementbyId("rating_count");
            if (rCount != null && uint.TryParse(rCount.GetInnerText(), out var c))
            {
                ratingCount = c;
            }

            _AnalyzeData(avgS, dispS, rImageClass, ratingCount);
        }

        private static Regex _RegPos = new Regex(@"background-position\s*:\s*([-\d]+)\s*px\s+([-\d]+)\s*px", RegexOptions.Compiled);

        internal void AnalyzeNode(HtmlNode ratingImageDivNode)
        {
            var divClass = ratingImageDivNode?.GetAttributeValue("class", null);
            if (divClass is null)
            {
                return;
            }

            if (!divClass.Contains("ir"))
            {
                return;
            }

            if (!_UserRated(divClass))
            {
                return;
            }

            var style = ratingImageDivNode.GetAttributeValue("style", "");
            var pos = _RegPos.Match(style);
            if (!pos.Success)
            {
                return;
            }

            var x = int.Parse(pos.Groups[1].Value);
            var y = int.Parse(pos.Groups[2].Value);
            UserScore = (Score)((x + 80) / 8 - (y < -10 ? 1 : 0));
        }

        private static bool _UserRated(string classes)
            => (classes.Contains("irr") || classes.Contains("irg") || classes.Contains("irb"));

        private void _AnalyzeData(double averageScore, double displayScore, string ratingImageClass, long ratingCount)
        {
            AverageScore = averageScore;
            RatingCount = ratingCount;

            if (ratingImageClass != null && _UserRated(ratingImageClass))
            {
                UserScore = displayScore.ToScore();
            }
            else
            {
                UserScore = Score.NotSet;
            }
        }

        public IAsyncAction RatingAsync(Score rating)
        {
            return AsyncInfo.Run(async token =>
            {
                var reqInfo = new RatingRequest(Owner, rating);
                var result = await reqInfo.GetResponseAsync(token);
                _AnalyzeData(result.AverageScore, result.UserScore, result.RatingImageClass, result.RatingCount);
            });
        }
    }
}
