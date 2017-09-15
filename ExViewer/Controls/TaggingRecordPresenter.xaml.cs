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
            DependencyProperty.Register(nameof(Record), typeof(TaggingRecord), typeof(TaggingRecordPresenter), new PropertyMetadata(default(TaggingRecord)));
    }
}
