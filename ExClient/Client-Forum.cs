using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using HtmlAgilityPack;
using System.IO;
using System.Runtime.InteropServices;
using System.Globalization;

namespace ExClient
{
    public partial class Client
    {
        private static readonly Uri forumUri = new Uri("https://forums.e-hentai.org/");

        public IAsyncOperation<UserInfo> LoadUserInfo(int userID)
        {
            return Task.Run(async () =>
            {
                var userInfoPage = await HttpClient.GetStringAsync(new Uri(forumUri, $"index.php?showuser={userID}"));
                var document = new HtmlDocument();
                document.LoadHtml(userInfoPage);
                var profileName = document.GetElementbyId("profilename");
                if(profileName == null)
                    return null;
                var profileRoot = profileName.ParentNode;
                var profiles = profileRoot.ChildNodes.Where(n => n.Name == "div").ToList();
                var info = profiles[2];
                var avatar = profiles[1].Descendants("img").FirstOrDefault();
                var groupAndJoin = profiles[4];
                DateTimeOffset register;
                if(!DateTimeOffset.TryParseExact(groupAndJoin.LastChild.InnerText,
                    "'Joined:' d-MMMM yy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces,
                    out register))
                {
                    if(groupAndJoin.LastChild.InnerText.Contains("Today"))
                        register = DateTimeOffset.UtcNow;
                    else if(groupAndJoin.LastChild.InnerText.Contains("Yesterday"))
                        register = DateTimeOffset.UtcNow.AddDays(-1);
                }
                var r = new UserInfo
                {
                    DisplayName = profileName.InnerText.DeEntitize(),
                    UserID = userID,
                    Infomation = (info.ChildNodes.Count == 1 && info.ChildNodes[0].Name == "i" && info.InnerText == "No Information") ? null : info.InnerText.DeEntitize(),
                    Avatar = (avatar == null) ? null : new Uri(avatar.Attributes["src"].Value),
                    MemberGroup = groupAndJoin.FirstChild.InnerText.Trim().Substring(14).DeEntitize(),
                    RegisterDate = register.Date
                };
                return r;
            }).AsAsyncOperation();
        }
    }

    public class UserInfo
    {
        internal UserInfo()
        {
        }

        public string DisplayName
        {
            get;
            internal set;
        }

        public int UserID
        {
            get;
            internal set;
        }

        public string MemberGroup
        {
            get;
            internal set;
        }

        public DateTime RegisterDate
        {
            get;
            internal set;
        }

        public string Infomation
        {
            get;
            internal set;
        }

        public Uri Avatar
        {
            get;
            internal set;
        }
    }
}