using ExViewer.Controls;
using Opportunity.Helpers.Universal.AsyncHelpers;
using Opportunity.MvvmUniverse.Services;
using Opportunity.MvvmUniverse.Services.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace ExViewer.Services
{
    public class ContentDialogNotificationData
    {
        public object Title { get; set; } = "";

        public object Content { get; set; } = "";

        public string CloseButtonText { get; set; } = "";
    }

    public class ContentDialogQuestionData : ContentDialogNotificationData
    {
        public string PrimaryButtonText { get; set; } = "";
        public string SecondaryButtonText { get; set; } = "";

        public ContentDialogButton DefaultButton { get; set; } = ContentDialogButton.None;
    }

    public sealed class ContentDialogNotification : MyContentDialog, INotificationHandler
    {
        public static readonly string Notification = "Notification";
        public static readonly string Question = "Question";

        public ContentDialogNotification()
        {
            this.Opened += this.ContentDialogNotification_Opened;
            this.Closing += this.ContentDialogNotification_Closing;
            this.Closed += this.ContentDialogNotification_Closed;
        }

        private void ContentDialogNotification_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            this.isopen = true;
        }

        private void ContentDialogNotification_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
        }

        private void ContentDialogNotification_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            this.isopen = false;
            this.Title = null;
            this.Content = null;
            this.PrimaryButtonText = "";
            this.SecondaryButtonText = "";
            this.CloseButtonText = "";
            this.DefaultButton = ContentDialogButton.None;
        }

        private bool isopen = false;

        public bool Notify(string category, object data)
        {
            if (category != Notification)
                return false;
            if (this.isopen)
                return false;
            if (!(data is ContentDialogNotificationData notificationData))
                return false;
            this.Title = notificationData.Title;
            this.Content = notificationData.Content;
            this.CloseButtonText = notificationData.CloseButtonText;
            var ignore = this.ShowAsync();
            return true;
        }

        public IAsyncOperation<NotificationResult> NotifyAsync(string category, object data)
        {
            if (category != Question || this.isopen || !(data is ContentDialogQuestionData notificationData))
                return AsyncOperation<NotificationResult>.CreateCompleted();

            this.Title = notificationData.Title;
            this.Content = notificationData.Content;
            this.PrimaryButtonText = notificationData.PrimaryButtonText;
            this.SecondaryButtonText = notificationData.SecondaryButtonText;
            this.CloseButtonText = notificationData.CloseButtonText;
            this.DefaultButton = notificationData.DefaultButton;

            return AsyncInfo.Run(async token =>
            {
                var t = ShowAsync();
                token.Register(t.Cancel);
                if ((await t) == ContentDialogResult.Primary)
                    return NotificationResult.Positive;
                else
                    return NotificationResult.Negetive;
            });
        }


        void IServiceHandler<Notificator>.OnAdd(Notificator service) { }
        void IServiceHandler<Notificator>.OnRemove(Notificator service) { }
    }
}
