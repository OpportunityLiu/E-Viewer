using ExClient;
using ExClient.Forums;
using ExClient.Status;
using Opportunity.MvvmUniverse;
using Opportunity.MvvmUniverse.Services.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExViewer.Views
{
    public partial class RootControl
    {
        internal static class RootController
        {
            internal static void SetRoot(RootControl root)
            {
                RootController.root = root;

                if (ExApiInfo.StatusBarSupported)
                {
                    StatusBar = StatusBar.GetForCurrentView();
                    av_VisibleBoundsChanged(ApplicationView, null);
                    ApplicationView.VisibleBoundsChanged += av_VisibleBoundsChanged;
                }

                root.sv_root.PaneClosing += Sv_root_PaneClosing;

                Frame.Navigated += Frame_Navigated;

                Controls.AboutControl.UpdateEhWiki.Executed += (s, e) =>
                {
                    if (e.Exception != null)
                    {
                        SendToast(Strings.Resources.Database.EhTagClient.Update.Failed, null);
                    }
                    else
                    {
                        SendToast(Strings.Resources.Database.EhTagClient.Update.Succeeded, null);
                    }

                    e.Handled = true;
                };
                Controls.AboutControl.UpdateETT.Executed += (s, e) =>
                {
                    if (e.Exception != null)
                    {
                        SendToast(Strings.Resources.Database.EhTagTranslatorClient.Update.Failed, null);
                    }
                    else
                    {
                        SendToast(Strings.Resources.Database.EhTagTranslatorClient.Update.Succeeded, null);
                    }

                    e.Handled = true;
                };
            }

            private static Uri launchUri;

            internal static void SetSplitViewButtonPlaceholderVisibility(bool visible)
            {
                var old = SplitViewButtonPlaceholderVisibility;
                if (old == visible)
                {
                    return;
                }

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
                    {
                        Themes.ThemeExtention.SetStatusBarInfoVisibility(Visibility.Collapsed);
                    }
                    else
                    {
                        Themes.ThemeExtention.SetStatusBarInfoVisibility(Visibility.Visible);
                    }
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
                if (uri is null)
                    return true;
                if (Available && Window.Current.Content == root)
                    return UriHandler.Handle(uri);
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

#if !DEBUG_BOUNDS
            async
#endif
            private static void av_VisibleBoundsChanged(ApplicationView sender, object args)
            {
#if !DEBUG_BOUNDS
                if (ExApiInfo.StatusBarSupported)
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
            }

            public static bool IsFullScreen => ApplicationView.IsFullScreenMode;

            public static ApplicationView ApplicationView { get; } = ApplicationView.GetForCurrentView();

            public static void SetFullScreen(bool fullScreen)
            {
                if (fullScreen)
                {
                    if (ApplicationView.TryEnterFullScreenMode())
                    {
                        ApplicationView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
                        Microsoft.AppCenter.Analytics.Analytics.TrackEvent("Full screen entered");
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

            public static Navigator Navigator => root.manager;

            public static Frame Frame => root?.fm_inner;

            private static RootControl root;

            public static RootControl Parent => root;

            private static Storyboard sbInitializer(ref Storyboard storage, EventHandler<object> completed, [CallerMemberName]string name = null)
            {
                if (storage != null)
                {
                    return storage;
                }

                var sb = (Storyboard)root.Resources[name];
                if (completed != null)
                {
                    sb.Completed += completed;
                }

                return storage = sb;
            }

            private static Storyboard playToast;
            private static Storyboard openSplitViewPane, closeSplitViewPane;

            private static Storyboard PlayToastPanel => sbInitializer(ref playToast, PlayToast_Completed);
            private static Storyboard CloseSplitViewPane => sbInitializer(ref closeSplitViewPane, null);
            private static Storyboard OpenSplitViewPane => sbInitializer(ref openSplitViewPane, null);

            private static void Sv_root_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
            {
                CloseSplitViewPane.Begin();
                root.CloseSplitViewPaneBtnPane.To = TbtPaneOpacity;
                if (TbtPaneOpacity < 0.6)
                {
                    Themes.ThemeExtention.SetStatusBarInfoVisibility(Visibility.Collapsed);
                }
                else
                {
                    Themes.ThemeExtention.SetStatusBarInfoVisibility(Visibility.Visible);
                }

                root.manager.IsBackEnabled = true;
            }

            internal static void HandleUriLaunch()
            {
                HandleUriLaunch(launchUri);
            }

            private static void Frame_Navigated(object sender, NavigationEventArgs e)
            {
                Microsoft.AppCenter.Analytics.Analytics.TrackEvent("Navigated", new Dictionary<string, string> { ["Type"] = e.SourcePageType.ToString() });
            }

            public static void SendToast(Exception ex, Type source)
            {
                if (ex is null)
                {
                    throw new ArgumentNullException(nameof(ex));
                }
                Telemetry.LogException(ex);
#if DEBUG
                Debug.WriteLine(ex, "Exception");
#endif
                SendToast(ex.GetMessage(), source);
            }

            public static void SendToast(string content, Type source)
            {
                if (!Available)
                {
                    return;
                }

                if (content is null)
                {
                    throw new ArgumentNullException(nameof(content));
                }

                root.Dispatcher.Begin(() =>
                {
                    if (source != null && source != root.fm_inner.Content?.GetType())
                    {
                        return;
                    }

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
                {
                    return;
                }

                var currentState = root.sv_root.IsPaneOpen;
                if (open == currentState)
                {
                    return;
                }

                var targetState = open ?? !currentState;
                if (targetState)
                {
                    (root.fm_inner.Content as IHasAppBar)?.CloseAll();
                    root.sv_root.IsPaneOpen = true;
                    OpenSplitViewPane.Begin();
                    Themes.ThemeExtention.SetStatusBarInfoVisibility(Visibility.Visible);
                    root.manager.IsBackEnabled = false;
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
                    await CoreApplication.MainView.Dispatcher.YieldIdle();
                    var dialog = new LogOnDialog();
                    await dialog.ShowAsync();
                    Microsoft.AppCenter.Analytics.Analytics.TrackEvent("Log on requested", new Dictionary<string, string> { ["Result"] = (dialog.Succeed ? "Succeed" : "Failed") });
                    UpdateUserInfo(dialog.Succeed);
                    if (dialog.Succeed)
                        Settings.SettingCollection.Current.Apply();
                    return dialog.Succeed;
                });
            }

            public static async void UpdateUserInfo(bool setNull)
            {
                if (Available && setNull)
                {
                    root.UserInfo = null;
                }
                if (Client.Current.UserId == -1)
                {
                    return;
                }

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
                action.Completed = (s, e) => root.Dispatcher.Begin(EnableView);
            }

            public static void TrackAsyncAction(IAsyncActionWithProgress<double> action)
            {
                TrackAsyncAction(action, p => p);
            }

            public static void TrackAsyncAction<TProgress>(IAsyncActionWithProgress<TProgress> action, Func<TProgress, double> progressConverter)
            {
                DisableView(null);
                action.Completed = (s, e) => root.Dispatcher.Begin(EnableView);
                action.Progress = (s, p) => root.Dispatcher.Begin(() => DisableView(progressConverter(p)));
            }

            public static void TrackAsyncAction<T>(IAsyncOperation<T> action)
            {
                DisableView(null);
                action.Completed = (s, e) => root.Dispatcher.Begin(EnableView);
            }

            public static void TrackAsyncAction<T>(IAsyncOperationWithProgress<T, double> action)
            {
                TrackAsyncAction(action, p => p);
            }

            public static void TrackAsyncAction<T, TProgress>(IAsyncOperationWithProgress<T, TProgress> action, Func<TProgress, double> progressConverter)
            {
                DisableView(null);
                action.Completed = (s, e) => root.Dispatcher.Begin(EnableView);
                action.Progress = (s, p) => root.Dispatcher.Begin(() => DisableView(progressConverter(p)));
            }

            public static void TrackAsyncAction(IAsyncAction action, AsyncActionCompletedHandler completed)
            {
                DisableView(null);
                action.Completed = (s, e) => root.Dispatcher.Begin(() =>
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
                action.Completed = (s, e) => root.Dispatcher.Begin(() =>
                {
                    EnableView();
                    completed(s, e);
                });
                action.Progress = (s, p) => root.Dispatcher.Begin(() => DisableView(progressConverter(p)));
            }

            public static void TrackAsyncAction<T>(IAsyncOperation<T> action, AsyncOperationCompletedHandler<T> completed)
            {
                DisableView(null);
                action.Completed = (s, e) => root.Dispatcher.Begin(() =>
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
                action.Completed = (s, e) => root.Dispatcher.Begin(() =>
                {
                    EnableView();
                    completed(s, e);
                });
                action.Progress = (s, p) => root.Dispatcher.Begin(() => DisableView(progressConverter(p)));
            }

            public static bool ViewEnabled { get; private set; } = true;

            private static void DisableView(double? progress)
            {
                ViewEnabled = false;

                root.FindName(nameof(root.rp_Disable));
                root.sv_root.IsEnabled = false;

                root.manager.IsBackEnabled = false;
                root.manager.IsForwardEnabled = false;

                root.rp_Disable.Visibility = Visibility.Visible;

                var indeterminate = !progress.HasValue;
                var keep = double.IsNaN(progress.GetValueOrDefault());
                if (!keep)
                {
                    root.pb_Disable.IsIndeterminate = indeterminate;
                    if (!indeterminate)
                    {
                        root.pb_Disable.Value = progress.Value;
                    }
                }
            }

            private static async void EnableView()
            {
                ViewEnabled = true;

                root.sv_root.IsEnabled = true;

                root.manager.IsBackEnabled = true;
                root.manager.IsForwardEnabled = true;

                root.rp_Disable.Visibility = Visibility.Collapsed;

                await root.Dispatcher.Yield();
                root.Focus(FocusState.Programmatic);
            }
        }
    }
}
