using ExClient.Api;
using ExClient.Galleries;

using HtmlAgilityPack;

using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Collections;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Windows.Web.Http;

namespace ExClient.Services
{
    public static class ExpungeExtensions
    {
        public static Task<ExpungeInfo> FetchExpungeInfoAsync(this GalleryInfo galleryInfo, CancellationToken token = default)
            => ExpungeInfo.FetchAsync(galleryInfo, token);
        public static Task<ExpungeInfo> FetchExpungeInfoAsync(this Gallery gallery, CancellationToken token = default)
            => ExpungeInfo.FetchAsync(gallery, token);
    }

    public sealed class ExpungeInfo : ObservableObject
    {
        public static async Task<ExpungeInfo> FetchAsync(GalleryInfo galleryInfo, CancellationToken token = default)
        {
            var r = new ExpungeInfo(galleryInfo);
            await r.RefreshAsync(token);
            return r;
        }

        public ExpungeInfo(GalleryInfo galleryInfo) => GalleryInfo = galleryInfo;

        public GalleryInfo GalleryInfo { get; }

        private Uri apiUri => new Uri($"gallerypopups.php?gid={GalleryInfo.ID}&t={GalleryInfo.Token.ToString()}&act=expunge", UriKind.Relative);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ObservableList<ExpungeRecord> _Records = new ObservableList<ExpungeRecord>();
        public ObservableListView<ExpungeRecord> Records => _Records.AsReadOnly();

        private static readonly Regex _InfoRegex = new Regex($@"^\s*\+(?<{nameof(ExpungeRecord.Power)}>\d+)\s*(?<{nameof(ExpungeRecord.Reason)}>\w+)\s*on\s*(?<{nameof(ExpungeRecord.Posted)}>.+?)\s*UTC\s*by\s*(?<{nameof(ExpungeRecord.Author)}>.+?)\s*$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        public async Task RefreshAsync(CancellationToken token = default)
        {
            var post = Client.Current.HttpClient.PostStringAsync(apiUri, new HttpFormUrlEncodedContent(new[]{
                new KeyValuePair<string, string>("log", "Show Expunge Log")
            }));
            token.Register(post.Cancel);
            var response = await post;

            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(response);
                var form = doc.GetElementbyId("galpop");
                if (form is null)
                {
                    throw new InvalidOperationException("Form id=`galpop` not found.");
                }
                var votes = form.SelectNodes("descendant::div[@class='c1']");
                if (votes is null)
                {
                    _Records.Clear();
                    return;
                }
                var data = new List<ExpungeRecord>();
                foreach (var item in votes)
                {
                    var match = _InfoRegex.Match(item.FirstChild.GetInnerText());
                    if (!match.Success)
                        throw new InvalidOperationException("infoRegex matches failed.");
                    data.Add(new ExpungeRecord(
                        (ExpungeReason)Enum.Parse(typeof(ExpungeReason), match.Groups[nameof(ExpungeRecord.Reason)].Value, true),
                        item.LastChild.GetInnerText(),
                        match.Groups[nameof(ExpungeRecord.Author)].Value,
                        DateTimeOffset.Parse(match.Groups[nameof(ExpungeRecord.Posted)].Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces),
                        int.Parse(match.Groups[nameof(ExpungeRecord.Power)].Value)
                        ));
                }
                _Records.Update(data);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(LocalizedStrings.Resources.WrongApiResponse, ex)
                    .AddData("RequestUri", apiUri.ToString())
                    .AddData("Response", response);
            }
        }

        public async Task VoteAsync(ExpungeReason reason, string explanation, CancellationToken token = default)
        {
            if (reason == ExpungeReason.None)
                explanation = null;

            var post = Client.Current.HttpClient.PostStringAsync(apiUri, new HttpFormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("expungecat", ((int)reason + 1).ToString()),
                new KeyValuePair<string, string>("expungexpl", explanation.IsNullOrWhiteSpace() ? " " : explanation.Trim()),
                new KeyValuePair<string, string>("pet", "Petition to Expunge"),
            }));
            token.Register(post.Cancel);
            var res = await post;
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(res);
                var form = doc.GetElementbyId("galpop");
                if (form is null)
                    throw new InvalidOperationException(LocalizedStrings.Resources.WrongApiResponse);
                var text = form.GetInnerText();
                if (!text.Contains("The requested action has been performed."))
                    throw new InvalidOperationException(form.GetInnerText());
            }
            catch (Exception ex)
            {
                ex.AddData("RequestUri", apiUri.ToString());
                ex.AddData("Response", res);
                throw;
            }
            await RefreshAsync(token);
        }
    }
}
