using static ExViewer.Helpers.DocumentHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;

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
            if(e.NewValue == null)
                throw new ArgumentNullException(nameof(Text));
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
            ((HighlightTextBlock)sender).reload();
        }

        private void reload()
        {
            if(this.Presenter == null)
                return;
            this.Presenter.Text = "";
            this.Presenter.Inlines.Clear();
            var text = this.Text;
            var highlightText = this.HighlightText;
            if(string.IsNullOrEmpty(highlightText))
                this.Presenter.Text = text;
            else
            {
                var matchIndex = -1;
                var currentIndex = 0;
                while((matchIndex = text.IndexOf(highlightText, currentIndex)) != -1)
                {
                    if(matchIndex != currentIndex)
                        this.Presenter.Inlines.Add(CreateRun(text.Substring(currentIndex, matchIndex - currentIndex)));
                    this.Presenter.Inlines.Add(CreateBold(highlightText));
                    currentIndex = matchIndex + highlightText.Length;
                }
                if(currentIndex != text.Length)
                    this.Presenter.Inlines.Add(CreateRun(text.Substring(currentIndex)));
            }
        }
    }
}
