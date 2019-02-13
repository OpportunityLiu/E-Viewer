using ExViewer.Controls;
using Opportunity.MvvmUniverse.Services.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials.UI;
using Windows.UI.Xaml;

namespace ExViewer.Helpers
{
    internal static class VerificationManager
    {
        public static async Task VerifyAsync()
        {
            string info = null;
            var succeed = false;
            var result = await UserConsentVerifier.RequestVerificationAsync(Strings.Resources.Verify.Dialog.Content);
            switch (result)
            {
            case UserConsentVerificationResult.Verified:
                succeed = true;
                break;
            case UserConsentVerificationResult.DeviceNotPresent:
                info = Strings.Resources.Verify.DeviceNotPresent;
                break;
            case UserConsentVerificationResult.NotConfiguredForUser:
                info = Strings.Resources.Verify.NotConfigured;
                break;
            case UserConsentVerificationResult.DisabledByPolicy:
                info = Strings.Resources.Verify.Disabled;
                break;
            case UserConsentVerificationResult.DeviceBusy:
                info = Strings.Resources.Verify.DeviceBusy;
                break;
            case UserConsentVerificationResult.RetriesExhausted:
                info = Strings.Resources.Verify.RetriesExhausted;
                break;
            case UserConsentVerificationResult.Canceled:
                info = Strings.Resources.Verify.Canceled;
                break;
            default:
                info = Strings.Resources.Verify.OtherFailure;
                break;
            }
            if (!succeed)
            {
                if (info != null)
                {
                    await Notificator.GetForCurrentView().NotifyAsync(new Services.ContentDialogNotificationData
                    {
                        Title = Strings.Resources.Verify.FailedDialogTitle,
                        Content = info,
                        CloseButtonText = Strings.Resources.General.Exit,
                    });
                }
                Application.Current.Exit();
            }
        }
    }
}
