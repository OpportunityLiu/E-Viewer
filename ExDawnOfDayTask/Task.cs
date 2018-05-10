using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using ExClient.HentaiVerse;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;
using System.Threading;
using System.Diagnostics;

namespace ExDawnOfDayTask
{
    public sealed class Task : IBackgroundTask
    {
        private const string TASK_NAME = "ExDawnOfDayTask";
        private static int registered = 0;
        public static void Register()
        {
            if (Interlocked.Exchange(ref registered, 1) != 0)
                return;
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == TASK_NAME)
                    return;
            }
            var builder = new BackgroundTaskBuilder
            {
                Name = TASK_NAME,
                TaskEntryPoint = "ExDawnOfDayTask.Task",
                IsNetworkRequested = true,
            };
            builder.SetTrigger(new TimeTrigger(24 * 60, false));
            builder.Register();
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            if (ExClient.Client.Current.NeedLogOn)
                return;
            var d = taskInstance.GetDeferral();
            try
            {
                HentaiVerseInfo.DawnOfDayRewardsAwarded += this.HentaiVerseInfo_DawnOfDayRewardsAwarded;
#if DEBUG
                if (Debugger.IsAttached)
                    HentaiVerseInfo_DawnOfDayRewardsAwarded(null, null);
#endif
                await HentaiVerseInfo.FetchAsync();
            }
            catch { }
            finally
            {
                HentaiVerseInfo.DawnOfDayRewardsAwarded -= this.HentaiVerseInfo_DawnOfDayRewardsAwarded;
                d.Complete();
            }
        }

        private static void sendToast(IReadOnlyDictionary<string, double> data)
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

        private void HentaiVerseInfo_DawnOfDayRewardsAwarded(object sender, DawnOfDayRewardsEventArgs e)
        {
#if DEBUG
            if (e is null)
            {
                sendToast(new Dictionary<string, double>
                {
                    ["Hath"] = 12.1,
                    ["EXP"] = 34567,
                });
                return;
            }
#endif
            sendToast(e.Data);
        }
    }
}
