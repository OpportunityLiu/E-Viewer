using ExViewer.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;

namespace ExViewer.Controls
{
    public class MyPage : Page
    {
        private void VisibleBoundsThicknessPropertyChangedCallback(DependencyObject d, DependencyProperty p)
        {
            var t = RootControl.RootController.Parent.ContentVisibleBoundsThickness;
            this.SetValue(VisibleBoundsThicknessProperty, t);
            this.VisibleBoundsThicknessChanged(t);
        }

        private long rootControlVisibleBoundsCallbackId;

        public MyPage()
        {
            this.Loading += this.MyPage_Loading;
            this.Unloaded += this.MyPage_Unloaded;
        }

        private void MyPage_Loading(FrameworkElement sender, object args)
        {
            this.VisibleBoundsThicknessPropertyChangedCallback(RootControl.RootController.Parent, RootControl.VisibleBoundsThicknessProperty);
            this.rootControlVisibleBoundsCallbackId = RootControl.RootController.Parent.RegisterPropertyChangedCallback(RootControl.VisibleBoundsThicknessProperty, VisibleBoundsThicknessPropertyChangedCallback);
        }

        private void MyPage_Unloaded(object sender, RoutedEventArgs e)
        {
            RootControl.RootController.Parent.UnregisterPropertyChangedCallback(RootControl.VisibleBoundsThicknessProperty, this.rootControlVisibleBoundsCallbackId);
        }

        public Thickness VisibleBoundsThickness
        {
            get => (Thickness)GetValue(VisibleBoundsThicknessProperty);
            set => SetValue(VisibleBoundsThicknessProperty, value);
        }

        public static readonly DependencyProperty VisibleBoundsThicknessProperty =
            DependencyProperty.Register(nameof(VisibleBoundsThickness), typeof(Thickness), typeof(MyPage), new PropertyMetadata(new Thickness()));

        public bool VisibleBoundHandledByDesign
        {
            get;
            protected set;
        }

        protected virtual void VisibleBoundsThicknessChanged(Thickness visibleBoundsThickness)
        {
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (VisibleBoundHandledByDesign)
            {
                var c = this.Content;
                if (c != null)
                {
                    c.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
                }
                return finalSize;
            }
            else
                return base.ArrangeOverride(finalSize);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (VisibleBoundHandledByDesign)
            {
                var c = this.Content;
                if (c != null)
                {
                    c.Measure(availableSize);
                    return c.DesiredSize;
                }
                else
                {
                    return new Size();
                }
            }
            else
                return base.MeasureOverride(availableSize);
        }
    }
}
