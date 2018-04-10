using ExViewer.Controls;
using ExViewer.Views;
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
    public sealed class InAppToastNotification : INotificationHandler
    {
        public IAsyncOperation<bool> NotifyAsync(object data)
        {
            if (!RootControl.RootController.Available)
                return AsyncOperation<bool>.CreateCompleted(false);
            switch (data)
            {
            case Exception ex:
                RootControl.RootController.SendToast(ex, null);
                return AsyncOperation<bool>.CreateCompleted(true);
            case string str:
                RootControl.RootController.SendToast(str, null);
                return AsyncOperation<bool>.CreateCompleted(true);
            default:
                return AsyncOperation<bool>.CreateCompleted(false);
            }
        }


        void IServiceHandler<Notificator>.OnAdd(Notificator service) { }
        void IServiceHandler<Notificator>.OnRemove(Notificator service) { }
    }
}
