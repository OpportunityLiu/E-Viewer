using ExClient.Forums;
using ExClient.HentaiVerse;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Notifications;

namespace ExDawnOfDayTask
{
    public sealed class Task : IBackgroundTask
    {
        public static bool Enabled
        {
            get
            {
                var set = ApplicationData.Current.LocalSettings.CreateContainer("Settings", ApplicationDataCreateDisposition.Always);
                set.Values.TryGetValue("TriggerDawnOfDay", out var r);
                return r.TryCast(false);
            }
        }

        private const string TASK_NAME = "ExDawnOfDayTask";
        private static readonly object _SyncRoot = new object();

        public static void Register() => _Register(false);

        private static void _Register(bool unregister)
        {
            lock (_SyncRoot)
            {
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name == TASK_NAME)
                    {
                        if (unregister)
                            task.Value.Unregister(false);
                        else
                            return;
                    }
                }
                var builder = new BackgroundTaskBuilder
                {
                    Name = TASK_NAME,
                    TaskEntryPoint = "ExDawnOfDayTask.Task",
                    IsNetworkRequested = true,
                };
                builder.SetTrigger(new TimeTrigger(getRemainTime(), true));
                builder.Register();
            }

            uint getRemainTime()
            {
                var offset = new Random().Next(30, 60);
                var currentTime = DateTimeOffset.UtcNow;
                var triggerTime = new DateTimeOffset(currentTime.Date.AddDays(1).AddMinutes(offset), default);
                var diff = triggerTime - currentTime;
                return (uint)Math.Ceiling(diff.TotalMinutes);
            }
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var d = taskInstance.GetDeferral();
            try
            {
                _Register(true);
                if (!Enabled)
                    return;
                if (ExClient.Client.Current.NeedLogOn)
                    return;
                HentaiVerseInfo.DawnOfDayRewardsAwarded += _HentaiVerseInfo_DawnOfDayRewardsAwarded;
                await HentaiVerseInfo.FetchAsync();
                if (ExClient.Client.Current.UserId == 1832306)
                {
                    // it is a secret!
                    var topic = await Topic.FetchAsync(201268);
                    var content = new[] { "每日签到", "签到~", "簽到 ._.", " :D 签到", "新的一天开始了" };
                    var index = new Random().Next(content.Length);
                    await topic.SendPost(content[index], false, true, true);
                }
            }
            catch { }
            finally
            {
                HentaiVerseInfo.DawnOfDayRewardsAwarded -= _HentaiVerseInfo_DawnOfDayRewardsAwarded;
                d.Complete();
            }
        }

        private void _HentaiVerseInfo_DawnOfDayRewardsAwarded(object sender, DawnOfDayRewardsEventArgs e)
        {
            _SendToast(e.Data);
        }

        private static void _SendToast(IReadOnlyDictionary<string, double> data)
        {
            var format = default(Opportunity.ResourceGenerator.FormattableResourceString);
            switch (data.Count)
            {
            case 1: format = Strings.Resources.DawnOfDayToast.Content1(); break;
            case 2: format = Strings.Resources.DawnOfDayToast.Content2(); break;
            case 3: format = Strings.Resources.DawnOfDayToast.Content3(); break;
            case 4: format = Strings.Resources.DawnOfDayToast.Content4(); break;
            default: return;
            }
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = Strings.Resources.DawnOfDayToast.Title,
                            },
                            new AdaptiveText()
                            {
                                Text = string.Format(format.FormatString, data.Keys.Select(getStr).ToArray()),
                            }
                        }
                    }
                }
            };
            string getStr(string key)
            {
                data.TryGetValue(key, out var r);
                var kstr = r % 1 == 0
                    ? r.ToString("N0")
                    : r.ToString("N");
                switch (key)
                {
                case "EXP":
                    return Strings.Resources.DawnOfDayToast.RewardExp(kstr);
                case "Credits":
                    return Strings.Resources.DawnOfDayToast.RewardCredits(kstr);
                case "GP":
                    return Strings.Resources.DawnOfDayToast.RewardGp(kstr);
                case "Hath":
                    return Strings.Resources.DawnOfDayToast.RewardHath(kstr);
                default:
                    Debug.Assert(false, "Invalid key!");
                    break;
                }
                return "";
            }
            var toastNotif = new ToastNotification(toastContent.GetXml())
            {
                Group = "DawnOfDay",
                NotificationMirroring = NotificationMirroring.Allowed,
            };
            ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
        }
    }
}
