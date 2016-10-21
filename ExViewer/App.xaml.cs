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
using ExViewer.Views;
using Microsoft.HockeyApp;

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
            HockeyClient.Current.Configure("9c09ca3908114a38a09c81ca8b68ee39", new TelemetryConfiguration
            {
                Collectors =
                    WindowsCollectors.Metadata |
                    WindowsCollectors.Session |
                    WindowsCollectors.UnhandledException
            }).SetExceptionDescriptionLoader(ex =>
            {
                var sb = new System.Text.StringBuilder();
                do
                {
                    sb.AppendLine($"Type: {ex.GetType()}");
                    sb.AppendLine($"HResult: {ex.HResult}");
                    sb.AppendLine($"Message: {ex.Message}");
                    sb.AppendLine();
                    sb.AppendLine("Data:");
                    foreach(var item in ex.Data.Keys)
                    {
                        sb.AppendLine($"    {item}: {ex.Data[item]}");
                    }
                    sb.AppendLine("Stacktrace:");
                    sb.AppendLine(ex.StackTrace);
                    ex = ex.InnerException;
                    sb.AppendLine("--------Inner Exception--------");
                } while(ex != null);
                return sb.ToString();
            });
            this.Suspending += OnSuspending;
            this.Resuming += OnResuming;
            this.RequestedTheme = Settings.SettingCollection.Current.Theme;
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if(System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
                //this.DebugSettings.IsOverdrawHeatMapEnabled = true;
                //this.DebugSettings.IsTextPerformanceVisualizationEnabled = true;
            }
#endif
            lanunchCore(e, e.PrelaunchActivated);
        }

        private async void lanunchCore(IActivatedEventArgs e, bool prelaunchActivated)
        {
            GalaSoft.MvvmLight.Threading.DispatcherHelper.Initialize();
            var currentWindow = Window.Current;
            var currentContent = currentWindow.Content;
            if(currentContent == null)
            {
                var view = ApplicationView.GetForCurrentView();
                view.SetPreferredMinSize(new Size(320, 500));
                currentContent = new SplashControl(e.SplashScreen, e.PreviousExecutionState);
                currentWindow.Content = currentContent;
            }
            if(currentContent is SplashControl)
            {
                if(!prelaunchActivated)
                    ((SplashControl)currentContent).EnableGoToContent();
            }
            else
            {
                currentWindow.Activate();
            }
            await JYAnalyticsUniversal.JYAnalytics.StartTrackAsync("fcf0a9351ea5917ec80d8c1b58b56ff1");
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);
            if(args.Kind == ActivationKind.Protocol)
            {
                var e = (ProtocolActivatedEventArgs)args;
                
            }
            lanunchCore(args, false);
        }

        private async void OnResuming(object sender, object e)
        {
            await JYAnalyticsUniversal.JYAnalytics.StartTrackAsync("fcf0a9351ea5917ec80d8c1b58b56ff1");
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await JYAnalyticsUniversal.JYAnalytics.EndTrackAsync();
            deferral.Complete();
        }
    }
}
