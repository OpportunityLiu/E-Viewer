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
                RootController.Parent = root;

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
                SplitViewButtonPlaceholderVisibilityChanged?.Invoke(Parent, visible);
                SetSplitViewButtonOpacity(tbtPaneOpacity);
            }

            public static event TypedEventHandler<RootControl, bool> SplitViewButtonPlaceholderVisibilityChanged;

            public static bool SplitViewButtonPlaceholderVisibility { get; private set; } = true;

            internal static void SetSplitViewButtonOpacity(double opacity)
            {
                tbtPaneOpacity = opacity;
                if (Available)
                {
                    Parent.tbtPane.Opacity = TbtPaneOpacity;
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
                if (Available)
                    return UriHandler.Handle(uri);
                if (!UriHandler.CanHandleInApp(uri))
                {
                    UriHandler.Handle(uri);
                    return false;
                }
                launchUri = uri;
                return true;
            }

            public static bool Available => Parent != null && Window.Current?.Content == Parent;

            public static StatusBar StatusBar { get; private set; }

#if !DEBUG_BOUNDS
            private
#endif
            static async void av_VisibleBoundsChanged(ApplicationView sender, object args)
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

            public static Navigator Navigator => Parent.manager;

            public static Frame Frame => Parent?.fm_inner;

            public static RootControl Parent { get; private set; }

            private static Storyboard sbInitializer(ref Storyboard storage, EventHandler<object> completed, [CallerMemberName]string name = null)
            {
                if (storage != null)
                {
                    return storage;
                }

                var sb = (Storyboard)Parent.Resources[name];
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
                Parent.CloseSplitViewPaneBtnPane.To = TbtPaneOpacity;
                if (TbtPaneOpacity < 0.6)
                {
                    Themes.ThemeExtention.SetStatusBarInfoVisibility(Visibility.Collapsed);
                }
                else
                {
                    Themes.ThemeExtention.SetStatusBarInfoVisibility(Visibility.Visible);
                }

                Parent.manager.IsBackEnabled = true;
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
                    return;
                if (content is null)
                    throw new ArgumentNullException(nameof(content));

                Parent.Dispatcher.Begin(() =>
                {
                    if (source != null && source != Parent.fm_inner.Content?.GetType())
                    {
                        return;
                    }

                    Parent.FindName(nameof(Parent.bd_Toast));
                    Parent.tb_Toast.Text = content;
                    Parent.bd_Toast.Visibility = Visibility.Visible;
                    PlayToastPanel.Begin();
                });
            }

            private static void PlayToast_Completed(object sender, object e)
            {
                Parent.bd_Toast.Visibility = Visibility.Collapsed;
            }

            public static void SwitchSplitView(bool? open)
            {
                if (!Available)
                    return;

                var currentState = Parent.sv_root.IsPaneOpen;
                if (open == currentState)
                    return;

                var targetState = open ?? !currentState;
                if (targetState)
                {
                    (Parent.fm_inner.Content as IHasAppBar)?.CloseAll();
                    Parent.sv_root.IsPaneOpen = true;
                    OpenSplitViewPane.Begin();
                    Themes.ThemeExtention.SetStatusBarInfoVisibility(Visibility.Visible);
                    Parent.manager.IsBackEnabled = false;
                    var currentTab = Parent.tabs.Keys.FirstOrDefault(t => t.IsChecked) ?? Parent.svt_Search;
                    currentTab.Focus(FocusState.Programmatic);
                }
                else
                {
                    Parent.sv_root.IsPaneOpen = false;
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
                    Parent.UserInfo = null;
                if (Client.Current.UserId < 0)
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
                    Parent.UserInfo = info;
            }

            public static void TrackAsyncAction(IAsyncAction action)
            {
                DisableView(null);
                action.Completed = (s, e) => Parent.Dispatcher.Begin(EnableView);
            }

            public static void TrackAsyncAction(IAsyncActionWithProgress<double> action)
            {
                TrackAsyncAction(action, p => p);
            }

            public static void TrackAsyncAction<TProgress>(IAsyncActionWithProgress<TProgress> action, Func<TProgress, double> progressConverter)
            {
                DisableView(null);
                action.Completed = (s, e) => Parent.Dispatcher.Begin(EnableView);
                action.Progress = (s, p) => Parent.Dispatcher.Begin(() => DisableView(progressConverter(p)));
            }

            public static void TrackAsyncAction<T>(IAsyncOperation<T> action)
            {
                DisableView(null);
                action.Completed = (s, e) => Parent.Dispatcher.Begin(EnableView);
            }

            public static void TrackAsyncAction<T>(IAsyncOperationWithProgress<T, double> action)
            {
                TrackAsyncAction(action, p => p);
            }

            public static void TrackAsyncAction<T, TProgress>(IAsyncOperationWithProgress<T, TProgress> action, Func<TProgress, double> progressConverter)
            {
                DisableView(null);
                action.Completed = (s, e) => Parent.Dispatcher.Begin(EnableView);
                action.Progress = (s, p) => Parent.Dispatcher.Begin(() => DisableView(progressConverter(p)));
            }

            public static void TrackAsyncAction(IAsyncAction action, AsyncActionCompletedHandler completed)
            {
                DisableView(null);
                action.Completed = (s, e) => Parent.Dispatcher.Begin(() =>
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
                action.Completed = (s, e) => Parent.Dispatcher.Begin(() =>
                {
                    EnableView();
                    completed(s, e);
                });
                action.Progress = (s, p) => Parent.Dispatcher.Begin(() => DisableView(progressConverter(p)));
            }

            public static void TrackAsyncAction<T>(IAsyncOperation<T> action, AsyncOperationCompletedHandler<T> completed)
            {
                DisableView(null);
                action.Completed = (s, e) => Parent.Dispatcher.Begin(() =>
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
                action.Completed = (s, e) => Parent.Dispatcher.Begin(() =>
                {
                    EnableView();
                    completed(s, e);
                });
                action.Progress = (s, p) => Parent.Dispatcher.Begin(() => DisableView(progressConverter(p)));
            }

            public static bool ViewEnabled { get; private set; } = true;

            private static void DisableView(double? progress)
            {
                ViewEnabled = false;

                Parent.FindName(nameof(Parent.rp_Disable));
                Parent.sv_root.IsEnabled = false;

                Parent.manager.IsBackEnabled = false;
                Parent.manager.IsForwardEnabled = false;

                Parent.rp_Disable.Visibility = Visibility.Visible;

                var indeterminate = !progress.HasValue;
                var keep = double.IsNaN(progress.GetValueOrDefault());
                if (!keep)
                {
                    Parent.pb_Disable.IsIndeterminate = indeterminate;
                    if (!indeterminate)
                    {
                        Parent.pb_Disable.Value = progress.Value;
                    }
                }
            }

            private static async void EnableView()
            {
                ViewEnabled = true;

                Parent.sv_root.IsEnabled = true;

                Parent.manager.IsBackEnabled = true;
                Parent.manager.IsForwardEnabled = true;

                Parent.rp_Disable.Visibility = Visibility.Collapsed;

                await Parent.Dispatcher.Yield();
                Parent.Focus(FocusState.Programmatic);
            }
        }
    }
}
