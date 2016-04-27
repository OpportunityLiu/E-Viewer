using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient
{
    public static class DispatcherHelper
    {
        public static CoreDispatcher Dispatcher
        {
            get; set;
        }

        public static IAsyncAction RunIdleAsync(Action action)
        {
            if(Dispatcher == null)
                return Task.Run(action).AsAsyncAction();
            else
                return Dispatcher.RunIdleAsync(e => action());
        }

        public static IAsyncAction RunNormalAsync(Action action)
        {
            if(Dispatcher == null)
                return Task.Run(action).AsAsyncAction();
            else
                return Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
        }

        public static IAsyncAction RunLowAsync(Action action)
        {
            if(Dispatcher == null)
                return Task.Run(action).AsAsyncAction();
            else
                return Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => action());
        }
    }
}
