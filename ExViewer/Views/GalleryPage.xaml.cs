using ExClient;
using ExClient.Galleries;
using ExViewer.Controls;
using ExViewer.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Microsoft.Toolkit.Uwp.UI.Animations.Expressions;
using Opportunity.MvvmUniverse.Views;
using System;
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
            this.InitializeComponent();
            this.RegisterPropertyChangedCallback(VisibleBoundsProperty, (s, e) => ((GalleryPage)s).InvalidateMeasure());
        }

        private void spContent_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var prop = e.GetCurrentPoint(this).Properties;
            if (!prop.IsHorizontalMouseWheel && prop.MouseWheelDelta != 0)
            {
                this.changeViewTo(prop.MouseWheelDelta < 0, false);
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
            this.Loaded -= this.page_Loaded;

            this.spVisual = ElementCompositionPreview.GetElementVisual(this.spContent);
            this.btnScrollVisual = ElementCompositionPreview.GetElementVisual(this.btn_Scroll);
            this.compositor = this.spVisual.Compositor;
            this.tracker = InteractionTracker.Create(this.compositor);
            var tref = this.tracker.GetReference();

            var trackerTarget = ExpressionValues.Target.CreateInteractionTrackerTarget();
            var endpoint1 = InteractionTrackerInertiaRestingValue.Create(this.compositor);
            endpoint1.SetCondition(trackerTarget.NaturalRestingPosition.Y < (trackerTarget.MaxPosition.Y - trackerTarget.MinPosition.Y) / 2);
            endpoint1.SetRestingValue(trackerTarget.MinPosition.Y);
            var endpoint2 = InteractionTrackerInertiaRestingValue.Create(this.compositor);
            endpoint2.SetCondition(trackerTarget.NaturalRestingPosition.Y >= (trackerTarget.MaxPosition.Y - trackerTarget.MinPosition.Y) / 2);
            endpoint2.SetRestingValue(trackerTarget.MaxPosition.Y);
            this.tracker.ConfigurePositionYInertiaModifiers(new InteractionTrackerInertiaModifier[] { endpoint1, endpoint2 });

            this.interactionSource = VisualInteractionSource.Create(this.spVisual);
            this.interactionSource.PositionYSourceMode = InteractionSourceMode.EnabledWithInertia;
            this.tracker.InteractionSources.Add(this.interactionSource);
            this.propertySet = this.compositor.CreatePropertySet();
            this.propertySet.InsertScalar("progress", 0.0f);
            this.propertySet.StartAnimation("progress", tref.Position.Y / tref.MaxPosition.Y);
            var progress = this.propertySet.GetReference().GetScalarProperty("progress");
            this.btnScrollVisual.StartAnimation("RotationAngleInDegrees", ExpressionFunctions.Clamp(progress, 0f, 1f) * 180);
            this.btnScrollVisual.CenterPoint = new System.Numerics.Vector3((float)this.btn_Scroll.ActualWidth / 2, (float)this.btn_Scroll.ActualHeight / 2, 0);
            this.spVisual.StartAnimation("Offset.Y", -ExpressionFunctions.Clamp(progress, -0.4f, 1.4f) * tref.MaxPosition.Y);
            gdInfo_SizeChanged(this, null);
        }

        private void pv_Content_Loaded(object sender, RoutedEventArgs e)
        {
            var fe_Content = (FrameworkElement)sender;
            var sv_Content = fe_Content.Descendants<ScrollViewer>().First();
            sv_Content.ViewChanging += this.pv_Content_ViewChanging;
            fe_Content.Loaded -= this.pv_Content_Loaded;
        }

        private int sizeChanging;
        private async void gdInfo_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.sizeChanging++;
            await Dispatcher.YieldIdle();
            this.sizeChanging--;
            if (this.sizeChanging == 0)
            {
                var state = this.isGdInfoHide;
                if (this.tracker != null)
                    this.tracker.MaxPosition = new System.Numerics.Vector3(0, (float)this.gdInfo.ActualHeight, 0);
                changeViewTo(state, true);
            }
        }

        private void spContent_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                this.interactionSource.TryRedirectForManipulation(e.GetCurrentPoint(this.spContent));
                this.isGdInfoHideDef = null;
            }
        }

        private void pv_Content_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if (e.NextView.VerticalOffset < e.FinalView.VerticalOffset && !this.isGdInfoHide)
            {
                changeViewTo(true, false);
            }
            else if (e.IsInertial && e.NextView.VerticalOffset < 1 && this.isGdInfoHide)
            {
                changeViewTo(false, false);
            }
        }

        private void spContent_GotFocus(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Control ele && ele.FocusState == FocusState.Keyboard)
            {
                var trans = this.spContent.TransformToVisual(ele).TransformPoint(new Point(0, this.gdInfo.ActualHeight));
                if (trans.Y > 0)
                    changeViewTo(false, false);
                else if (trans.Y < -68)
                    changeViewTo(true, false);
            }
        }

        private Grid gdPvContentHeaderPresenter;

        protected override Size MeasureOverride(Size availableSize)
        {
            var t = VisibleBounds;
            var height = availableSize.Height - 48 - t.Top;
            var infoH = height - t.Bottom;
            if (InputPane.GetForCurrentView().OccludedRect.Height == 0)
            {
                if (this.gdPvContentHeaderPresenter == null)
                    this.gdPvContentHeaderPresenter = this.pv.Descendants<Grid>("HeaderPresenter").FirstOrDefault();
                if (this.gdPvContentHeaderPresenter != null)
                    infoH -= 68/*this.gdPvContentHeaderPresenter.ActualHeight*/ + 24;
            }
            this.gdInfo.MaxHeight = Math.Min(infoH, 360);
            this.gd_Pivot.Height = height;
            return base.MeasureOverride(availableSize);
        }

        private bool? isGdInfoHideDef = false;
        private bool isGdInfoHide
        {
            get
            {
                if (this.isGdInfoHideDef is bool va)
                    return va;
                if (this.tracker == null)
                    return false;
                var current = this.tracker.Position.Y;
                var max = this.tracker.MaxPosition.Y;
                return current > max / 2;
            }
        }

        private void changeViewTo(bool hideGdInfo, bool disableAnimation)
        {
            if (this.isGdInfoHide == hideGdInfo && !disableAnimation)
                return;
            if (this.tracker == null)
                return;
            this.isGdInfoHideDef = hideGdInfo;
            if (disableAnimation)
            {
                if (hideGdInfo)
                {
                    this.tracker.TryUpdatePosition(this.tracker.MaxPosition);
                }
                else
                {
                    this.tracker.TryUpdatePosition(this.tracker.MinPosition);
                }
            }
            else
            {
                var ani = this.compositor.CreateVector3KeyFrameAnimation();
                if (hideGdInfo)
                {
                    ani.InsertKeyFrame(1, this.tracker.MaxPosition);
                }
                else
                {
                    ani.InsertKeyFrame(1, this.tracker.MinPosition);
                }
                this.tracker.TryUpdatePositionWithAnimation(ani);
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.tpTags.IsTabStop = false;
            base.OnNavigatedTo(e);
            var reset = e.NavigationMode == NavigationMode.New;
            var restore = e.NavigationMode == NavigationMode.Back;
            this.ViewModel = GalleryVM.GetVM((long)e.Parameter);
            var idx = this.ViewModel.View.CurrentPosition;
            if (reset)
            {
                changeViewTo(false, true);
                this.gv.ScrollIntoView(this.ViewModel.Gallery.FirstOrDefault());
                this.lv_Comments.ScrollIntoView(this.lv_Comments.Items.FirstOrDefault());
                this.lv_Torrents.ScrollIntoView(this.lv_Torrents.Items.FirstOrDefault());
                await Task.Delay(33);
                this.pv.Focus(FocusState.Programmatic);
                this.pv.SelectedIndex = 0;
            }
            else if (restore)
            {
                changeViewTo(true, true);
                this.gv.ScrollIntoView(this.ViewModel.View.CurrentItem);
                var container = default(Control);
                var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation("ImageAnimation");
                if (animation != null)
                {
                    if (this.pv.SelectedIndex == 0)
                        await this.gv.TryStartConnectedAnimationAsync(animation, this.ViewModel.View.CurrentItem, "Image");
                    else
                        animation.Cancel();
                }
                await Dispatcher.YieldIdle();
                if (container == null)
                    container = (Control)this.gv.ContainerFromIndex(idx);
                if (container != null && this.pv.SelectedIndex == 0)
                {
                    container.Focus(FocusState.Programmatic);
                }
            }
            await Dispatcher.YieldIdle();
            this.tpTags.IsTabStop = true;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (e.SourcePageType == typeof(ImagePage))
                changeViewTo(true, true);
        }

        private void gv_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.gv.PrepareConnectedAnimation("ImageAnimation", e.ClickedItem, "Image");
            this.ViewModel.OpenImage.Execute((GalleryImage)e.ClickedItem);
        }

        public new GalleryVM ViewModel
        {
            get => (GalleryVM)base.ViewModel;
            set => base.ViewModel = value;
        }

        private void btn_Scroll_Click(object sender, RoutedEventArgs e)
        {
            changeViewTo(!this.isGdInfoHide, false);
        }

        private async void pv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (this.pv.SelectedIndex)
            {
            case 1://Comments
                if (!this.ViewModel.Gallery.Comments.IsLoaded)
                    await this.ViewModel.LoadComments();
                break;
            case 2://Torrents
                if (this.ViewModel.Torrents == null)
                    await this.ViewModel.LoadTorrents();
                // finish the add animation
                await Task.Delay(150);
                if (this.lv_Torrents.Items.Count > 0)
                {
                    this.lv_Torrents.SelectedIndex = -1;
                    this.lv_Torrents.SelectedIndex = 0;
                }
                break;
            }
        }

        public void ChangePivotSelection(int index)
        {
            this.pv.SelectedIndex = index;
        }

        private void lv_Torrents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.RemovedItems)
            {
                var con = (ListViewItem)this.lv_Torrents.ContainerFromItem(item);
                if (con == null)
                    continue;
                var gd = (FrameworkElement)((FrameworkElement)con.ContentTemplateRoot).FindName("gd_TorrentDetail");
                gd.Visibility = Visibility.Collapsed;
            }
            var added = e.AddedItems.FirstOrDefault();
            if (added != null)
            {
                var con = (ListViewItem)this.lv_Torrents.ContainerFromItem(added);
                if (con == null)
                    return;
                var gd = (FrameworkElement)((FrameworkElement)con.ContentTemplateRoot).FindName("gd_TorrentDetail");
                gd.Visibility = Visibility.Visible;
            }
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = true;
            switch (e.Key)
            {
            case VirtualKey.GamepadY:
                if (this.cb_top.IsOpen)
                {
                    e.Handled = false;
                    break;
                }
                this.changeViewTo(false, false);
                this.tpTags.Focus(FocusState.Keyboard);
                break;
            case VirtualKey.GamepadX:
                if (this.cb_top.IsOpen)
                {
                    e.Handled = false;
                    break;
                }
                this.changeViewTo(true, false);
                if ((this.pv.SelectedItem as PivotItem)?.Content is ListViewBase lvb)
                {
                    if (lvb.Items.Count == 0)
                        this.pv.Focus(FocusState.Keyboard);
                    else if (lvb.ContainerFromIndex(lvb.SelectedIndex) is Control c)
                        c.Focus(FocusState.Keyboard);
                    else
                        lvb.Focus(FocusState.Keyboard);
                }
                else
                    this.pv.Focus(FocusState.Keyboard);
                break;
            case VirtualKey.GamepadMenu:
            case VirtualKey.Application:
                if (this.cb_top.IsOpen = !this.cb_top.IsOpen)
                {
                    if (this.btn_MoreButton == null)
                        this.btn_MoreButton = this.cb_top.Descendants<Button>("MoreButton").FirstOrDefault();
                    this.btn_MoreButton?.Focus(FocusState.Programmatic);
                }
                break;
            default:
                e.Handled = false;
                break;
            }
        }

        private Button btn_MoreButton;
        public void CloseAll()
        {
            this.cb_top.IsOpen = false;
        }

        private void page_Loading(FrameworkElement sender, object args)
        {
            this.SetSplitViewButtonPlaceholderVisibility(null, RootControl.RootController.SplitViewButtonPlaceholderVisibility);
            RootControl.RootController.SplitViewButtonPlaceholderVisibilityChanged += this.SetSplitViewButtonPlaceholderVisibility;
        }

        private void page_Unloaded(object sender, RoutedEventArgs e)
        {
            RootControl.RootController.SplitViewButtonPlaceholderVisibilityChanged -= this.SetSplitViewButtonPlaceholderVisibility;
        }

        private AddToFavoritesDialog addToFavorites;

        private async void abbFavorites_Click(object sender, RoutedEventArgs e)
        {
            var addToFavorites = System.Threading.LazyInitializer.EnsureInitialized(ref this.addToFavorites);
            addToFavorites.Gallery = this.ViewModel.Gallery;
            await addToFavorites.ShowAsync();
        }

        private void cb_top_Opening(object sender, object e)
        {
            this.tbGalleryName.MaxLines = 0;
            Grid.SetColumn(this.tbGalleryName, 0);
        }

        private void cb_top_Closed(object sender, object e)
        {
            this.tbGalleryName.ClearValue(TextBlock.MaxLinesProperty);
            Grid.SetColumn(this.tbGalleryName, 1);
        }

        public void SetSplitViewButtonPlaceholderVisibility(RootControl sender, bool visible)
        {
            if (visible)
                this.cdSplitViewPlaceholder.Width = new GridLength(48);
            else
                this.cdSplitViewPlaceholder.Width = new GridLength(0);
        }

        private static string favoriteCategoryToName(FavoriteCategory cat)
        {
            if (cat == null || cat.Index < 0)
                return Strings.Resources.Views.GalleryPage.FavoritesAppBarButton.Text;
            return cat.Name;
        }
        private static Visibility operationStateToVisibility(OperationState value)
        {
            if (value == OperationState.NotStarted)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }
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
            }
            return null;
        }

        private static readonly Brush opNotStarted = new SolidColorBrush(Colors.Transparent);
        private static readonly Brush opStarted = (Brush)Application.Current.Resources["SystemControlHighlightAccentBrush"];
        private static readonly Brush opFailed = new SolidColorBrush(Colors.Red);
        private static readonly Brush opCompleted = new SolidColorBrush(Colors.Green);
    }
}
