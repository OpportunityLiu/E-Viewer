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
using ExClient.Galleries.Rating;

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

        public Score? UserRatingValue
        {
            get => (Score?)GetValue(UserRatingValueProperty);
            set => SetValue(UserRatingValueProperty, value);
        }

        /// <summary>
        /// Indentify <see cref="UserRatingValue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty UserRatingValueProperty =
            DependencyProperty.Register(nameof(UserRatingValue), typeof(Score?), typeof(Rating), new PropertyMetadata(null, UserRatingValuePropertyChanged));

        private static void UserRatingValuePropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (Score?)e.OldValue;
            var newValue = (Score?)e.NewValue;
            if (oldValue == newValue)
                return;
            var sender = (Rating)dp;
            sender.InvalidateArrange();
        }

        private TextBlock tbBackground;
        private TextBlock tbPlaceholder;
        private TextBlock tbUserRating;
        private RectangleGeometry rgPlaceholderClip;
        private RectangleGeometry rgUserRatingClip;

        protected override void OnApplyTemplate()
        {
            this.tbBackground = GetTemplateChild("Background") as TextBlock;
            this.tbPlaceholder = GetTemplateChild("Placeholder") as TextBlock;
            this.tbUserRating = GetTemplateChild("UserRating") as TextBlock;
            this.rgPlaceholderClip = GetTemplateChild("PlaceholderClip") as RectangleGeometry;
            this.rgUserRatingClip = GetTemplateChild("UserRatingClip") as RectangleGeometry;
            base.OnApplyTemplate();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            draw(finalSize);
            return base.ArrangeOverride(finalSize);
        }

        private void draw(Size size)
        {
            var ph = this.PlaceholderValue;
            var ur = this.UserRatingValue;
            if (ur is Score urv)
            {
                if (this.rgPlaceholderClip != null)
                    this.rgPlaceholderClip.Rect = Rect.Empty;
                if (this.rgUserRatingClip != null)
                    this.rgUserRatingClip.Rect = new Rect(0, 0, size.Width / 5 * urv.ToDouble(), size.Height);
            }
            else
            {
                if (this.rgPlaceholderClip != null)
                    this.rgPlaceholderClip.Rect = new Rect(0, 0, size.Width / 5 * ph, size.Height);
                if (this.rgUserRatingClip != null)
                    this.rgUserRatingClip.Rect = Rect.Empty;
            }
        }
    }
}
