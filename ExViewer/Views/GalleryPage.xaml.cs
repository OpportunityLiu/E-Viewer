using ExClient;
using ExClient.Galleries;
using ExClient.Galleries.Rating;
using ExViewer.Controls;
using ExViewer.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Microsoft.Toolkit.Uwp.UI.Animations.Expressions;
using Opportunity.MvvmUniverse.Views;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class GalleryPage : MvvmPage, IHasAppBar
    {
        public GalleryPage()
        {
            InitializeComponent();
            RegisterPropertyChangedCallback(VisibleBoundsProperty, OnVisibleBoundsPropertyChanged);
        }

        private void OnVisibleBoundsPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            spContent.InvalidateMeasure();
        }

        private void spContent_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var prop = e.GetCurrentPoint(this).Properties;
            if (!prop.IsHorizontalMouseWheel && prop.MouseWheelDelta != 0)
            {
                changeViewTo(prop.MouseWheelDelta < 0, false);
                e.Handled = true;
            }
        }

        private Compositor compositor;
        private Visual spVisual;
        private Visual btnScrollVisual;
        private InteractionTracker tracker;
        private VisualInteractionSource interactionSource;
        private CompositionPropertySet propertySet;

        private void page_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= page_Loaded;

            spVisual = ElementCompositionPreview.GetElementVisual(spContent);
            btnScrollVisual = ElementCompositionPreview.GetElementVisual(btn_Scroll);
            compositor = spVisual.Compositor;
            tracker = InteractionTracker.Create(compositor);
            var tref = tracker.GetReference();

            var trackerTarget = ExpressionValues.Target.CreateInteractionTrackerTarget();
            var endpoint1 = InteractionTrackerInertiaRestingValue.Create(compositor);
            endpoint1.SetCondition(trackerTarget.NaturalRestingPosition.Y < (trackerTarget.MaxPosition.Y - trackerTarget.MinPosition.Y) / 2);
            endpoint1.SetRestingValue(trackerTarget.MinPosition.Y);
            var endpoint2 = InteractionTrackerInertiaRestingValue.Create(compositor);
            endpoint2.SetCondition(trackerTarget.NaturalRestingPosition.Y >= (trackerTarget.MaxPosition.Y - trackerTarget.MinPosition.Y) / 2);
            endpoint2.SetRestingValue(trackerTarget.MaxPosition.Y);
            tracker.ConfigurePositionYInertiaModifiers(new InteractionTrackerInertiaModifier[] { endpoint1, endpoint2 });

            interactionSource = VisualInteractionSource.Create(spVisual);
            interactionSource.PositionYSourceMode = InteractionSourceMode.EnabledWithInertia;
            tracker.InteractionSources.Add(interactionSource);
            propertySet = compositor.CreatePropertySet();
            propertySet.InsertScalar("progress", 0.0f);
            propertySet.StartAnimation("progress", tref.Position.Y / tref.MaxPosition.Y);
            var progress = propertySet.GetReference().GetScalarProperty("progress");
            btnScrollVisual.StartAnimation("RotationAngleInDegrees", ExpressionFunctions.Clamp(progress, 0f, 1f) * 180);
            btnScrollVisual.CenterPoint = new System.Numerics.Vector3((float)btn_Scroll.ActualWidth / 2, (float)btn_Scroll.ActualHeight / 2, 0);
            spVisual.StartAnimation("Offset.Y", -ExpressionFunctions.Clamp(progress, -0.4f, 1.4f) * tref.MaxPosition.Y);
            gdInfo_SizeChanged(this, null);
        }

        private void pv_Content_Loaded(object sender, RoutedEventArgs e)
        {
            var feContent = (FrameworkElement)sender;
            var svContent = feContent.Descendants<ScrollViewer>().First();
            svContent.ViewChanging += pv_Content_ViewChanging;
            feContent.Loaded -= pv_Content_Loaded;
        }

        private int sizeChanging;
        private async void gdInfo_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var state = isGdInfoHide;
            if (tracker != null)
                tracker.MaxPosition = new System.Numerics.Vector3(0, (float)gdInfo.ActualHeight, 0);
            sizeChanging++;
            await Dispatcher.Yield(CoreDispatcherPriority.Low);
            sizeChanging--;
            if (sizeChanging != 0)
                return;
            changeViewTo(state, true);
        }

        private void spContent_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch && interactionSource != null)
            {
                try
                {
                    interactionSource.TryRedirectForManipulation(e.GetCurrentPoint(spContent));
                }
                catch (UnauthorizedAccessException) { }
                isGdInfoHideDef = null;
            }
        }

        private void pv_Content_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if (e.NextView.VerticalOffset < e.FinalView.VerticalOffset && !isGdInfoHide)
            {
                changeViewTo(true, false);
            }
            else if (e.IsInertial && e.NextView.VerticalOffset < 1 && isGdInfoHide)
            {
                changeViewTo(false, false);
            }
        }

        private void spContent_GotFocus(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Control ele && ele.FocusState == FocusState.Keyboard)
            {
                foreach (var item in ele.Ancestors<ListViewBase>())
                {
                    if (item == gv || item == lv_Comments || item == lv_Torrents)
                    {
                        changeViewTo(true, false);
                        return;
                    }
                }
                var trans = spContent.TransformToVisual(ele).TransformPoint(new Point(0, gdInfo.ActualHeight));
                if (trans.Y > 0)
                {
                    changeViewTo(false, false);
                }
                else if (trans.Y < -68)
                {
                    changeViewTo(true, false);
                }
            }
        }

        private bool? isGdInfoHideDef = false;
        private bool isGdInfoHide
        {
            get
            {
                if (isGdInfoHideDef is bool va)
                {
                    return va;
                }
                if (tracker is null)
                {
                    return false;
                }
                var current = tracker.Position.Y;
                var max = tracker.MaxPosition.Y;
                return current > max / 2;
            }
        }

        private void changeViewTo(bool hideGdInfo, bool disableAnimation)
        {
            if (isGdInfoHide == hideGdInfo && !disableAnimation)
            {
                return;
            }
            if (tracker is null)
            {
                return;
            }
            isGdInfoHideDef = hideGdInfo;
            if (disableAnimation)
            {
                tracker.TryUpdatePosition(hideGdInfo ? tracker.MaxPosition : tracker.MinPosition);
            }
            else
            {
                var ani = compositor.CreateVector3KeyFrameAnimation();
                ani.InsertKeyFrame(1, hideGdInfo ? tracker.MaxPosition : tracker.MinPosition);
                tracker.TryUpdatePositionWithAnimation(ani);
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.Assert(e.Parameter != null, "e.Parameter != null");
            base.OnNavigatedTo(e);
            var reset = e.NavigationMode == NavigationMode.New;
            var restore = e.NavigationMode == NavigationMode.Back;
            ViewModel = GalleryVM.GetVM((long)e.Parameter);
            var idx = ViewModel.View.CurrentPosition;
            ViewModel.View.IsCurrentPositionLocked = false;
            if (reset)
            {
                changeViewTo(false, true);

                gv.ScrollIntoView(ViewModel.Gallery.First());

                if (ViewModel.Gallery.Comments.IsLoaded)
                {
                    lv_Comments.ScrollIntoView(lv_Comments.Items.FirstOrDefault());
                }
                else
                {
                    void handler(object s, System.ComponentModel.PropertyChangedEventArgs args)
                    {
                        var sender = (ExClient.Galleries.Commenting.CommentCollection)s;
                        if (string.IsNullOrEmpty(args.PropertyName) || args.PropertyName == nameof(sender.IsLoaded))
                        {
                            sender.PropertyChanged -= handler;
                            if (!sender.IsEmpty)
                            {
                                lv_Comments.ScrollIntoView(sender[0]);
                            }
                        }
                    }
                    ViewModel.Gallery.Comments.PropertyChanged += handler;
                }

                lv_Torrents.ScrollIntoView(lv_Torrents.Items.FirstOrDefault());

                await Task.Delay(33);

                pv.Focus(FocusState.Programmatic);
                pv.SelectedIndex = 0;
            }
            else if (restore)
            {
                var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("ImageAnimation");
                changeViewTo(true, true);
                if (ViewModel.View.CurrentItem is null)
                {
                    ViewModel.View.MoveCurrentToFirst();
                    idx = 0;
                }
                gv.ScrollIntoView(ViewModel.View.CurrentItem, ScrollIntoViewAlignment.Leading);
                await Dispatcher.Yield(CoreDispatcherPriority.Low);
                var container = (Control)gv.ContainerFromIndex(idx);
                if (container != null && pv.SelectedIndex == 0)
                {
                    container.Focus(FocusState.Programmatic);
                }
                if (animation != null)
                {
                    if (pv.SelectedIndex == 0 && container != null)
                    {
                        await gv.TryStartConnectedAnimationAsync(animation, ViewModel.View.CurrentItem, "Image");
                    }
                    else
                    {
                        animation.Cancel();
                    }
                }
                await Dispatcher.YieldIdle();
                container = (Control)gv.ContainerFromIndex(idx);
                if (container != null && pv.SelectedIndex == 0)
                {
                    container.Focus(FocusState.Programmatic);
                }
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            ViewModel.View.IsCurrentPositionLocked = true;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (e.SourcePageType == typeof(ImagePage))
            {
                changeViewTo(true, true);
            }
        }

        private void gv_ItemClick(object sender, ItemClickEventArgs e)
        {
            gv.PrepareConnectedAnimation("ImageAnimation", e.ClickedItem, "Image");
            ViewModel.OpenImage.Execute((GalleryImage)e.ClickedItem);
        }

        public new GalleryVM ViewModel
        {
            get => (GalleryVM)base.ViewModel;
            set => base.ViewModel = value;
        }

        private void btn_Scroll_Click(object sender, RoutedEventArgs e)
        {
            changeViewTo(!isGdInfoHide, false);
        }

        private async void pv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (pv.SelectedIndex)
            {
            case 1://Comments
                if (!ViewModel.Gallery.Comments.IsLoaded)
                {
                    await ViewModel.LoadComments();
                }
                break;
            case 2://Torrents
                if (ViewModel.Torrents is null)
                {
                    await ViewModel.LoadTorrents();
                }
                // finish the add animation
                await Task.Delay(150);
                if (!lv_Torrents.Items.IsNullOrEmpty())
                {
                    lv_Torrents.SelectedIndex = -1;
                    lv_Torrents.SelectedIndex = 0;
                }
                break;
            }
        }

        public void ChangePivotSelection(int index)
        {
            pv.SelectedIndex = index;
        }

        private void lv_Torrents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in lv_Torrents.Descendants<ListViewItem>())
            {
                setDetailVisibility(item, Visibility.Collapsed);
            }
            var con = (ListViewItem)lv_Torrents.ContainerFromIndex(lv_Torrents.SelectedIndex);
            if (con is null)
                return;
            setDetailVisibility(con, Visibility.Visible);

            void setDetailVisibility(ListViewItem item, Visibility visibility)
            {
                var gd = (FrameworkElement)((FrameworkElement)item.ContentTemplateRoot).FindName("gd_TorrentDetail");
                gd.Visibility = visibility;
            }
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = true;
            switch (e.Key)
            {
            case VirtualKey.GamepadY:
                if (cb_top.IsOpen)
                {
                    e.Handled = false;
                    break;
                }
                changeViewTo(false, false);
                tpTags.Focus(FocusState.Keyboard);
                break;
            case VirtualKey.GamepadX:
                if (cb_top.IsOpen)
                {
                    e.Handled = false;
                    break;
                }
                changeViewTo(true, false);
                if ((pv.SelectedItem as PivotItem)?.Content is ListViewBase lvb)
                {
                    if (lvb.Items.Count == 0)
                    {
                        pv.Focus(FocusState.Keyboard);
                    }
                    else if (lvb.ContainerFromIndex(lvb.SelectedIndex) is Control c)
                    {
                        c.Focus(FocusState.Keyboard);
                    }
                    else
                    {
                        lvb.Focus(FocusState.Keyboard);
                    }
                }
                else
                {
                    pv.Focus(FocusState.Keyboard);
                }
                break;
            case VirtualKey.GamepadMenu:
            case VirtualKey.Application:
                cb_top.IsOpen = !cb_top.IsOpen;
                if (cb_top.IsOpen && (btnMoreButton is null))
                {
                    btnMoreButton = cb_top.Descendants<Button>("MoreButton").FirstOrDefault();
                    btnMoreButton?.Focus(FocusState.Programmatic);
                }
                break;
            default:
                e.Handled = false;
                break;
            }
        }

        private Button btnMoreButton;
        public void CloseAll()
        {
            cb_top.IsOpen = false;
        }

        private void page_Loading(FrameworkElement sender, object args)
        {
            SetSplitViewButtonPlaceholderVisibility(null, RootControl.RootController.SplitViewButtonPlaceholderVisibility);
            RootControl.RootController.SplitViewButtonPlaceholderVisibilityChanged += SetSplitViewButtonPlaceholderVisibility;
        }

        private void page_Unloaded(object sender, RoutedEventArgs e)
        {
            RootControl.RootController.SplitViewButtonPlaceholderVisibilityChanged -= SetSplitViewButtonPlaceholderVisibility;
        }

        private void cb_top_Opening(object sender, object e)
        {
            tbGalleryName.MaxLines = 0;
            Grid.SetColumn(tbGalleryName, 0);
        }

        private void cb_top_Closed(object sender, object e)
        {
            tbGalleryName.ClearValue(TextBlock.MaxLinesProperty);
            Grid.SetColumn(tbGalleryName, 1);
        }

        private Thickness gdCbContentPadding(double minHeight)
        {
            var tb = (minHeight - 20) / 2;
            return new Thickness(0, tb, 0, tb);
        }

        public void SetSplitViewButtonPlaceholderVisibility(RootControl sender, bool visible)
        {
            if (visible)
            {
                cdSplitViewPlaceholder.Width = new GridLength(48);
            }
            else
            {
                cdSplitViewPlaceholder.Width = new GridLength(0);
            }
        }

        private static string favoriteCategoryToName(FavoriteCategory cat)
        {
            if (cat is null || cat.Index < 0)
            {
                return Strings.Resources.Views.GalleryPage.FavoritesAppBarButton.Text;
            }
            return cat.Name;
        }
        private static Visibility operationStateToVisibility(OperationState value)
            => value == OperationState.NotStarted ? Visibility.Collapsed : Visibility.Visible;

        private static Brush operationStateToBrush(OperationState value)
        {
            switch (value)
            {
            case OperationState.NotStarted:
                return opNotStarted;
            case OperationState.Started:
                return opStarted;
            case OperationState.Failed:
                return opFailed;
            case OperationState.Completed:
                return opCompleted;
            default:
                return null;
            }
        }

        // for function binding with bindback
        private static Score score(Score score) => score;

        private static readonly Brush opNotStarted = new SolidColorBrush(Colors.Transparent);
        private static readonly Brush opStarted = (Brush)Application.Current.Resources["SystemControlHighlightAccentBrush"];
        private static readonly Brush opFailed = new SolidColorBrush(Colors.Red);
        private static readonly Brush opCompleted = new SolidColorBrush(Colors.Green);
    }

    internal sealed class GalleryPagePanel : Panel
    {
        private Grid _GdPvContentHeaderPresenter;

        protected override Size MeasureOverride(Size availableSize)
        {
            var height = availableSize.Height;
            var width = availableSize.Width;
            var gp = this.Ancestors<GalleryPage>().FirstOrDefault();
            var infoH = height - (gp is null ? 0 : gp.VisibleBounds.Bottom);
            if (InputPane.GetForCurrentView().OccludedRect.Height == 0)
            {
                if (_GdPvContentHeaderPresenter is null)
                {
                    _GdPvContentHeaderPresenter = Children[1].Descendants<Grid>("HeaderPresenter").FirstOrDefault();
                }
                if (_GdPvContentHeaderPresenter != null)
                {
                    infoH -= _GdPvContentHeaderPresenter.ActualHeight + 24/*this.btn_Scroll.ActualHeight*/;
                }
            }

            infoH = Math.Max(80, Math.Min(infoH, 360));
            Children[0].Measure(new Size(width, infoH));
            Children[1].Measure(availableSize);
            return new Size(width, Children[0].DesiredSize.Height + height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var width = finalSize.Width;
            var height = finalSize.Height;
            var infoH = Children[0].DesiredSize.Height;
            Children[0].Arrange(new Rect(0, 0, width, infoH));
            Children[1].Arrange(new Rect(0, infoH, width, finalSize.Height - infoH));
            return finalSize;
        }
    }
}
