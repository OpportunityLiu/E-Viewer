using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Windows.Foundation.Metadata;

namespace ExClient
{
    public static class DispatcherHelper
    {
        private static CoreDispatcher dispatcher;

        public static void SetDispatcher(CoreDispatcher dispatcher)
        {
            if(dispatcher == null)
                return;
            DispatcherHelper.dispatcher = dispatcher;
        }

        public static IAsyncAction RunIdleAsync(DispatchedHandler action)
        {
            return RunAsync(action, CoreDispatcherPriority.Idle);
        }

        public static IAsyncAction RunLowAsync(DispatchedHandler action)
        {
            return RunAsync(action, CoreDispatcherPriority.Low);
        }

        public static IAsyncAction RunNormalAsync(DispatchedHandler action)
        {
            return RunAsync(action, CoreDispatcherPriority.Normal);
        }

        [Deprecated("Do not use this method, preserved for system event.", DeprecationType.Remove, 0)]
        public static IAsyncAction RunHighAsync(DispatchedHandler action)
        {
            return RunAsync(action, CoreDispatcherPriority.High);
        }

        public static IAsyncAction RunAsync(DispatchedHandler action, CoreDispatcherPriority priority)
        {
            if(dispatcher == null)
                return Task.Run(() => action()).AsAsyncAction();
            else
                return dispatcher.RunAsync(priority, action);
        }
    }
}
