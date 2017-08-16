using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using System.Diagnostics;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Navigation;
using ExClient;
using JYAnalyticsUniversal;
using Microsoft.HockeyApp;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using Windows.UI.Core;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Core;
using Opportunity.MvvmUniverse;
using Windows.UI;

namespace ExViewer.Views
{
    public partial class RootControl
    {
        internal static class RootController
        {
            internal static void SetRoot(RootControl root)
            {
                RootController.root = root;

                StatusBar = ApiInfo.StatusBarSupported ? StatusBar.GetForCurrentView() : null;

                root.sv_root.PaneClosing += Sv_root_PaneClosing;

                Frame.Navigated += Frame_Navigated;

                var tb = CoreApplication.GetCurrentView().TitleBar;
                tb.LayoutMetricsChanged += titleBar_LayoutMetricsChanged;
                titleBar_LayoutMetricsChanged(tb, null);

                av_VisibleBoundsChanged(ApplicationView, null);
                ApplicationView.VisibleBoundsChanged += av_VisibleBoundsChanged;

                InputPane.Showing += InputPane_VisibilityChanging;
                InputPane.Hiding += InputPane_VisibilityChanging;
            }

            private static void InputPane_VisibilityChanging(InputPane sender, InputPaneVisibilityEventArgs args)
            {
                args.EnsuredFocusedElementInView = true;
                root.InvalidateMeasure();
            }

            private static void titleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
            {
                var ov = TitleBarHeight;
                var nv = sender.Height;
                if (ov == nv)
                    return;
                TitleBarHeight = nv;
                root.InvalidateMeasure();
            }

            private static Uri launchUri;

            internal static void SetSplitViewButtonPlaceholderVisibility(bool visible)
            {
                var old = SplitViewButtonPlaceholderVisibility;
                if (old == visible)
                    return;
                SplitViewButtonPlaceholderVisibility = visible;
                SplitViewButtonPlaceholderVisibilityChanged?.Invoke(root, visible);
                SetSplitViewButtonOpacity(tbtPaneOpacity);
            }

            public static event TypedEventHandler<RootControl, bool> SplitViewButtonPlaceholderVisibilityChanged;

            public static bool SplitViewButtonPlaceholderVisibility { get; private set; } = true;

            internal static void SetSplitViewButtonOpacity(double opacity)
            {
                tbtPaneOpacity = opacity;
                if (Available)
                {
                    root.tbtPane.Opacity = TbtPaneOpacity;
                    if (TbtPaneOpacity < 0.6)
                        Themes.ThemeExtention.SetStatusBarInfoVisibility(Visibility.Collapsed);
                    else
                        Themes.ThemeExtention.SetStatusBarInfoVisibility(Visibility.Visible);
                }
            }

            private static double tbtPaneOpacity = 1;
            private static double TbtPaneOpacity => SplitViewButtonPlaceholderVisibility ? tbtPaneOpacity : 1;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="uri"></param>
            /// <returns>表示是否在应用内处理</returns>
            public static bool HandleUriLaunch(Uri uri)
            {
                if (uri == null)
                    return true;
                if (Available)
                {
                    return UriHandler.Handle(uri);
                }
                if (!UriHandler.CanHandleInApp(uri))
                {
                    UriHandler.Handle(uri);
                    return false;
                }

                launchUri = uri;
                return true;
            }

            public static bool Available => root != null;

            public static StatusBar StatusBar { get; private set; }

            public static double TitleBarHeight { get; private set; } = -1;


#if !DEBUG_BOUNDS
            async
#endif
            private static void av_VisibleBoundsChanged(ApplicationView sender, object args)
            {
                if (IsFullScreen)
                    sender.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
                else
                    sender.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);
#if !DEBUG_BOUNDS
                if (ApiInfo.StatusBarSupported)
                {
                    if (sender.Orientation == ApplicationViewOrientation.Landscape)
                    {
                        await StatusBar.HideAsync();
                    }
                    else
                    {
                        await StatusBar.ShowAsync();
                    }
                }
#endif
                root.InvalidateMeasure();
            }

            public static bool IsFullScreen => ApplicationView.IsFullScreenMode;

            public static ApplicationView ApplicationView { get; } = ApplicationView.GetForCurrentView();

            public static InputPane InputPane { get; } = InputPane.GetForCurrentView();

            public static void SetFullScreen(bool fullScreen)
            {
                if (fullScreen)
                {
                    if (ApplicationView.TryEnterFullScreenMode())
                    {
                        ApplicationView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
                        Microsoft.Azure.Mobile.Analytics.Analytics.TrackEvent("Full screen entered");
                    }
                }
                else
                {
                    ApplicationView.ExitFullScreenMode();
                    ApplicationView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);
                }
            }

            public static void ChangeFullScreen()
            {
                SetFullScreen(!IsFullScreen);
            }

            public static Frame Frame => root?.fm_inner;

            public static string CurrentPageName
            {
                get;
                private set;
            }

            private static RootControl root;

            public static RootControl Parent => root;

