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
using AsyncFriendlyStackTrace;
using Microsoft.HockeyApp;
using ExViewer.Views;

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
#if !DEBUG
            HockeyClient.Current.Configure("9c09ca3908114a38a09c81ca8b68ee39", new TelemetryConfiguration()
            {
                Collectors =
                    WindowsCollectors.Metadata |
                    WindowsCollectors.Session |
                    WindowsCollectors.UnhandledException
                //,
                //DescriptionLoader = ex =>
                //{
                //    var sb = new System.Text.StringBuilder();
                //    do
                //    {
                //        sb.AppendLine($"Type: {ex.GetType()}");
                //        sb.AppendLine($"HResult: {ex.HResult}");
                //        sb.AppendLine($"Message: {ex.Message}");
                //        sb.AppendLine();
                //        sb.AppendLine("Data:");
                //        foreach(var item in ex.Data.Keys)
                //        {
                //            sb.AppendLine($"    {item}: {ex.Data[item]}");
                //        }
                //        sb.AppendLine("Stacktrace:");
                //        sb.AppendLine(ex.StackTrace);
                //        sb.AppendLine("AsyncStacktrace:");
                //        sb.AppendLine(ex.ToAsyncString());
                //        ex = ex.InnerException;
                //        sb.AppendLine("--------Inner Exception--------");
                //    } while(ex != null);
                //    return sb.ToString();
                //}
            });
#endif
            this.Suspending += OnSuspending;
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
            if(e.PrelaunchActivated)
                return;

            var currentWindow = Window.Current;
            GalaSoft.MvvmLight.Threading.DispatcherHelper.Initialize();
            var currentContent = currentWindow.Content;
            if(currentContent is SplashControl)
            {
            }
            else if(currentContent == null)
            {
                var view = ApplicationView.GetForCurrentView();
                view.SetPreferredMinSize(new Size(320, 500));
                currentWindow.Content = new SplashControl(e.SplashScreen, e.PreviousExecutionState);
            }
            else
            {
                currentWindow.Activate();
            }
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
