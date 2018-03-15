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
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace ExViewer.Services
{
    public class ContentDialogNotificationData
    {
        public object Title { get; set; } = "";

        public object Content { get; set; } = "";

        public string PrimaryButtonText { get; set; } = "";
        public ICommand PrimaryButtonCommand { get; set; }

        public string SecondaryButtonText { get; set; } = "";
        public ICommand SecondaryButtonCommand { get; set; }

        public string CloseButtonText { get; set; } = "";

        public ContentDialogButton DefaultButton { get; set; } = ContentDialogButton.None;
    }

    public sealed class ContentDialogNotification : MyContentDialog, INotificationHandler
    {
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
            this.PrimaryButtonCommand = null;
            this.SecondaryButtonText = "";
            this.SecondaryButtonCommand = null;
            this.CloseButtonText = "";
            this.DefaultButton = ContentDialogButton.None;
        }

        private bool isopen = false;

        public IAsyncOperation<bool> NotifyAsync(object data)
        {
            if (this.isopen || !(data is ContentDialogNotificationData notificationData))
                return AsyncOperation<bool>.CreateCompleted(false);

            this.Title = notificationData.Title;
            this.Content = notificationData.Content;
            this.PrimaryButtonText = notificationData.PrimaryButtonText;
            this.PrimaryButtonCommand = notificationData.PrimaryButtonCommand;
            this.SecondaryButtonText = notificationData.SecondaryButtonText;
            this.SecondaryButtonCommand = notificationData.SecondaryButtonCommand;
            this.CloseButtonText = notificationData.CloseButtonText;
            this.DefaultButton = notificationData.DefaultButton;

            return ShowAsync().ContinueWith(a => true);
        }


        void IServiceHandler<Notificator>.OnAdd(Notificator service) { }
        void IServiceHandler<Notificator>.OnRemove(Notificator service) { }
    }
}
