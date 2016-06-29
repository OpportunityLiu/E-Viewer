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
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(WrapStackPanel), new PropertyMetadata(Orientation.Horizontal));

        protected override Size MeasureOverride(Size availableSize)
        {
            var childAvailableSize = new Size(availableSize.Width, double.PositiveInfinity);
            var desireSize = new Size();
            var currentLineWidth = 0d;
            var currentLineHeight = 0d;
            foreach(var item in this.Children)
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

        protected override Size ArrangeOverride(Size finalSize)
        {
            var usedSize = new Size();
            var currentLineWidth = 0d;
            var currentLineHeight = 0d;
            var currentOffset = 0d;
            var elementOfCurrentLine = new List<UIElement>();
            foreach(var item in this.Children)
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
    }
}