            private static Storyboard sbInitializer(ref Storyboard storage, EventHandler<object> completed, [CallerMemberName]string name = null)
            {
                if (storage != null)
                    return storage;
                var sb = (Storyboard)root.Resources[name];
                if (completed != null)
                    sb.Completed += completed;
                return storage = sb;
            }

            private static Storyboard showPanel, hidePanel, playToast;
            private static Storyboard openSplitViewPane, closeSplitViewPane;

            private static Storyboard ShowDisablePanel => sbInitializer(ref showPanel, ShowPanel_Completed);
            private static Storyboard HideDisablePanel => sbInitializer(ref hidePanel, HidePanel_Completed);
            private static Storyboard PlayToastPanel => sbInitializer(ref playToast, PlayToast_Completed);
            private static Storyboard CloseSplitViewPane => sbInitializer(ref closeSplitViewPane, null);
            private static Storyboard OpenSplitViewPane => sbInitializer(ref openSplitViewPane, null);

            private static void Sv_root_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
            {
                CloseSplitViewPane.Begin();
                root.CloseSplitViewPaneBtnPane.To = TbtPaneOpacity;
                if (TbtPaneOpacity < 0.6)
                    Themes.ThemeExtention.SetStatusBarInfoVisibility(Visibility.Collapsed);
                else
                    Themes.ThemeExtention.SetStatusBarInfoVisibility(Visibility.Visible);
                root.manager.AppViewBackButtonVisibilityOverride = null;
            }

            internal static void HandleUriLaunch()
            {
                if (launchUri != null)
                    HandleUriLaunch(launchUri);
            }

            private static void Frame_Navigated(object sender, NavigationEventArgs e)
            {
                CurrentPageName = Frame.Content.GetType().ToString();
                Microsoft.Azure.Mobile.Analytics.Analytics.TrackEvent("Navigated", new Dictionary<string, string> { ["Type"] = e.SourcePageType.ToString() });
            }

            public static void SendToast(Exception ex, Type source)
            {
                var sourceString = source?.ToString() ?? "null";
#if DEBUG
                Debug.WriteLine(ex, "Exception");
#else
                JYAnalytics.TrackError($"Exception {ex.HResult:X8}: {ex.GetType().ToString()} at {sourceString}");
                HockeyClient.Current.TrackException(ex);
#endif
                SendToast(ex.GetMessage(), source);
            }

            public static void SendToast(string content, Type source)
            {
                if (!Available)
                    return;
                DispatcherHelper.BeginInvokeOnUIThread(() =>
                {
                    if (source != null && source != root.fm_inner.Content?.GetType())
                        return;
                    root.FindName(nameof(root.bd_Toast));
                    root.tb_Toast.Text = content;
                    root.bd_Toast.Visibility = Visibility.Visible;
                    PlayToastPanel.Begin();
                });
            }

            private static void PlayToast_Completed(object sender, object e)
            {
                root.bd_Toast.Visibility = Visibility.Collapsed;
            }

            public static void SwitchSplitView(bool? open)
            {
                if (!Available)
                    return;
                var currentState = root.sv_root.IsPaneOpen;
                if (open == currentState)
                    return;
                var targetState = open ?? !currentState;
                if (targetState)
                {
                    (root.fm_inner.Content as IHasAppBar)?.CloseAll();
                    root.sv_root.IsPaneOpen = true;
                    OpenSplitViewPane.Begin();
                    Themes.ThemeExtention.SetStatusBarInfoVisibility(Visibility.Visible);
                    root.manager.AppViewBackButtonVisibilityOverride = AppViewBackButtonVisibility.Collapsed;
                    var currentTab = root.tabs.Keys.FirstOrDefault(t => t.IsChecked) ?? root.svt_Search;
                    currentTab.Focus(FocusState.Programmatic);
                }
                else
                {
                    root.sv_root.IsPaneOpen = false;
                }
            }

            public static IAsyncOperation<bool> RequestLogOn()
            {
                return Run(async token =>
                {
                    var result = await new LogOnDialog().ShowAsync();
                    var succeed = !Client.Current.NeedLogOn;
                    JYAnalytics.TrackEvent("LogOnRequested", $"Result: {(succeed ? "Succeed" : "Failed")}");
                    Microsoft.Azure.Mobile.Analytics.Analytics.TrackEvent("Log on requested", new Dictionary<string, string> { ["Result"] = (succeed ? "Succeed" : "Failed") });
                    UpdateUserInfo(result == ContentDialogResult.Primary);
                    if (succeed)
                    {
                        Settings.SettingCollection.Current.Apply();
                    }
                    return succeed;
                });
            }

            public static async void UpdateUserInfo(bool setNull)
            {
                if (Available && setNull)
                {
                    root.UserInfo = null;
                }
                if (Client.Current.UserID == -1)
                    return;
                var info = default(UserInfo);
                try
                {
                    info = await Client.Current.FetchCurrentUserInfoAsync();
                    await info.SaveToCache();
                }
                catch (Exception)
                {
                    info = await UserInfo.LoadFromCache();
                }
                if (Available)
                {
                    root.UserInfo = info;
                }
            }

