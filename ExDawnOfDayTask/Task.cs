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
                await HentaiVerseInfo.FetchAsync();
            }
            catch { }
            finally
            {
                HentaiVerseInfo.DawnOfDayRewardsAwarded -= this.HentaiVerseInfo_DawnOfDayRewardsAwarded;
                d.Complete();
            }
        }

        private void HentaiVerseInfo_DawnOfDayRewardsAwarded(object sender, DawnOfDayRewardsEventArgs e)
        {
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
                                Text = "It is the dawn of a new day!"
                            },
                            new AdaptiveText()
                            {
                                Text = $"You gain {getStr("EXP")} EXP, {getStr("Credits")} Credits, {getStr("GP")} GP and {getStr("Hath")} Hath!"
                            }
                        }
                    }
                }
            };
            string getStr(string key)
            {
                e.Data.TryGetValue(key, out var r);
                if (r % 1 == 0)
                    return r.ToString("N0");
                return r.ToString("N");
            }
            var toastNotif = new ToastNotification(toastContent.GetXml())
            {
                ExpirationTime = DateTimeOffset.UtcNow.Date.AddDays(1),
                Group = "DawnOfDay",
                NotificationMirroring = NotificationMirroring.Allowed,
            };
            ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
        }
    }
}
