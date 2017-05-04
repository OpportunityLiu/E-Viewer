using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using ExViewer.Views;
using Windows.ApplicationModel.Core;
#if !DEBUG
using Microsoft.HockeyApp;
#endif

namespace ExViewer
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {

        // [STAThread()]
        //public extern void a();
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
#if !DEBUG
            HockeyClient.Current.Configure("9c09ca3908114a38a09c81ca8b68ee39", new TelemetryConfiguration
            {
                Collectors =
                    WindowsCollectors.Metadata |
                    WindowsCollectors.Session |
                    WindowsCollectors.UnhandledException
            }).SetExceptionDescriptionLoader(Telemetry.LogException);
#endif
            this.Suspending += this.OnSuspending;
            this.Resuming += this.OnResuming;
            this.UnhandledException += this.App_UnhandledException;
            this.RequestedTheme = Settings.SettingCollection.Current.Theme;
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            RootControl.RootController.SendToast(e.Exception, null);
            e.Handled = true;
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
                this.DebugSettings.IsTextPerformanceVisualizationEnabled = true;
            }
#endif
            lanunchCore(e, e.PrelaunchActivated);
        }

        private async void lanunchCore(IActivatedEventArgs e, bool prelaunchActivated)
        {
            Opportunity.MvvmUniverse.Helpers.DispatcherHelper.Init();
            Opportunity.MvvmUniverse.Helpers.DispatcherHelper.UseForNotification = true;
            var currentWindow = Window.Current;
            var currentContent = currentWindow.Content;
            if(currentContent == null)
            {
                var view = ApplicationView.GetForCurrentView();
                view.SetPreferredMinSize(new Size(320, 500));
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                currentContent = new SplashControl(e.SplashScreen, e.PreviousExecutionState);
                currentWindow.Content = currentContent;
            }
            if(currentContent is SplashControl sc)
            {
                if(!prelaunchActivated)
                    sc.EnableGoToContent();
            }
            else
            {
                currentWindow.Activate();
            }
            await JYAnalyticsUniversal.JYAnalytics.StartTrackAsync("fcf0a9351ea5917ec80d8c1b58b56ff1");
            ((Opportunity.Converters.ObjectToBooleanConverter)this.Resources["EmptyStringToCollapsedConverter"]).ValueForFalse = "";
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);
            if(args.Kind == ActivationKind.Protocol)
            {
                var e = (ProtocolActivatedEventArgs)args;
                var needHandleInApp = RootControl.RootController.HandleUriLaunch(e.Uri);
                if(!needHandleInApp
                    && e.PreviousExecutionState != ApplicationExecutionState.Running
                    && e.PreviousExecutionState != ApplicationExecutionState.Suspended)
                    Exit();
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
