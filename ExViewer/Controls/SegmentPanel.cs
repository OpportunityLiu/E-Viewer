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
    public class SegmentPanel : Panel
    {
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// Indentify <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(SegmentPanel), new PropertyMetadata(Orientation.Horizontal, OrientationPropertyChanged));

        private static void OrientationPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (Orientation)e.OldValue;
            var newValue = (Orientation)e.NewValue;
            if (oldValue == newValue)
                return;
            var sender = (SegmentPanel)dp;
            sender.InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (Children.Count == 0)
                return base.MeasureOverride(availableSize);
            var size = Orientation == Orientation.Horizontal
                ? new Size(availableSize.Width / Children.Count, availableSize.Height)
                : new Size(availableSize.Width, availableSize.Height / Children.Count);
            var max = 0d;
            foreach (var item in Children)
            {
                item.Measure(size);
                var need = Orientation == Orientation.Horizontal
                    ? item.DesiredSize.Height
                    : item.DesiredSize.Width;
                max = Math.Max(max, need);
            }
            return Orientation == Orientation.Horizontal
                ? new Size(availableSize.Width, max)
                : new Size(max, availableSize.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count == 0)
                return base.ArrangeOverride(finalSize);
            var size = Orientation == Orientation.Horizontal
                ? new Size(finalSize.Width / Children.Count, finalSize.Height)
                : new Size(finalSize.Width, finalSize.Height / Children.Count);
            var offset = 0d;
            var step = Orientation == Orientation.Horizontal
                ? finalSize.Width / Children.Count
                : finalSize.Height / Children.Count;
            foreach (var item in Children)
            {
                var point = Orientation == Orientation.Horizontal
                    ? new Point(offset, 0)
                    : new Point(0, offset);
                item.Arrange(new Rect(point, size));
                offset += step;
            }
            return finalSize;
        }
    }
}
