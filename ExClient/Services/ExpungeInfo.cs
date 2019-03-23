using ExClient.Api;
using ExClient.Galleries;
using ExClient.Internal;
using HtmlAgilityPack;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ExClient.Services
{
    public static class ExpungeExtensions
    {
        public static IAsyncOperation<ExpungeInfo> FetchExpungeInfoAsync(this GalleryInfo galleryInfo)
            => ExpungeInfo.FetchAsync(galleryInfo);
        public static IAsyncOperation<ExpungeInfo> FetchExpungeInfoAsync(this Gallery gallery)
            => ExpungeInfo.FetchAsync(gallery);
    }

    public sealed class ExpungeInfo : ObservableObject
    {
        public static IAsyncOperation<ExpungeInfo> FetchAsync(GalleryInfo galleryInfo)
        {
            return AsyncInfo.Run(async token =>
            {
                var r = new ExpungeInfo(galleryInfo);
                var u = r.RefreshAsync();
                token.Register(u.Cancel);
                await u;
                token.ThrowIfCancellationRequested();
                return r;
            });
        }

        public ExpungeInfo(GalleryInfo galleryInfo) => GalleryInfo = galleryInfo;

        public GalleryInfo GalleryInfo { get; }

        private Uri apiUri => new Uri($"gallerypopups.php?gid={GalleryInfo.ID}&t={GalleryInfo.Token.ToString()}&act=expunge", UriKind.Relative);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ObservableList<ExpungeRecord> records = new ObservableList<ExpungeRecord>();
        public ObservableListView<ExpungeRecord> Records => records.AsReadOnly();

        private static readonly Regex infoRegex = new Regex($@"^\s*\+(?<{nameof(ExpungeRecord.Power)}>\d+)\s*(?<{nameof(ExpungeRecord.Reason)}>\w+)\s*on\s*(?<{nameof(ExpungeRecord.Posted)}>.+?)\s*UTC\s*by\s*(?<{nameof(ExpungeRecord.Author)}>.+?)\s*$", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        public IAsyncAction RefreshAsync()
        {
            return Task.Run(async () =>
            {
                var response = await Client.Current.HttpClient.PostAsync(apiUri, new KeyValuePair<string, string>("log", "Show Expunge Log"));
                var responseStr = await response.Content.ReadAsStringAsync();
                try
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(responseStr);
                    var form = doc.GetElementbyId("galpop");
                    if (form is null)
                    {
                        throw new InvalidOperationException("Form id=`galpop` not found.");
                    }
                    var votes = form.SelectNodes("descendant::div[@class='c1']");
                    if (votes is null)
                    {
                        records.Clear();
                        return;
                    }
                    var data = new List<ExpungeRecord>();
                    foreach (var item in votes)
                    {
                        var match = infoRegex.Match(item.FirstChild.GetInnerText());
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
                    records.Update(data);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(LocalizedStrings.Resources.WrongApiResponse, ex)
                        .AddData("RequestUri", response.RequestMessage.RequestUri.ToString())
                        .AddData("Response", responseStr);
                }
            }).AsAsyncAction();
        }

        public IAsyncAction VoteAsync(ExpungeReason reason, string explanation)
        {
            if (reason == ExpungeReason.None)
                explanation = null;
            return AsyncInfo.Run(async token =>
            {
                var post = Client.Current.HttpClient.PostAsync(apiUri,
                    new KeyValuePair<string, string>("expungecat", ((int)reason + 1).ToString()),
                    new KeyValuePair<string, string>("expungexpl", explanation.IsNullOrWhiteSpace() ? " " : explanation.Trim()),
                    new KeyValuePair<string, string>("pet", "Petition to Expunge")
                );
                token.Register(post.Cancel);
                var res = await post;
                var resStr = await res.Content.ReadAsStringAsync();
                using (var stm = (await res.Content.ReadAsInputStreamAsync()).AsStreamForRead())
                {
                    try
                    {
                        var doc = new HtmlDocument();
                        doc.Load(stm);
                        var form = doc.GetElementbyId("galpop");
                        if (form is null)
                            throw new InvalidOperationException(LocalizedStrings.Resources.WrongApiResponse);
                        var text = form.GetInnerText();
                        if (!text.Contains("The requested action has been performed."))
                            throw new InvalidOperationException(form.GetInnerText());
                    }
                    catch (Exception ex)
                    {
                        ex.AddData("RequestUri", res.RequestMessage.RequestUri.ToString());
                        ex.AddData("Response", resStr);
                        throw;
                    }
                }
                await RefreshAsync();
            });
        }
    }
}
