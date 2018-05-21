using ExClient.Services;
using HtmlAgilityPack;
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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace ExViewer.Controls
{
    public sealed partial class ExpungeRecordPresenter : UserControl
    {
        public ExpungeRecordPresenter()
        {
            this.InitializeComponent();
        }

        public ExpungeRecord Record
        {
            get => (ExpungeRecord)GetValue(RecordProperty);
            set => SetValue(RecordProperty, value);
        }

        /// <summary>
        /// Indentify <see cref="Record"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RecordProperty =
            DependencyProperty.Register(nameof(Record), typeof(ExpungeRecord), typeof(ExpungeRecordPresenter), new PropertyMetadata(default(ExpungeRecord)));

        private static HtmlNode warpString(string data)
        {
            if (data.IsNullOrWhiteSpace())
                return null;
            return HtmlNode.CreateNode(HtmlEntity.Entitize(data, true, true));
        }
    }
}
