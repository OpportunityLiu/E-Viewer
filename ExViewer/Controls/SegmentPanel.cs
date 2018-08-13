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
            if (Children.Count == 0 || availableSize.Width <= 1e-5 || availableSize.Height <= 1e-5)
                return base.MeasureOverride(availableSize);
            var size = Orientation == Orientation.Horizontal
                ? new Size(0, availableSize.Height)
                : new Size(availableSize.Width, 0);
            var needs = Children.Select(c => { c.Measure(availableSize); return c.DesiredSize; }).ToList();
            var wm = needs.Max(s => s.Width);
            var hm = needs.Max(s => s.Height);
            if (Orientation == Orientation.Horizontal)
            {
                var wms = wm * needs.Count;
                if (wms > availableSize.Width)
                    return new Size(availableSize.Width, hm);
                return new Size(wm * needs.Count, hm);
            }
            else
            {
                var hms = hm * needs.Count;
                if (hms > availableSize.Height)
                    return new Size(wm, availableSize.Height);
                return new Size(wm, hm * needs.Count);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count == 0 || finalSize.Width <= 1e-5 || finalSize.Height <= 1e-5)
                return base.ArrangeOverride(finalSize);

            var needs = Children.Select(c => c.DesiredSize).ToList();
            if (Orientation == Orientation.Horizontal)
            {
                var allneed = needs.Sum(n => n.Width);
                if (allneed >= finalSize.Width)
                {
                    var hoffset = 0d;
                    foreach (var item in Children)
                    {
                        var w = item.DesiredSize.Width * finalSize.Width / allneed;
                        item.Arrange(new Rect(hoffset, 0, w, finalSize.Height));
                        hoffset += w;
                    }
                    return finalSize;
                }

                var widths = Enumerable.Repeat(finalSize.Width / needs.Count, needs.Count).ToList();
                var adjusted = Enumerable.Repeat(false, needs.Count).ToList();
                while (true)
                {
                    var allmeetrequire = true;
                    for (var i = 0; i < needs.Count; i++)
                    {
                        var s = needs[i];
                        var w = widths[i];
                        var diff = s.Width - w;
                        if (diff > 1e-5)
                        {
                            allmeetrequire = false;
                            adjusted[i] = true;
                            var ac = adjusted.Count(c => !c);
                            if (ac == 0)
                                break;
                            var ajdiff = diff / ac;
                            for (var j = 0; j < adjusted.Count; j++)
                            {
                                if (!adjusted[j])
                                {
                                    widths[j] -= ajdiff;
                                    widths[i] += ajdiff;
                                }
                            }
                            break;
                        }
                    }
                    if (allmeetrequire)
                        break;
                }
                {
                    var i = 0;
                    var hoffset = 0d;
                    foreach (var item in Children)
                    {
                        var w = widths[i];
                        item.Arrange(new Rect(hoffset, 0, w, finalSize.Height));
                        hoffset += w;
                        i++;
                    }
                    return finalSize;
                }
            }
            else
            {
                var allneed = needs.Sum(n => n.Height);
                if (allneed >= finalSize.Height)
                {
                    var voffset = 0d;
                    foreach (var item in Children)
                    {
                        var h = item.DesiredSize.Height * finalSize.Height / allneed;
                        item.Arrange(new Rect(0, voffset, finalSize.Width, h));
                        voffset += h;
                    }
                    return finalSize;
                }

                var heights = Enumerable.Repeat(finalSize.Height / needs.Count, needs.Count).ToList();
                var adjusted = Enumerable.Repeat(false, needs.Count).ToList();
                while (true)
                {
                    var allmeetrequire = true;
                    for (var i = 0; i < needs.Count; i++)
                    {
                        var s = needs[i];
                        var h = heights[i];
                        var diff = s.Height - h;
                        if (diff > 1e-5)
                        {
                            allmeetrequire = false;
                            adjusted[i] = true;
                            var ac = adjusted.Count(c => !c);
                            if (ac == 0)
                                break;
                            var ajdiff = diff / ac;
                            for (var j = 0; j < adjusted.Count; j++)
                            {
                                if (!adjusted[j])
                                {
                                    heights[j] -= ajdiff;
                                    heights[i] += ajdiff;
                                }
                            }
                            break;
                        }
                    }
                    if (allmeetrequire)
                        break;
                }
                {
                    var i = 0;
                    var voffset = 0d;
                    foreach (var item in Children)
                    {
                        var h = heights[i];
                        item.Arrange(new Rect(0, voffset, finalSize.Width, h));
                        voffset += h;
                        i++;
                    }
                    return finalSize;
                }
            }
        }
    }
}
