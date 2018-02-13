using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.Foundation;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace ExViewer.Controls
{
    public sealed class Rating : Control
    {
        public Rating()
        {
            this.DefaultStyleKey = typeof(Rating);
        }

        public double PlaceholderValue
        {
            get => (double)GetValue(PlaceholderValueProperty);
            set => SetValue(PlaceholderValueProperty, value);
        }

        private TextBlock tbBackground;
        private TextBlock tbPlaceholder;
        private RectangleGeometry rgPlaceholderClip;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.tbBackground = GetTemplateChild("Background") as TextBlock;
            this.tbPlaceholder = GetTemplateChild("Placeholder") as TextBlock;
            this.rgPlaceholderClip = GetTemplateChild("PlaceholderClip") as RectangleGeometry;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            draw(finalSize);
            return base.ArrangeOverride(finalSize);
        }

        private void draw(Size size)
        {
            if (this.tbBackground == null || this.tbPlaceholder == null)
                return;
            this.rgPlaceholderClip.Rect = new Rect(0, 0, size.Width / 5 * this.PlaceholderValue, size.Height);
        }

        /// <summary>
        /// Indentify <see cref="PlaceholderValue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PlaceholderValueProperty =
            DependencyProperty.Register(nameof(PlaceholderValue), typeof(double), typeof(Rating), new PropertyMetadata(0d, PlaceholderValuePropertyChanged));

        private static void PlaceholderValuePropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (double)e.OldValue;
            var newValue = (double)e.NewValue;
            if (oldValue == newValue)
                return;
            var sender = (Rating)dp;
            if (double.IsNaN(newValue) || newValue > 5 || newValue < 0)
                throw new ArgumentOutOfRangeException(nameof(PlaceholderValue));
            sender.InvalidateArrange();
        }

    }
}
