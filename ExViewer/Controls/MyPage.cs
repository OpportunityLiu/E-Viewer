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
        private static List<WeakReference<MyPage>> instances = new List<WeakReference<MyPage>>();

        static MyPage()
        {
            RootControl.RootController.Parent.RegisterPropertyChangedCallback(RootControl.VisibleBoundsThicknessProperty, VisibleBoundsThicknessPropertyChangedCallback);
        }

        private static void VisibleBoundsThicknessPropertyChangedCallback(DependencyObject d, DependencyProperty p)
        {
            for(var i = 0; i < instances.Count;)
            {
                if(instances[i].TryGetTarget(out var item))
                {
                    item.SetValue(VisibleBoundsThicknessProperty, RootControl.RootController.Parent.ContentVisibleBoundsThickness);
                    i++;
                }
                else
                {
                    instances.RemoveAt(i);
                }
            }
        }

        public MyPage()
        {
            this.SetValue(VisibleBoundsThicknessProperty, RootControl.RootController.Parent.ContentVisibleBoundsThickness);
            instances.Add(new WeakReference<MyPage>(this));
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

        protected override Size ArrangeOverride(Size finalSize)
        {
            if(VisibleBoundHandledByDesign)
            {
                var c = this.Content;
                if(c != null)
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
            if(VisibleBoundHandledByDesign)
            {
                var c = this.Content;
                if(c != null)
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