            public static void TrackAsyncAction(IAsyncAction action)
            {
                DisableView(null);
                action.Completed = (s, e) => DispatcherHelper.BeginInvokeOnUIThread(EnableView);
            }

            public static void TrackAsyncAction(IAsyncActionWithProgress<double> action)
            {
                TrackAsyncAction(action, p => p);
            }

            public static void TrackAsyncAction<TProgress>(IAsyncActionWithProgress<TProgress> action, Func<TProgress, double> progressConverter)
            {
                DisableView(null);
                action.Completed = (s, e) => DispatcherHelper.BeginInvokeOnUIThread(EnableView);
                action.Progress = (s, p) => DispatcherHelper.BeginInvokeOnUIThread(() => DisableView(progressConverter(p)));
            }

            public static void TrackAsyncAction<T>(IAsyncOperation<T> action)
            {
                DisableView(null);
                action.Completed = (s, e) => DispatcherHelper.BeginInvokeOnUIThread(EnableView);
            }

            public static void TrackAsyncAction<T>(IAsyncOperationWithProgress<T, double> action)
            {
                TrackAsyncAction(action, p => p);
            }

            public static void TrackAsyncAction<T, TProgress>(IAsyncOperationWithProgress<T, TProgress> action, Func<TProgress, double> progressConverter)
            {
                DisableView(null);
                action.Completed = (s, e) => DispatcherHelper.BeginInvokeOnUIThread(EnableView);
                action.Progress = (s, p) => DispatcherHelper.BeginInvokeOnUIThread(() => DisableView(progressConverter(p)));
            }

            public static void TrackAsyncAction(IAsyncAction action, AsyncActionCompletedHandler completed)
            {
                DisableView(null);
                action.Completed = (s, e) => DispatcherHelper.BeginInvokeOnUIThread(() =>
                {
                    EnableView();
                    completed(s, e);
                });
            }

            public static void TrackAsyncAction(IAsyncActionWithProgress<double> action, AsyncActionWithProgressCompletedHandler<double> completed)
            {
                TrackAsyncAction(action, p => p, completed);
            }

            public static void TrackAsyncAction<TProgress>(IAsyncActionWithProgress<TProgress> action, Func<TProgress, double> progressConverter, AsyncActionWithProgressCompletedHandler<TProgress> completed)
            {
                DisableView(null);
                action.Completed = (s, e) => DispatcherHelper.BeginInvokeOnUIThread(() =>
                {
                    EnableView();
                    completed(s, e);
                });
                action.Progress = (s, p) => DispatcherHelper.BeginInvokeOnUIThread(() => DisableView(progressConverter(p)));
            }

            public static void TrackAsyncAction<T>(IAsyncOperation<T> action, AsyncOperationCompletedHandler<T> completed)
            {
                DisableView(null);
                action.Completed = (s, e) => DispatcherHelper.BeginInvokeOnUIThread(() =>
                {
                    EnableView();
                    completed(s, e);
                });
            }

            public static void TrackAsyncAction<T>(IAsyncOperationWithProgress<T, double> action, AsyncOperationWithProgressCompletedHandler<T, double> completed)
            {
                TrackAsyncAction(action, p => p, completed);
            }

            public static void TrackAsyncAction<T, TProgress>(IAsyncOperationWithProgress<T, TProgress> action, Func<TProgress, double> progressConverter, AsyncOperationWithProgressCompletedHandler<T, TProgress> completed)
            {
                DisableView(null);
                action.Completed = (s, e) => DispatcherHelper.BeginInvokeOnUIThread(() =>
                {
                    EnableView();
                    completed(s, e);
                });
                action.Progress = (s, p) => DispatcherHelper.BeginInvokeOnUIThread(() => DisableView(progressConverter(p)));
            }

            public static bool ViewEnabled
            {
                get; private set;
            } = true;

            private static void DisableView(double? progress)
            {
                ViewEnabled = false;

                root.FindName(nameof(root.rp_Disable));
                root.sv_root.IsEnabled = false;
                root.rp_Disable.Visibility = Visibility.Visible;
                var indeterminate = !progress.HasValue || double.IsNaN(progress.Value);
                root.pb_Disable.IsIndeterminate = indeterminate;
                if (!indeterminate)
                    root.pb_Disable.Value = progress.Value;

                HideDisablePanel.Stop();
                ShowDisablePanel.Begin();
            }

            private static async void EnableView()
            {
                ViewEnabled = true;

                root.sv_root.IsEnabled = true;

                ShowDisablePanel.Stop();
                HideDisablePanel.Begin();

                await root.Dispatcher.Yield();
                root.Focus(FocusState.Programmatic);
            }

            private static void HidePanel_Completed(object sender, object e)
            {
                root.rp_Disable.Visibility = Visibility.Collapsed;
            }

            private static void ShowPanel_Completed(object sender, object e)
            {
            }
        }
    }
}
