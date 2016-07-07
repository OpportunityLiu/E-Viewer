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
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Controls
{
    public sealed partial class HtmlTextBlock : UserControl
    {
        public HtmlTextBlock()
        {
            this.InitializeComponent();
        }

        public Style RichTextBlockStyle
        {
            get
            {
                return (Style)GetValue(RichTextBlockStyleProperty);
            }
            set
            {
                SetValue(RichTextBlockStyleProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for TextBlockStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RichTextBlockStyleProperty =
            DependencyProperty.Register("RichTextBlockStyle", typeof(Style), typeof(HtmlTextBlock), new PropertyMetadata(null));

        public HtmlNode HtmlContent
        {
            get
            {
                return (HtmlNode)GetValue(HtmlContentProperty);
            }
            set
            {
                SetValue(HtmlContentProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for HtmlContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HtmlContentProperty =
            DependencyProperty.Register("HtmlContent", typeof(HtmlNode), typeof(HtmlTextBlock), new PropertyMetadata(null, HtmlContentPropertyChanged));


        public static void HtmlContentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var s = (HtmlTextBlock)sender;
            s.loadHtml(s.Presenter, (HtmlNode)e.NewValue);
        }

        private void loadHtml(RichTextBlock presenter, HtmlNode content)
        {
            presenter.Blocks.Clear();
            if(content == null)
                return;
            var para = new Paragraph() { Foreground = (Brush)Resources["ApplicationForegroundThemeBrush"] };
            foreach(var node in content.ChildNodes)
            {
                var tbNode = createNode(node);
                para.Inlines.Add(tbNode);
            }
            presenter.Blocks.Add(para);
        }

        private Inline createNode(HtmlNode node)
        {
            if(node is HtmlTextNode)
                return new Run { Text = HtmlEntity.DeEntitize(node.InnerText) };
            switch(node.Name)
            {
            case "br":
                return new LineBreak();
            case "strong"://[b]
                var b = new Bold();
                foreach(var item in createChildNodes(node))
                    b.Inlines.Add(item);
                return b;
            case "em"://[i]
                var i = new Italic();
                foreach(var item in createChildNodes(node))
                    i.Inlines.Add(item);
                return i;
            case "span"://[u]
                var u = new Underline();
                foreach(var item in createChildNodes(node))
                    u.Inlines.Add(item);
                return u;
            case "del"://[s]
                var s = new Span() { Foreground = (Brush)Resources["ApplicationSecondaryForegroundThemeBrush"] };
                foreach(var item in createChildNodes(node))
                    s.Inlines.Add(item);
                return s;
            case "a"://[url]
                var target = new Uri(node.GetAttributeValue("href", "http://exhentai.org"));
                try
                {
                    var aLink = new Hyperlink() { NavigateUri = target };
                    foreach(var item in createChildNodes(node))
                        aLink.Inlines.Add(item);
                    return aLink;
                }
                catch(ArgumentException)// has InlineUIContainer in childnodes
                {
                    var aBtnContent = new RichTextBlock { IsTextSelectionEnabled = false };
                    loadHtml(aBtnContent, node);
                    var aBtn = new HyperlinkButton()
                    {
                        NavigateUri = target,
                        Content = aBtnContent,
                        Padding = new Thickness()
                    };
                    ToolTipService.SetToolTip(aBtn, target);
                  return new InlineUIContainer { Child = aBtn };
                }
            case "img"://[img]
                var img = new InlineUIContainer()
                {
                    Child = new Image()
                    {
                        Source = new BitmapImage()
                        {
                            UriSource = new Uri(node.GetAttributeValue("src", ""))
                        },
                        Stretch = Stretch.None
                    }
                };
                var con = new Span();
                con.Inlines.Add(img);
                return con;
            default:
                return new Run
                {
                    Text = node.InnerHtml
                };
            }
        }

        private IEnumerable<Inline> createChildNodes(HtmlNode node)
        {
            return node.ChildNodes.Select(n => createNode(n));
        }
    }
}
