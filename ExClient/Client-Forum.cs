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
using Windows.Storage;
using System.Runtime.InteropServices;
using System.Globalization;
using Newtonsoft.Json;

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
                    Avatar = (avatar == null) ? null : new Uri(forumUri, avatar.Attributes["src"].Value),
                    MemberGroup = groupAndJoin.FirstChild.InnerText.Trim().Substring(14).DeEntitize(),
                    RegisterDate = register.Date
                };
                return r;
            }).AsAsyncOperation();
        }
    }

    public class UserInfo
    {
        public IAsyncAction SaveToCache()
        {
            return Run(async token =>
            {
                var str = JsonConvert.SerializeObject(this);
                var file = await StorageHelper.LocalState.CreateFileAsync("UserInfo", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, str);
                var avatarFile = await StorageHelper.LocalState.CreateFileAsync("UserAvatar", CreationCollisionOption.ReplaceExisting);
                var buffur = await Client.Current.HttpClient.GetBufferAsync(Avatar);
                await FileIO.WriteBufferAsync(avatarFile, buffur);
            });
        }

        public static IAsyncOperation<UserInfo> LoadFromCache()
        {
            return Run(async token =>
            {
                var file = await StorageHelper.LocalState.TryGetFileAsync("UserInfo");
                if(file == null)
                    return null;
                var str = await FileIO.ReadTextAsync(file);
                var obj = JsonConvert.DeserializeObject<UserInfo>(str);
                obj.Avatar = new Uri("ms-appdata:///local/UserAvatar");
                return obj;
            });
        }

        internal UserInfo()
        {
        }

        [JsonProperty]
        public string DisplayName
        {
            get;
            internal set;
        }

        [JsonProperty]
        public int UserID
        {
            get;
            internal set;
        }

        [JsonProperty]
        public string MemberGroup
        {
            get;
            internal set;
        }

        [JsonProperty]
        public DateTime RegisterDate
        {
            get;
            internal set;
        }

        [JsonProperty]
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