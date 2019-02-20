using ExClient.Galleries.Rating;
using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace ExViewer.Controls
{
    public sealed class Rating : Control
    {
        public Rating()
        {
            DefaultStyleKey = typeof(Rating);
            FocusEngaged += Rating_FocusEngaged;
            FocusDisengaged += Rating_FocusDisengaged;
        }

        private void Rating_FocusDisengaged(Control sender, FocusDisengagedEventArgs args)
        {
            ElementSoundPlayer.Play(ElementSoundKind.GoBack);
        }

        private void Rating_FocusEngaged(Control sender, FocusEngagedEventArgs args)
        {
            if (actualUserRating == Score.NotSet)
            {
                actualUserRating = PlaceholderValue.ToScore();
            }

            ElementSoundPlayer.Play(ElementSoundKind.Invoke);
            draw();
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
            {
                return;
            }

            var sender = (Rating)dp;
            if (double.IsNaN(newValue) || newValue > 5 || newValue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(PlaceholderValue));
            }

            sender.draw();
        }

        public Score UserRatingValue
        {
            get => (Score)GetValue(UserRatingValueProperty);
            set => SetValue(UserRatingValueProperty, value);
        }

        /// <summary>
        /// Indentify <see cref="UserRatingValue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty UserRatingValueProperty =
            DependencyProperty.Register(nameof(UserRatingValue), typeof(Score), typeof(Rating), new PropertyMetadata(Score.NotSet, UserRatingValuePropertyChanged));

        private static void UserRatingValuePropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (Score)e.OldValue;
            var newValue = (Score)e.NewValue;
            if (oldValue == newValue)
            {
                return;
            }

            var sender = (Rating)dp;
            sender.actualUserRating = newValue;
            sender.draw();
        }

        private TextBlock tbBackground;
        private TextBlock tbPlaceholder;
        private TextBlock tbUserRating;
        private RectangleGeometry rgPlaceholderClip;
        private RectangleGeometry rgUserRatingClip;

        private Score actualUserRating = Score.NotSet;

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            actualUserRating = UserRatingValue;
            draw();
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            ElementSoundPlayer.Play(ElementSoundKind.Focus);
            base.OnGotFocus(e);
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);
            if (!IsEnabled)
            {
                return;
            }

            e.Handled = true;
            switch (e.OriginalKey)
            {
            case Windows.System.VirtualKey.Right:
            case Windows.System.VirtualKey.GamepadDPadRight:
            case Windows.System.VirtualKey.GamepadLeftThumbstickRight:
                if (actualUserRating == Score.NotSet)
                {
                    actualUserRating = PlaceholderValue.ToScore();
                }

                if (actualUserRating != Score.Score_5_0)
                {
                    actualUserRating++;
                }
                ElementSoundPlayer.Play(ElementSoundKind.Focus);
                draw();
                break;
            case Windows.System.VirtualKey.Left:
            case Windows.System.VirtualKey.GamepadDPadLeft:
            case Windows.System.VirtualKey.GamepadLeftThumbstickLeft:
                if (actualUserRating == Score.NotSet)
                {
                    var a = PlaceholderValue;
                    if (a <= 0.5)
                    {
                        actualUserRating = Score.Score_0_5;
                    }
                    else
                    {
                        actualUserRating = a.ToScore();
                    }
                }

                if (actualUserRating != Score.Score_0_5)
                {
                    actualUserRating--;
                }

                ElementSoundPlayer.Play(ElementSoundKind.Focus);
                draw();
                break;
            case Windows.System.VirtualKey.Space:
            case Windows.System.VirtualKey.Enter:
            case Windows.System.VirtualKey.GamepadA:
                if (actualUserRating != Score.NotSet)
                {
                    UserRatingValue = actualUserRating;
                }

                ElementSoundPlayer.Play(ElementSoundKind.Invoke);
                RemoveFocusEngagement();
                break;
            case Windows.System.VirtualKey.Escape:
            case Windows.System.VirtualKey.GamepadB:
                actualUserRating = UserRatingValue;
                RemoveFocusEngagement();
                draw();
                break;
            default:
                e.Handled = false;
                break;
            }
        }

        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            base.OnPointerMoved(e);
            if (FocusState == FocusState.Keyboard)
            {
                return;
            }

            e.Handled = true;
            var p = e.GetCurrentPoint(this);
            var pp = p.Position.X / ActualWidth;

            if (pp < -0.5)
            {
                pp = -1;
            }
            else if (pp < 0)
            {
                pp = 0;
            }
            else if (pp > 1)
            {
                pp = 1;
            }

            if (pp < 0 || !IsEnabled)
            {
                actualUserRating = UserRatingValue;
            }
            else
            {
                actualUserRating = (Score)Math.Max((byte)Math.Round(pp * 10), (byte)1);
            }

            draw();
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            if (FocusState == FocusState.Keyboard)
            {
                return;
            }

            e.Handled = true;
            actualUserRating = UserRatingValue;
            draw();
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (!IsEnabled)
            {
                return;
            }

            if (FocusState == FocusState.Keyboard)
            {
                return;
            }

            e.Handled = true;
            CapturePointer(e.Pointer);
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);
            ReleasePointerCapture(e.Pointer);
            if (FocusState == FocusState.Keyboard)
            {
                return;
            }

            e.Handled = true;
            if (IsEnabled)
            {
                UserRatingValue = actualUserRating;
            }
            else
            {
                actualUserRating = UserRatingValue;
                draw();
            }
        }

        protected override void OnApplyTemplate()
        {
            tbBackground = GetTemplateChild("Background") as TextBlock;
            tbPlaceholder = GetTemplateChild("Placeholder") as TextBlock;
            tbUserRating = GetTemplateChild("UserRating") as TextBlock;
            rgPlaceholderClip = GetTemplateChild("PlaceholderClip") as RectangleGeometry;
            rgUserRatingClip = GetTemplateChild("UserRatingClip") as RectangleGeometry;
            base.OnApplyTemplate();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            draw(finalSize);
            return base.ArrangeOverride(finalSize);
        }

        private void draw() => draw(new Size(ActualWidth, ActualHeight));
        private void draw(Size size)
        {
            var ph = PlaceholderValue;
            var ur = actualUserRating;
            if (ur > 0)
            {
                if (rgPlaceholderClip != null)
                {
                    rgPlaceholderClip.Rect = Rect.Empty;
                }

                if (rgUserRatingClip != null)
                {
                    rgUserRatingClip.Rect = new Rect(0, 0, size.Width / 5 * ur.ToDouble(), size.Height);
                }
            }
            else
            {
                if (rgPlaceholderClip != null)
                {
                    rgPlaceholderClip.Rect = new Rect(0, 0, size.Width / 5 * ph, size.Height);
                }

                if (rgUserRatingClip != null)
                {
                    rgUserRatingClip.Rect = Rect.Empty;
                }
            }
        }
    }
}
