using ExClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ExViewer
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if(System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            if(Client.Current == null)
            {
                var pv = new PasswordVault();
                try
                {
                    var pass = pv.FindAllByResource("ex").First();
                    pass.RetrievePassword();
                    await Client.CreateClient(pass.UserName, pass.Password);
                }
                catch(Exception ex) when(ex.HResult == -2147023728)
                {
                }
            }

            var current = Window.Current;
            ExClient.DispatcherHelper.Dispatcher = current.Dispatcher;
            if(current.Content == null)
            {
                if(e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }
                var tb = ApplicationView.GetForCurrentView().TitleBar;

                tb.BackgroundColor = (Color)Resources["SystemChromeMediumColor"];
                tb.InactiveBackgroundColor = (Color)Resources["SystemChromeMediumColor"];
                tb.ButtonBackgroundColor = (Color)Resources["SystemChromeMediumColor"];
                tb.ButtonHoverBackgroundColor = (Color)Resources["SystemChromeMediumLowColor"];
                tb.ButtonInactiveBackgroundColor = (Color)Resources["SystemChromeMediumColor"];
                tb.ButtonPressedBackgroundColor = (Color)Resources["SystemChromeHighColor"];

                tb.ForegroundColor = (Color)Resources["SystemBaseMediumHighColor"];
                tb.InactiveForegroundColor = (Color)Resources["SystemChromeDisabledLowColor"];
                tb.ButtonForegroundColor = (Color)Resources["SystemBaseMediumHighColor"];
                tb.ButtonHoverForegroundColor = (Color)Resources["SystemBaseMediumHighColor"];
                tb.ButtonInactiveForegroundColor = (Color)Resources["SystemChromeDisabledLowColor"];
                tb.ButtonPressedForegroundColor = (Color)Resources["SystemBaseMediumHighColor"];

                current.Content = new Views.MainPage();
            }
            current.Activate();
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }
    }
}
