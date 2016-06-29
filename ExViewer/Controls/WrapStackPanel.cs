using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ExViewer.Controls
{
    public class WrapStackPanel : Panel
    {
        public Orientation Orientation
        {
            get
            {
                return (Orientation)GetValue(OrientationProperty);
            }
            set
            {
                SetValue(OrientationProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(WrapStackPanel), new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

        private static void OnOrientationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((UIElement)sender).InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return LayoutHelper.MeasureOverride(this, availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return LayoutHelper.ArrangeOverride(this, finalSize);
        }

        private static class LayoutHelper
        {
            public static Size MeasureOverride(WrapStackPanel that, Size availableSize)
            {
                if(that.Orientation == Orientation.Vertical)
                    return MeasureForVertical(that, ref availableSize);
                else
                    return MeasureForHorizotal(that, ref availableSize);
            }

            private static Size MeasureForHorizotal(WrapStackPanel that, ref Size availableSize)
            {
                var childAvailableSize = new Size(availableSize.Width, double.PositiveInfinity);
                var desireSize = new Size();
                var currentLineWidth = 0d;
                var currentLineHeight = 0d;
                foreach(var item in that.Children)
                {
                    item.Measure(childAvailableSize);
                    var currentLineDesireWidth = currentLineWidth + item.DesiredSize.Width;
                    if(currentLineWidth == 0 || currentLineDesireWidth <= availableSize.Width)
                    {
                        //布局至当前行
                        currentLineWidth = currentLineDesireWidth;
                        currentLineHeight = Math.Max(currentLineHeight, item.DesiredSize.Height);
                    }
                    else
                    {
                        //布局至下一行
                        desireSize.Height += currentLineHeight;
                        desireSize.Width = Math.Max(desireSize.Width, currentLineWidth);

                        currentLineHeight = item.DesiredSize.Height;
                        currentLineWidth = item.DesiredSize.Width;
                    }
                }
                desireSize.Height += currentLineHeight;
                desireSize.Width = Math.Max(desireSize.Width, currentLineWidth);
                return desireSize;
            }

            private static Size MeasureForVertical(WrapStackPanel that, ref Size availableSize)
            {
                var childAvailableSize = new Size(double.PositiveInfinity, availableSize.Height);
                var desireSize = new Size();
                var currentLineHeight = 0d;
                var currentLineWidth = 0d;
                foreach(var item in that.Children)
                {
                    item.Measure(childAvailableSize);
                    var currentLineDesireHeight = currentLineHeight + item.DesiredSize.Height;
                    if(currentLineHeight == 0 || currentLineDesireHeight <= availableSize.Height)
                    {
                        //布局至当前列
                        currentLineHeight = currentLineDesireHeight;
                        currentLineWidth = Math.Max(currentLineWidth, item.DesiredSize.Width);
                    }
                    else
                    {
                        //布局至下一行列
                        desireSize.Width += currentLineWidth;
                        desireSize.Height = Math.Max(desireSize.Height, currentLineHeight);

                        currentLineWidth = item.DesiredSize.Width;
                        currentLineHeight = item.DesiredSize.Height;
                    }
                }
                desireSize.Width += currentLineWidth;
                desireSize.Height = Math.Max(desireSize.Height, currentLineHeight);
                return desireSize;
            }

            public static Size ArrangeOverride(WrapStackPanel that, Size finalSize)
            {
                if(that.Orientation == Orientation.Vertical)
                    return ArrangeForVertical(that, finalSize);
                else
                    return ArrangeForHorizotal(that, finalSize);
            }

            private static Size ArrangeForHorizotal(WrapStackPanel that, Size finalSize)
            {
                var usedSize = new Size();
                var currentLineWidth = 0d;
                var currentLineHeight = 0d;
                var currentOffset = 0d;
                var elementOfCurrentLine = new List<UIElement>();
                foreach(var item in that.Children)
                {
                    var currentLineDesireWidth = currentLineWidth + item.DesiredSize.Width;
                    if(currentLineWidth == 0 || currentLineDesireWidth - finalSize.Width < 0.0001)
                    {
                        //布局至当前行
                        currentLineWidth = currentLineDesireWidth;
                        currentLineHeight = Math.Max(currentLineHeight, item.DesiredSize.Height);
                        elementOfCurrentLine.Add(item);
                    }
                    else
                    {
                        //布局至下一行
                        currentOffset = 0d;
                        foreach(var element in elementOfCurrentLine)
                        {
                            element.Arrange(new Rect(currentOffset, usedSize.Height, element.DesiredSize.Width, currentLineHeight));
                            currentOffset += element.DesiredSize.Width;
                        }
                        usedSize.Height += currentLineHeight;
                        usedSize.Width = Math.Max(usedSize.Width, currentLineWidth);

                        elementOfCurrentLine.Clear();
                        currentLineHeight = item.DesiredSize.Height;
                        currentLineWidth = item.DesiredSize.Width;
                        elementOfCurrentLine.Add(item);
                    }
                }
                currentOffset = 0d;
                foreach(var element in elementOfCurrentLine)
                {
                    element.Arrange(new Rect(currentOffset, usedSize.Height, element.DesiredSize.Width, currentLineHeight));
                    currentOffset += element.DesiredSize.Width;
                }
                usedSize.Height += currentLineHeight;
                usedSize.Width = Math.Max(usedSize.Width, currentLineWidth);
                return usedSize;
            }

            private static Size ArrangeForVertical(WrapStackPanel that, Size finalSize)
            {
                var usedSize = new Size();
                var currentLineHeight = 0d;
                var currentLineWidth = 0d;
                var currentOffset = 0d;
                var elementOfCurrentLine = new List<UIElement>();
                foreach(var item in that.Children)
                {
                    var currentLineDesireHeight = currentLineHeight + item.DesiredSize.Height;
                    if(currentLineHeight == 0 || currentLineDesireHeight - finalSize.Height < 0.0001)
                    {
                        //布局至当前列
                        currentLineHeight = currentLineDesireHeight;
                        currentLineWidth = Math.Max(currentLineWidth, item.DesiredSize.Width);
                        elementOfCurrentLine.Add(item);
                    }
                    else
                    {
                        //布局至下一列
                        currentOffset = 0d;
                        foreach(var element in elementOfCurrentLine)
                        {
                            element.Arrange(new Rect(usedSize.Width, currentOffset, currentLineWidth, element.DesiredSize.Height));
                            currentOffset += element.DesiredSize.Height;
                        }
                        usedSize.Width += currentLineWidth;
                        usedSize.Height = Math.Max(usedSize.Height, currentLineHeight);

                        elementOfCurrentLine.Clear();
                        currentLineWidth = item.DesiredSize.Width;
                        currentLineHeight = item.DesiredSize.Height;
                        elementOfCurrentLine.Add(item);
                    }
                }
                currentOffset = 0d;
                foreach(var element in elementOfCurrentLine)
                {
                    element.Arrange(new Rect(usedSize.Width, currentOffset, currentLineWidth, element.DesiredSize.Height));
                    currentOffset += element.DesiredSize.Height;
                }
                usedSize.Width += currentLineWidth;
                usedSize.Height = Math.Max(usedSize.Height, currentLineHeight);
                return usedSize;
            }
        }
    }
}
