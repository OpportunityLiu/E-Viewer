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
        public object PrimaryButtonCommandParameter { get; set; }

        public string SecondaryButtonText { get; set; } = "";
        public ICommand SecondaryButtonCommand { get; set; }
        public object SecondaryButtonCommandParameter { get; set; }

        public string CloseButtonText { get; set; } = "";

        public ContentDialogButton DefaultButton { get; set; } = ContentDialogButton.None;
    }

    public sealed class ContentDialogNotification : MyContentDialog, INotificationHandler
    {
        public ContentDialogNotification()
        {
            Opened += ContentDialogNotification_Opened;
            Closing += ContentDialogNotification_Closing;
            Closed += ContentDialogNotification_Closed;
        }

        private void ContentDialogNotification_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            isopen = true;
        }

        private void ContentDialogNotification_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
        }

        private void ContentDialogNotification_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            isopen = false;
            Title = null;
            Content = null;
            PrimaryButtonText = "";
            PrimaryButtonCommand = null;
            PrimaryButtonCommandParameter = null;
            SecondaryButtonText = "";
            SecondaryButtonCommand = null;
            SecondaryButtonCommandParameter = null;
            CloseButtonText = "";
            DefaultButton = ContentDialogButton.None;
        }

        private bool isopen = false;

        public IAsyncOperation<bool> NotifyAsync(object data)
        {
            if (this.isopen || !(data is ContentDialogNotificationData notificationData))
            {
                return AsyncOperation<bool>.CreateCompleted(false);
            }

            Title = notificationData.Title;
            Content = notificationData.Content;
            PrimaryButtonText = notificationData.PrimaryButtonText;
            PrimaryButtonCommand = notificationData.PrimaryButtonCommand;
            PrimaryButtonCommandParameter = notificationData.PrimaryButtonCommandParameter;
            SecondaryButtonText = notificationData.SecondaryButtonText;
            SecondaryButtonCommand = notificationData.SecondaryButtonCommand;
            SecondaryButtonCommandParameter = notificationData.SecondaryButtonCommandParameter;
            CloseButtonText = notificationData.CloseButtonText;
            DefaultButton = notificationData.DefaultButton;

            return ShowAsync().ContinueWith(a => true);
        }


        void IServiceHandler<Notificator>.OnAdd(Notificator service) { }
        void IServiceHandler<Notificator>.OnRemove(Notificator service) { }
    }
}
