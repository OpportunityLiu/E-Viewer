using ExViewer.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

namespace ExViewer.Helpers
{
    internal static class JumplistManager
    {
        private sealed class HistoryRecordComparer : IEqualityComparer<HistoryRecord>
        {
            public bool Equals(HistoryRecord x, HistoryRecord y)
            {
                if (ReferenceEquals(x, y))
                    return true;
                if (x is null || y is null)
                    return false;
                return x.Title == y.Title && x.Type == y.Type;
            }

            public int GetHashCode(HistoryRecord obj)
            {
                if (obj is null)
                    return 0;
                return obj.Title.GetHashCode() ^ obj.Type.GetHashCode();
            }

            public static HistoryRecordComparer Instance { get; } = new HistoryRecordComparer();
        }

        public static async Task RefreshJumplistAsync()
        {
            if (!JumpList.IsSupported())
                return;
            var jl = await JumpList.LoadCurrentAsync();
            foreach (var item in jl.Items)
            {
                if (item.RemovedByUser)
                    HistoryDb.Remove(new Uri(item.Arguments));
            }
            jl.Items.Clear();
            using (var db = new HistoryDb())
            {
                var records = db.HistorySet.OrderByDescending(r => r.TimeStamp).Take(40).ToArray();
                var added = new HashSet<HistoryRecord>(HistoryRecordComparer.Instance);
                var dtformatter = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("shortdate shorttime");
                const string sep = " - ";
                foreach (var record in records)
                {
                    if (!added.Add(record))
                        continue;
                    var title = default(string);
                    try
                    {
                        title = string.Format(Strings.Resources.JumpList.Recent.HistoryRecord.GetValue(record.Type.ToString()), record.Title.Trim());
                    }
                    catch (Exception ex)
                    {
                        Telemetry.LogException(ex);
                        title = $"{record.Title.Trim()} - E-Viewer";
                    }
                    var item = JumpListItem.CreateWithArguments(record.Uri.ToString(), title);
                    item.GroupName = "ms-resource:///Resources/JumpList/Recent/GroupName";
                    item.Logo = new Uri($"ms-appx:///Assets/JumpList/{record.Type.ToString()}.png");
                    item.Description = item.DisplayName + sep + dtformatter.Format(record.Time) + sep + item.Arguments;
                    jl.Items.Add(item);
                    if (jl.Items.Count >= 20)
                        break;
                }
            }
            await jl.SaveAsync();
        }
    }
}
