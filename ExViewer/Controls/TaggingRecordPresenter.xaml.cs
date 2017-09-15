using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ExClient.Status;
using ExClient.Tagging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Controls
{
    public sealed partial class TaggingRecordPresenter : UserControl
    {
        public TaggingRecordPresenter()
        {
            this.InitializeComponent();
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
            if (oldValue.IsBlocked == newValue.IsBlocked
                && oldValue.IsSlaved == newValue.IsSlaved
                && oldValue.Tag == newValue.Tag
                && oldValue.Score == newValue.Score)
                return;
            var sender = (TaggingRecordPresenter)dp;
            var dc = newValue.Tag.GetDisplayContentAsync();
            if (dc.Status == AsyncStatus.Completed)
            {
                sender.tbTag.Text = $"{newValue.Tag.Namespace.ToFriendlyNameString()}: {dc.GetResults()}";
            }
            else
            {
                sender.tbTag.Text = $"{newValue.Tag.Namespace.ToFriendlyNameString()}: {newValue.Tag.Content}";
                dc.Completed = (IAsyncOperation<string> op, AsyncStatus asyncStatus) =>
                {
                    if (asyncStatus != AsyncStatus.Completed)
                        return;
                    var dispValue = op.GetResults();
                    Opportunity.MvvmUniverse.DispatcherHelper.BeginInvokeOnUIThread(() =>
                    {
                        sender.tbTag.Text = $"{newValue.Tag.Namespace.ToFriendlyNameString()}: {dispValue}";
                    });
                };
            }
            if (newValue.Score > 0)
                sender.tbTag.Foreground = upBrush;
            else if (newValue.Score < 0)
                sender.tbTag.Foreground = downBrush;
            else
                sender.tbTag.ClearValue(TextBlock.ForegroundProperty);
            var indicators = "";
            if (newValue.IsBlocked && newValue.IsSlaved)
                indicators = Strings.Resources.Controls.TaggingRecordPresenter.BlockedAndSlavedIndicator;
            else if (newValue.IsBlocked)
                indicators = Strings.Resources.Controls.TaggingRecordPresenter.BlockedIndicator;
            else if (newValue.IsSlaved)
                indicators = Strings.Resources.Controls.TaggingRecordPresenter.SlavedIndicator;
            sender.tbStatus.Text = indicators;
        }

        private static readonly Brush upBrush = (Brush)Application.Current.Resources["VoteUpTagBrush"];
        private static readonly Brush downBrush = (Brush)Application.Current.Resources["VoteDownTagBrush"];
    }
}
