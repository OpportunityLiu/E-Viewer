using ExClient.Status;
using ExClient.Tagging;
using System;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Controls
{
    public sealed partial class TaggingRecordPresenter : UserControl
    {
        public TaggingRecordPresenter()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            update(Record);
        }

        public TaggingRecord Record
        {
            get => (TaggingRecord)GetValue(RecordProperty);
            set => SetValue(RecordProperty, value);
        }

        /// <summary>
        /// Indentify <see cref="Record"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RecordProperty =
            DependencyProperty.Register(nameof(Record), typeof(TaggingRecord), typeof(TaggingRecordPresenter), new PropertyMetadata(default(TaggingRecord), RecordPropertyChanged));

        private static void RecordPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var oldValue = (TaggingRecord)e.OldValue;
            var newValue = (TaggingRecord)e.NewValue;
            if (oldValue == newValue)
            {
                return;
            }

            var sender = (TaggingRecordPresenter)dp;
            sender.update(newValue);
        }

        private void update(TaggingRecord data)
        {
            var dc = data.Tag.GetDisplayContentAsync();
            if (dc.Status == AsyncStatus.Completed)
            {
                tbTag.Text = $"{data.Tag.Namespace.ToFriendlyNameString()}: {dc.GetResults()}";
            }
            else
            {
                tbTag.Text = $"{data.Tag.Namespace.ToFriendlyNameString()}: {data.Tag.Content}";
                dc.Completed = (IAsyncOperation<string> op, AsyncStatus asyncStatus) =>
                {
                    if (asyncStatus != AsyncStatus.Completed)
                    {
                        return;
                    }

                    var dispValue = op.GetResults();
                    var t = $"{data.Tag.Namespace.ToFriendlyNameString()}: {dispValue}";
                    Dispatcher.Begin(() =>
                    {
                        tbTag.Text = t;
                    });
                };
            }

            if (data.Score > 0)
            {
                tbTag.Foreground = upBrush;
            }
            else if (data.Score < 0)
            {
                tbTag.Foreground = downBrush;
            }
            else
            {
                tbTag.ClearValue(TextBlock.ForegroundProperty);
            }

            var indicators = "";
            if (data.IsBlocked && data.IsSlaved)
            {
                indicators = BlockedAndSlavedIndicator;
            }
            else if (data.IsBlocked)
            {
                indicators = BlockedIndicator;
            }
            else if (data.IsSlaved)
            {
                indicators = SlavedIndicator;
            }

            tbStatus.Text = indicators;
        }

        private static readonly string BlockedAndSlavedIndicator = Strings.Resources.Controls.TaggingRecordPresenter.BlockedAndSlavedIndicator;
        private static readonly string BlockedIndicator = Strings.Resources.Controls.TaggingRecordPresenter.BlockedIndicator;
        private static readonly string SlavedIndicator = Strings.Resources.Controls.TaggingRecordPresenter.SlavedIndicator;

        private static readonly Brush upBrush = (Brush)Application.Current.Resources["VoteUpTagBrush"];
        private static readonly Brush downBrush = (Brush)Application.Current.Resources["VoteDownTagBrush"];
    }
}
