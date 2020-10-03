using ExViewer.Helpers;
using ExViewer.Views;
using Microsoft.AppCenter.Analytics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
#if !DEBUG
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
#endif

namespace ExViewer
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    public sealed partial class App : Application
    {
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            InitializeComponent();
            RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;
            Suspending += OnSuspending;
            Resuming += OnResuming;
            UnhandledException += App_UnhandledException;
            RequestedTheme = Settings.SettingCollection.Current.Theme;
            ExDawnOfDayTask.Task.Register();
            //ElementSoundPlayer.State = ElementSoundPlayerState.On;
        }

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            var ex = e.Exception;
            switch (ex)
            {
            case OperationCanceledException _:
                break;
            default:
                RootControl.RootController.SendToast(ex, null);
                break;
            }
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
            if (System.Diagnostics.Debugger.IsAttached)
            {
                DebugSettings.EnableFrameRateCounter = true;
                //this.DebugSettings.IsOverdrawHeatMapEnabled = true;
                //this.DebugSettings.IsTextPerformanceVisualizationEnabled = true;
            }
#endif
            if (!e.Arguments.IsNullOrEmpty())
                lanuchUri(new Uri(e.Arguments), e.PreviousExecutionState);
            lanunchCore(e, e.PrelaunchActivated);
        }

        private void lanunchCore(IActivatedEventArgs e, bool prelaunchActivated)
        {
#if !DEBUG
            if (!AppCenter.Configured)
            {
                var region = new Windows.Globalization.GeographicRegion();
                AppCenter.SetCountryCode(region.CodeTwoLetter);
                AppCenter.Start(Telemetry.AppCenterKey, typeof(Analytics), typeof(Crashes));
            }
#endif
            var notificator = Opportunity.MvvmUniverse.Services.Notification.Notificator.GetForCurrentView();
            if (notificator.Handlers.Count == 0)
            {
                notificator.Handlers.Add(new Services.ContentDialogNotification());
                notificator.Handlers.Add(new Services.InAppToastNotification());
            }
            var jumplist = JumplistManager.RefreshJumplistAsync();
            var currentWindow = Window.Current;
            var currentContent = currentWindow.Content;
            if (currentContent is null)
            {
                var view = ApplicationView.GetForCurrentView();
                view.SetPreferredMinSize(new Size(320, 500));
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                currentContent = new SplashControl(e.SplashScreen);
                currentWindow.Content = currentContent;
            }
            if (currentContent is SplashControl sc)
            {
                if (!prelaunchActivated)
                {
                    sc.EnableGoToContent();
                }
            }
            else
            {
                currentWindow.Activate();
            }
            ((Opportunity.UWP.Converters.Typed.StringToBooleanConverter)Resources["EmptyStringToFalseConverter"]).ValuesForFalse.Add("");
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Protocol)
            {
                var e = (ProtocolActivatedEventArgs)args;
                lanuchUri(e.Uri, e.PreviousExecutionState);
            }
            lanunchCore(args, false);
        }

        private void lanuchUri(Uri uri, ApplicationExecutionState previousExecutionState)
        {
            var needHandleInApp = RootControl.RootController.HandleUriLaunch(uri);
            if (!needHandleInApp
                && previousExecutionState != ApplicationExecutionState.Running
                && previousExecutionState != ApplicationExecutionState.Suspended)
            {
                Exit();
            }
            if (needHandleInApp)
            {
                Analytics.TrackEvent("Launched by uri", new Dictionary<string, string> { ["Uri"] = uri.ToString() });
            }
        }

        private void OnResuming(object sender, object e)
        {
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
            var def = e.SuspendingOperation.GetDeferral();
            try
            {
                await JumplistManager.RefreshJumplistAsync();
            }
            finally
            {
                def.Complete();
            }
        }
    }
}
