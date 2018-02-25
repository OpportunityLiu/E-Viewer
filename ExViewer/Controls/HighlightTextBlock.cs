using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using static ExViewer.Helpers.DocumentHelper;

namespace ExViewer.Controls
{
    [ContentProperty(Name = nameof(Text))]
    public class HighlightTextBlock : Control
    {
        public HighlightTextBlock()
        {
            this.DefaultStyleKey = typeof(HighlightTextBlock);
        }

        private TextBlock Presenter;

        protected override void OnApplyTemplate()
        {
            this.Presenter = GetTemplateChild(nameof(Presenter)) as TextBlock;
            reload();
        }

        public string Text
        {
            get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value);
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(HighlightTextBlock), new PropertyMetadata("", TextPropertyChangedCallback));

        private static void TextPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var o = (string)e.OldValue;
            var n = (string)e.NewValue;
            if (n == null)
                throw new ArgumentNullException(nameof(Text));
            if (o == n)
                return;
            ((HighlightTextBlock)sender).reload();
        }

        public string HighlightText
        {
            get => (string)GetValue(HighlightTextProperty); set => SetValue(HighlightTextProperty, value);
        }

        // Using a DependencyProperty as the backing store for HighlightText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightTextProperty =
            DependencyProperty.Register("HighlightText", typeof(string), typeof(HighlightTextBlock), new PropertyMetadata(null, HighlightTextPropertyChangedCallback));

        private static void HighlightTextPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var o = (string)e.OldValue;
            var n = (string)e.NewValue;
            if (o == n)
                return;
            ((HighlightTextBlock)sender).reload();
        }

        public StringComparison Comparison
        {
            get => (StringComparison)GetValue(ComparisonProperty); set => SetValue(ComparisonProperty, value);
        }

        // Using a DependencyProperty as the backing store for Comparison.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ComparisonProperty =
            DependencyProperty.Register("Comparison", typeof(StringComparison), typeof(HighlightTextBlock), new PropertyMetadata(StringComparison.CurrentCulture, ComparisonPropertyChangedCallback));

        private static void ComparisonPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var o = (StringComparison)e.OldValue;
            var n = (StringComparison)e.NewValue;
            if (o == n)
                return;
            ((HighlightTextBlock)sender).reload();
        }

        public Style TextBlockStyle
        {
            get => (Style)GetValue(TextBlockStyleProperty);
            set => SetValue(TextBlockStyleProperty, value);
        }

        public static readonly DependencyProperty TextBlockStyleProperty =
            DependencyProperty.Register(nameof(TextBlockStyle), typeof(Style), typeof(HighlightTextBlock), new PropertyMetadata(null));

        private void reload()
        {
            if (this.Presenter == null)
                return;
            this.Presenter.Text = "";
            this.Presenter.Inlines.Clear();
            var text = this.Text;
            var highlightText = this.HighlightText;
            if (string.IsNullOrEmpty(highlightText))
                this.Presenter.Text = text;
            else
            {
                var matchIndex = -1;
                var currentIndex = 0;
                while ((matchIndex = text.IndexOf(highlightText, currentIndex, Comparison)) != -1)
                {
                    if (matchIndex != currentIndex)
                        this.Presenter.Inlines.Add(CreateRun(text.Substring(currentIndex, matchIndex - currentIndex)));
                    this.Presenter.Inlines.Add(CreateBold(text.Substring(matchIndex, highlightText.Length)));
                    currentIndex = matchIndex + highlightText.Length;
                }
                if (currentIndex != text.Length)
                    this.Presenter.Inlines.Add(CreateRun(text.Substring(currentIndex)));
            }
        }
    }
}
