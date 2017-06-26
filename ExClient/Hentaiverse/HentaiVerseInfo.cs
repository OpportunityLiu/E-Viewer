using HtmlAgilityPack;
using System;
using System.Linq;

namespace ExClient.HentaiVerse
{
    public static class HentaiVerseInfo
    {
        public static Uri RootUri { get; } = new Uri("http://hentaiverse.org/");

        public static Uri LogOnUri => new Uri(RootUri, $"login.php?ipb_member_id={Client.Current.UserID}&ipb_pass_hash={Client.Current.PassHash}");

        public static event EventHandler<MonsterEncounteredEventArgs> MonsterEncountered;

        public static bool IsEnabled { get; set; } = false;

        internal static void AnalyzePage(HtmlDocument doc)
        {
            var eventPane = doc.GetElementbyId("eventpane");
            if (eventPane == null)
                return;
            var div1 = eventPane.Element("div");
            if (div1 == null)
                return;
            var a = eventPane.Descendants("a").FirstOrDefault();
            if (a != null)
            {
                var uri = a.GetAttributeValue("href", "");
                var ev = MonsterEncountered;
                if (ev == null)
                    return;
                ev(Client.Current, new MonsterEncounteredEventArgs(new Uri(uri)));
            }
        }
    }
}
