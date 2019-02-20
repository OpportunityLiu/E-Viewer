using ExClient.Api;
using ExClient.Galleries;
using ExClient.Internal;
using HtmlAgilityPack;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ExClient.Services
{
    public static class RenameExtensions
    {
        public static IAsyncOperation<RenameInfo> FetchRenameInfoAsync(this GalleryInfo galleryInfo)
            => RenameInfo.FetchAsync(galleryInfo);
        public static IAsyncOperation<RenameInfo> FetchRenameInfoAsync(this Gallery gallery)
            => RenameInfo.FetchAsync(gallery);
    }

    public sealed class RenameInfo : ObservableObject
    {
        public static IAsyncOperation<RenameInfo> FetchAsync(GalleryInfo galleryInfo)
        {
            return AsyncInfo.Run(async token =>
            {
                var r = new RenameInfo(galleryInfo);
                var u = r.RefreshAsync();
                token.Register(u.Cancel);
                await u;
                token.ThrowIfCancellationRequested();
                return r;
            });
        }

        private RenameInfo(GalleryInfo galleryInfo) => GalleryInfo = galleryInfo;

        public GalleryInfo GalleryInfo { get; }

        public ObservableListView<RenameRecord> RomanRecords => rmnRecords.AsReadOnly();
        public ObservableListView<RenameRecord> JapaneseRecords => jpnRecords.AsReadOnly();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ObservableList<RenameRecord> rmnRecords = new ObservableList<RenameRecord>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ObservableList<RenameRecord> jpnRecords = new ObservableList<RenameRecord>();

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string originalRmn;
        public string OriginalRomanTitle
        {
            get => originalRmn;
            private set => Set(ref originalRmn, value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string originalJpn;
        public string OriginalJapaneseTitle
        {
            get => originalJpn;
            private set => Set(ref originalJpn, value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private RenameRecord votedRmn;
        public RenameRecord VotedRoman
        {
            get => votedRmn;
            private set => Set(ref votedRmn, value);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private RenameRecord votedJpn;
        public RenameRecord VotedJapanese
        {
            get => votedJpn;
            private set => Set(ref votedJpn, value);
        }

        private Uri apiUri => new Uri($"gallerypopups.php?gid={GalleryInfo.ID}&t={GalleryInfo.Token.ToTokenString()}&act=rename", UriKind.Relative);

        public IAsyncAction RefreshAsync()
        {
            return Task.Run(async () =>
            {
                var doc = await Client.Current.HttpClient.GetDocumentAsync(apiUri);
                analyze(doc);
            }).AsAsyncAction();
        }

        private void analyze(HtmlDocument doc)
        {
            if (doc.DocumentNode.ChildNodes.Count == 1 && doc.DocumentNode.FirstChild.NodeType == HtmlNodeType.Text)
                throw new InvalidOperationException(doc.DocumentNode.FirstChild.GetInnerText())
                    .AddData("Uri", apiUri.ToString())
                    .AddData("Document", doc.Text);
            var tables = doc.DocumentNode.Descendants("table").ToList();
            IReadOnlyList<RenameRecord> romanRec, japaneseRec;
            RenameRecord romanV, japaneseV;
            (romanV, romanRec, OriginalRomanTitle) = analyzeTable(tables[0]);
            (japaneseV, japaneseRec, OriginalJapaneseTitle) = analyzeTable(tables[1]);
            rmnRecords.Update(romanRec, default(IEqualityComparer<RenameRecord>), (o, n) => o.Power = n.Power);
            jpnRecords.Update(japaneseRec, default(IEqualityComparer<RenameRecord>), (o, n) => o.Power = n.Power);
            VotedRoman = romanV;
            VotedJapanese = japaneseV;

            var errorNode = doc.DocumentNode.SelectSingleNode("//p[@class='br']");
            if (errorNode != null)
            {
                var error = errorNode.GetInnerText();
                switch (error)
                {
                case "New suggested title is too short.":
                case "New suggested title is too short.New suggested title is too short.":
                    error = LocalizedStrings.Resources.RenameTitleTooShort;
                    break;
                //localize;
                default:
                    break;
                }
                throw new InvalidOperationException(error)
                    .AddData("Uri", apiUri.ToString())
                    .AddData("Document", doc.Text);
            }

            (RenameRecord current, IReadOnlyList<RenameRecord> records, string original) analyzeTable(HtmlNode tableNode)
            {
                var original = default(string);
                var text = tableNode.Element("tr").LastChild.FirstChild;
                if (text.NodeType == HtmlNodeType.Text)
                {
                    original = text.GetInnerText();
                }

                var trecords = tableNode.Elements("tr").Skip(1).ToList();
                var records = new List<RenameRecord>();
                var current = default(RenameRecord);
                foreach (var rec in trecords)
                {
                    var input = rec.Descendants("input").First();
                    // 0 for new; -1 for blank vote
                    var recId = input.GetAttribute("value", -1);
                    if (recId > 0)
                    {
                        var powStr = rec.ChildNodes[1].GetInnerText();
                        var power = int.Parse(powStr.Substring(0, powStr.Length - 1));
                        var title = rec.ChildNodes[2].GetInnerText();
                        var record = new RenameRecord(recId, title, power);
                        records.Add(record);
                        if (input.GetAttribute("checked", false))
                            current = record;
                    }

                }
                return (current, records, original);
            }
        }

        public IAsyncAction VoteAsync(RenameRecord roman, RenameRecord japanese)
        {
            return AsyncInfo.Run(async token =>
            {
                var post = Client.Current.HttpClient.PostAsync(apiUri,
                    new KeyValuePair<string, string>("new_r", roman.Title),
                    new KeyValuePair<string, string>("new_j", japanese.Title),
                    new KeyValuePair<string, string>("nid_r", roman.ID.ToString()),
                    new KeyValuePair<string, string>("nid_j", japanese.ID.ToString()),
                    new KeyValuePair<string, string>("apply", "Submit")
                );
                token.Register(post.Cancel);
                var res = await post;
                using (var stm = (await res.Content.ReadAsInputStreamAsync()).AsStreamForRead())
                {
                    var doc = new HtmlDocument();
                    doc.Load(stm);
                    analyze(doc);
                }
            });
        }

        public IAsyncAction VoteAsync(string roman, RenameRecord japanese) => VoteAsync(generateTempRenameRecord(roman), japanese);

        public IAsyncAction VoteAsync(RenameRecord roman, string japanese) => VoteAsync(roman, generateTempRenameRecord(japanese));

        public IAsyncAction VoteAsync(string roman, string japanese) => VoteAsync(generateTempRenameRecord(roman), generateTempRenameRecord(japanese));

        private static RenameRecord generateTempRenameRecord(string title)
        {
            if (title.IsNullOrWhiteSpace())
                return new RenameRecord(-1, "", -1);
            return new RenameRecord(0, title.Trim(), 0);
        }
    }
}
