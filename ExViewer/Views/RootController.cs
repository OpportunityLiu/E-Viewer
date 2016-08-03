using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using GalaSoft.MvvmLight.Threading;
using System.Diagnostics;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Navigation;

namespace ExViewer.Views
{
    public partial class RootControl
    {
        internal static class RootController
        {
            static RootController()
            {
                Application.Current.UnhandledException += App_UnhandledException;
                av.VisibleBoundsChanged += Av_VisibleBoundsChanged;
            }

            private static void Av_VisibleBoundsChanged(ApplicationView sender, object args)
            {
                if(IsFullScreen)
                {
                    av.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
                }
                else
                {
                    av.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);
                }
            }

            private static void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
            {
#if DEBUG
                if(Debugger.IsAttached)
                    Debugger.Break();
#endif
                SendToast(e.Exception, null);
                e.Handled = true;
            }

            private static void Frame_NavigationFailed(object sender, NavigationFailedEventArgs e)
            {
#if DEBUG
                if(Debugger.IsAttached)
                    Debugger.Break();
#endif
            }

            private static RootControl root;

            private static Storyboard showPanel, hidePanel, playToast;

            internal static void SetRoot(RootControl root)
            {
                RootController.root = root;
                root.fm_inner.NavigationFailed += Frame_NavigationFailed;
                showPanel = (Storyboard)root.Resources["ShowDisablePanel"];
                hidePanel = (Storyboard)root.Resources["HideDisablePanel"];
                playToast = (Storyboard)root.Resources["PlayToastPanel"];

                showPanel.Completed += ShowPanel_Completed;
                hidePanel.Completed += HidePanel_Completed;
                playToast.Completed += PlayToast_Completed;
            }

            public static void SendToast(Exception ex, Type source)
            {
                SendToast(ex.GetMessage(), source);
            }

            public static void SendToast(string content, Type source)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    if(source != root.fm_inner.Content?.GetType() && source != null)
                        return;
                    root.FindName(nameof(root.bd_Toast));
                    root.tb_Toast.Text = content;
                    root.bd_Toast.Visibility = Visibility.Visible;
                    playToast.Begin();
                });
            }

            private static void PlayToast_Completed(object sender, object e)
            {
                root.bd_Toast.Visibility = Visibility.Collapsed;
            }

            public static void SwitchSplitView()
            {
                if(root == null)
                    return;
                root.sv_root.IsPaneOpen = !root.sv_root.IsPaneOpen;
            }

            public static IAsyncOperation<ContentDialogResult> RequireLogOn()
            {
                return new LogOnDialog().ShowAsync();
            }

            public static bool ViewDisabled
            {
                get; private set;
            }

            public static bool ViewEnabled
            {
                get; private set;
            } = true;

            public static void TrackAsyncAction(IAsyncAction action)
            {
                DisableView(null);
                action.Completed = (s, e) => DispatcherHelper.CheckBeginInvokeOnUI(EnableView);
            }

            public static void TrackAsyncAction(IAsyncActionWithProgress<double> action)
            {
                TrackAsyncAction(action, p => p);
            }

            public static void TrackAsyncAction<TProgress>(IAsyncActionWithProgress<TProgress> action, Func<TProgress, double> progressConverter)
            {
                DisableView(null);
                action.Completed = (s, e) => DispatcherHelper.CheckBeginInvokeOnUI(EnableView);
                action.Progress = (s, p) => DispatcherHelper.CheckBeginInvokeOnUI(() => DisableView(progressConverter(p)));
            }

            public static void TrackAsyncAction<T>(IAsyncOperation<T> action)
            {
                DisableView(null);
                action.Completed = (s, e) => DispatcherHelper.CheckBeginInvokeOnUI(EnableView);
            }

            public static void TrackAsyncAction<T>(IAsyncOperationWithProgress<T, double> action)
            {
                TrackAsyncAction(action, p => p);
            }

            public static void TrackAsyncAction<T, TProgress>(IAsyncOperationWithProgress<T, TProgress> action, Func<TProgress, double> progressConverter)
            {
                DisableView(null);
                action.Completed = (s, e) => DispatcherHelper.CheckBeginInvokeOnUI(EnableView);
                action.Progress = (s, p) => DispatcherHelper.CheckBeginInvokeOnUI(() => DisableView(progressConverter(p)));
            }

            public static void TrackAsyncAction(IAsyncAction action, AsyncActionCompletedHandler completed)
            {
                DisableView(null);
                action.Completed = (s, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
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
                action.Completed = (s, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    EnableView();
                    completed(s, e);
                });
                action.Progress = (s, p) => DispatcherHelper.CheckBeginInvokeOnUI(() => DisableView(progressConverter(p)));
            }

            public static void TrackAsyncAction<T>(IAsyncOperation<T> action, AsyncOperationCompletedHandler<T> completed)
            {
                DisableView(null);
                action.Completed = (s, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
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
                action.Completed = (s, e) => DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    EnableView();
                    completed(s, e);
                });
                action.Progress = (s, p) => DispatcherHelper.CheckBeginInvokeOnUI(() => DisableView(progressConverter(p)));
            }

            private static void DisableView(double? progress)
            {
                if(ViewEnabled && !ViewDisabled)
                {
                    ViewDisabled = true;
                    root.sv_root.IsEnabled = false;
                    root.FindName(nameof(root.rp_Disable));
                    root.rp_Disable.Visibility = Visibility.Visible;
                    showPanel.Begin();
                }
                var indeterminate = !progress.HasValue || double.IsNaN(progress.Value);
                root.pb_Disable.IsIndeterminate = indeterminate;
                if(!indeterminate)
                    root.pb_Disable.Value = progress.Value;
            }

            private static void EnableView()
            {
                if(ViewDisabled)
                {
                    //entering
                    if(ViewEnabled)
                    {
                        showPanel.Stop();
                    }
                    //entered
                    else
                    {
                    }
                    ViewDisabled = false;
                    hidePanel.Begin();
                }
            }

            private static void HidePanel_Completed(object sender, object e)
            {
                root.sv_root.IsEnabled = true;
                root.rp_Disable.Visibility = Visibility.Collapsed;
                ViewEnabled = true;
            }

            private static void ShowPanel_Completed(object sender, object e)
            {
                ViewEnabled = false;
            }

            public static bool IsFullScreen
            {
                get
                {
                    return av.IsFullScreenMode;
                }
            }

            private static ApplicationView av = ApplicationView.GetForCurrentView();

            public static void SetFullScreen(bool fullScreen)
            {
                if(fullScreen)
                {
                    if(av.TryEnterFullScreenMode())
                    {
                        av.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
                    }
                }
                else
                {
                    av.ExitFullScreenMode();
                    av.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);
                }
            }

            public static void SetFullScreen()
            {
                SetFullScreen(!IsFullScreen);
            }

            public static Frame Frame => root?.fm_inner;
        }
    }
}
