using Opportunity.Helpers.Universal.AsyncHelpers;
using Opportunity.MvvmUniverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ExClient.Forums
{
    public sealed class Topic : ObservableObject
    {
        public static IAsyncOperation<Topic> FetchAsync(long id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));
            return AsyncInfo.Run(async token =>
            {
                var topic = new Topic(id);
                var doctask = Client.Current.HttpClient.GetDocumentAsync(topic.Uri);
                token.Register(doctask.Cancel);
                var doc = await doctask;
                token.ThrowIfCancellationRequested();
                topic.Md5 = _Md5Regex.Match(doc.DocumentNode.InnerHtml).Groups[1].Value;
                topic.ForumId = int.Parse(_ForumIdRegex.Match(doc.DocumentNode.InnerHtml).Groups[1].Value);
                return topic;
            });
        }

        private static readonly Regex _Md5Regex = new Regex(@"var\s+ipb_md5_check\s*=\s*""([a-f0-9]+)"";", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _ForumIdRegex = new Regex(@"var\s+ipb_input_f\s*=\s*""([0-9]+)"";", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private Topic(long id)
        {
            Id = id;
        }

        public long Id { get; }

        public Uri Uri => new Uri(Client.ForumsUri, $"index.php?showtopic={Id}");

        public int ForumId { get; private set; }

        internal string Md5 { get; private set; }

        public IAsyncAction SendPost(string content, bool enableTrack, bool enableEmoji, bool enableSignature)
        {
            if (content.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(content));
            Client.Current.CheckLogOn();
            return Client.Current.HttpClient.PostAsync(Client.ForumsUri, getContent()).AsAsyncAction();

            IEnumerable<KeyValuePair<string, string>> getContent()
            {
                yield return new KeyValuePair<string, string>("act", "Post");
                yield return new KeyValuePair<string, string>("CODE", "03");
                yield return new KeyValuePair<string, string>("f", ForumId.ToString());
                yield return new KeyValuePair<string, string>("t", Id.ToString());
                yield return new KeyValuePair<string, string>("st", "0");
                yield return new KeyValuePair<string, string>("auth_key", Md5);
                yield return new KeyValuePair<string, string>("fast_reply_used", "1");
                yield return new KeyValuePair<string, string>("Post", content);
                if (enableTrack)
                    yield return new KeyValuePair<string, string>("enabletrack", "1");
                if (enableEmoji)
                    yield return new KeyValuePair<string, string>("enableemo", "yes");
                if (enableSignature)
                    yield return new KeyValuePair<string, string>("enablesig", "yes");
                yield return new KeyValuePair<string, string>("submit", "Add Reply");
            }
        }
    }
}
