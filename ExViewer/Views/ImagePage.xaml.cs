using ExViewer.Controls;
using ExViewer.Settings;
using ExViewer.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.System.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Color = Windows.UI.Color;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ImagePage : Page
    {
        public ImagePage()
        {
            //HACK:防止崩溃，暂时设为 disabled
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
            this.InitializeComponent();
            var backColor = ((SolidColorBrush)Resources["ApplicationPageBackgroundThemeBrush"]).Color;
            var needColor = (Color)Resources["SystemChromeMediumColor"];
            var toColor = Color.FromArgb(85,
                (byte)(backColor.R - 3 * (backColor.R - needColor.R)),
                (byte)(backColor.G - 3 * (backColor.G - needColor.G)),
                (byte)(backColor.B - 3 * (backColor.B - needColor.B)));

            cb_top.Background = new SolidColorBrush(toColor);
            if(autoOverflowSupported)
                cb_top.IsDynamicOverflowEnabled = false;
        }

        private static bool autoOverflowSupported = Windows.Foundation.Metadata.ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.CommandBar", "IsDynamicOverflowEnabled");

        public GalleryVM VM
        {
            get
            {
                return (GalleryVM)GetValue(VMProperty);
            }
            set
            {
                SetValue(VMProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for VM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty =
            DependencyProperty.Register("VM", typeof(GalleryVM), typeof(ImagePage), new PropertyMetadata(null));

        private readonly ApplicationView av = ApplicationView.GetForCurrentView();
        private readonly DisplayRequest displayRequest = new DisplayRequest();
        private bool displayActived;

        private void btn_pane_Click(object sender, RoutedEventArgs e)
        {
            cb_top.IsOpen = false;
            RootControl.RootController.SwitchSplitView();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            cb_top.Visibility = Visibility.Visible;
            VM = await GalleryVM.GetVMAsync((long)e.Parameter);
            Bindings.Update();
            av.VisibleBoundsChanged += Av_VisibleBoundsChanged;
            Av_VisibleBoundsChanged(av, null);
            fv.SelectedIndex = VM.CurrentIndex;
            await Task.Delay(50);
            fv.SelectedIndex = VM.CurrentIndex;
            fv.Focus(FocusState.Pointer);

            if(SettingCollection.Current.KeepScreenOn)
            {
                displayRequest.RequestActive();
                displayActived = true;
            }
            if(!StatusCollection.Current.ImageViewTipShown)
                showTip();
            fv.FlowDirection = SettingCollection.Current.ReverseFlowDirection ? 
                FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            VM.CurrentIndex = fv.SelectedIndex;
            VM = null;
            if(DeviceTrigger.IsMobile)
                RootControl.RootController.SetFullScreen(false);
            if(displayActived)
            {
                displayRequest.RequestRelease();
                displayActived = false;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            av.VisibleBoundsChanged -= Av_VisibleBoundsChanged;
        }

        bool? isFullScreen;

        private void Av_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            var currentState = RootControl.RootController.IsFullScreen;
            switch(isFullScreen)
            {
            case true:
                if(currentState)
                    return;
                break;
            case false:
                if(!currentState)
                    return;
                break;
            }
            if(currentState)
            {
                abb_fullScreen.Icon = new SymbolIcon(Symbol.BackToWindow);
                abb_fullScreen.Label = LocalizedStrings.Resources.ImagePageBackToWindow;
            }
            else
            {
                abb_fullScreen.Icon = new SymbolIcon(Symbol.FullScreen);
                abb_fullScreen.Label = LocalizedStrings.Resources.ImagePageFullScreen;
            }
            isFullScreen = currentState;
        }

        private void setScale()
        {
            int lb = fv.SelectedIndex - 1;
            int ub = fv.SelectedIndex + 2;
            lb = lb < 0 ? 0 : lb;
            ub = ub > VM.Gallery.Count ? VM.Gallery.Count : ub;
            for(int i = lb; i < ub; i++)
            {
                if(i == fv.SelectedIndex)
                    continue;
                var selected = (FlipViewItem)fv.ContainerFromIndex(i);
                if(selected == null)
                    continue;
                var inner = (Grid)selected.ContentTemplateRoot;
                if(inner == null)
                    continue;
                var ip = (ImagePresenter)inner.FindName("ip");
                ip.ResetZoom();
            }
        }

        IAsyncOperation<LoadMoreItemsResult> loadItems;

        private void fv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(VM?.Gallery == null)
                return;
            var start = fv.SelectedIndex;
            if(start < 0)
                return;
            var end = start + 5;
            if(end > VM.Gallery.Count)
            {
                end = VM.Gallery.Count;
            }
            if(end + 10 > VM.Gallery.Count && VM.Gallery.HasMoreItems)
            {
                if(loadItems == null || loadItems.Status != AsyncStatus.Started)
                {
                    loadItems = VM.Gallery.LoadMoreItemsAsync(5);

                    //HACK:集合改变后应用崩溃的修复，操作系统更新后尝试移除
                    loadItems.Completed = async (s, arg) =>
                    {
                        if(arg != AsyncStatus.Completed)
                            return;
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            var current = fv.SelectedIndex;
                            fv.ItemsSource = null;
                            Bindings.Update();
                            fv.SelectedIndex = current;
                        });
                    };
                    ////
                }
            }
            for(int i = start; i < end; i++)
            {
                var ignore = VM.Gallery[i].LoadImageAsync(false, SettingCollection.Current.GetStrategy(), false);
            }
            setScale();
        }

        private System.Threading.CancellationTokenSource changeCbVisibility;

        private async void fvi_Tapped(object sender, TappedRoutedEventArgs e)
        {
            changeCbVisibility = new System.Threading.CancellationTokenSource();
            await Task.Delay(SettingCollection.Current.ChangeCommandBarDelay, this.changeCbVisibility.Token).ContinueWith(async t =>
            {
                if(t.IsCanceled)
                    return;
                await this.cb_top.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    this.changeCbVisibility.Cancel();
                    switch(this.cb_top.Visibility)
                    {
                    case Visibility.Visible:
                        this.cb_top.Visibility = Visibility.Collapsed;
                        break;
                    case Visibility.Collapsed:
                        this.cb_top.Visibility = Visibility.Visible;
                        break;
                    }
                });
            });
        }

        private void fvi_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if(changeCbVisibility != null)
            {
                if(changeCbVisibility.IsCancellationRequested)
                    switch(cb_top.Visibility)
                    {
                    case Visibility.Visible:
                        cb_top.Visibility = Visibility.Collapsed;
                        break;
                    case Visibility.Collapsed:
                        cb_top.Visibility = Visibility.Visible;
                        break;
                    }
                else
                    changeCbVisibility.Cancel();
            }
            e.Handled = true;
        }

        private async void Flyout_Opening(object sender, object e)
        {
            VM.CurrentIndex = fv.SelectedIndex;
            await VM.RefreshInfoAsync();
        }

        private void cb_top_Opening(object sender, object e)
        {
            cb_top_Open.Begin();
        }

        private void cb_top_Closing(object sender, object e)
        {
            cb_top_Close.Begin();
        }

        private void abb_fullScreen_Click(object sender, RoutedEventArgs e)
        {
            RootControl.RootController.SetFullScreen();
        }

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            base.OnKeyDown(e);
            if(!enterPressed && e.Key == VirtualKey.Enter)
            {
                RootControl.RootController.SetFullScreen();
                enterPressed = true;
                e.Handled = true;
            }
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);
            if(e.Key == VirtualKey.Enter)
            {
                enterPressed = false;
            }
        }

        private bool enterPressed;

        private void abb_Help_Click(object sender, RoutedEventArgs e)
        {
            showTip();
        }

        private static async void showTip()
        {
            await new ContentDialog
            {
                Title = LocalizedStrings.Resources.ImageViewTipsTitle,
                Content = LocalizedStrings.Resources.ImageViewTipsContent,
                PrimaryButtonText = LocalizedStrings.Resources.OK
            }.ShowAsync();
            StatusCollection.Current.ImageViewTipShown = true;
        }
    }
}
