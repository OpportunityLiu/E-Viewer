using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static ExViewer.Helpers.DocumentHelper;

namespace ExViewer.Controls
{
    public class HtmlTextBlock : Control
    {
        public HtmlTextBlock()
        {
            DefaultStyleKey = typeof(HtmlTextBlock);
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
        }

        private RichTextBlock Presenter;

        protected override void OnApplyTemplate()
        {
            this.Presenter = GetTemplateChild(nameof(Presenter)) as RichTextBlock;
            reload();
        }

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
            s.reload();
        }

        public bool DetectLink
        {
            get
            {
                return (bool)GetValue(DetectLinkProperty);
            }
            set
            {
                SetValue(DetectLinkProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for DetectLink.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DetectLinkProperty =
            DependencyProperty.Register("DetectLink", typeof(bool), typeof(HtmlTextBlock), new PropertyMetadata(false, DetectLinkPropertyChanged));

        public static void DetectLinkPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var s = (HtmlTextBlock)sender;
            s.reload();
        }

        private static readonly string eof = " ";
        private static readonly Regex linkDetector = new Regex(
            @"
(
  (?<explict>[a-zA-z]+://[^\s]*)
|
  (?<implict>
    (?<=\s|^)
    ([^:@/\\\.\s]+\.)+
    (a[d-gil-oq-uwz]|aero|b[abd-jmnorstvwyz]|biz|c[acf-ik-oqruvxyz]|com|coop|d[ejkmoz]|e[ceghstv]|edu|f[ijkmor]|g[abdefhilmnprtuwy]|gov|h[kmnrtu]|i[delnoq-t]|info|in[kt]|j[mop]|k[eghimnprwyz]|l[abcikr-vy]|m[acdeghl-tv-z]|mil|mobi|moe|na(?:me)?|n[cefgiloprtuz]|net|[opstz]m|org|p[ae-hklnrtwy]|pro|pub|[qsuvz]a|red?|r[ouw]|s[b-eg-lnortuyz]|t[cdfghjklnoprtvwz]|tech|top|u[gksy]|v[cegnu]|w[fs]|y[eu]|z[rw])
    (:\d+)?
    (
        [/\\?\.]
        [^\s]*
    )?
    (?=\s|$)
  )
)", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.ExplicitCapture);

        private void reload()
        {
            if(this.Presenter == null)
                return;
            this.loadHtml(this.Presenter, this.HtmlContent, this.DetectLink);
        }

        private void loadHtml(RichTextBlock presenter, HtmlNode content, bool detectLink)
        {
            presenter.Blocks.Clear();
            if(content == null)
                return;
            var para = new Paragraph();
            foreach(var node in content.ChildNodes)
            {
                var tbNode = createNode(node, detectLink);
                para.Inlines.Add(tbNode);
            }
            para.Inlines.Add(new Run { Text = eof });
            presenter.Blocks.Add(para);
        }

        private Inline createNode(HtmlNode node, bool detectLink)
        {
            if(node is HtmlTextNode)
            {
                var text = HtmlEntity.DeEntitize(node.InnerText);
                MatchCollection matches;
                if(detectLink && (matches = linkDetector.Matches(text)).Count > 0)
                {
                    var t = new Span();
                    var currentPos = 0;
                    foreach(Match match in matches)
                    {
                        t.Inlines.Add(CreateRun(text.Substring(currentPos, match.Index - currentPos)));
                        var uri = (Uri)null;
                        try
                        {
                            if(match.Groups["implict"].Success)
                                uri = new Uri($"http://{match.Value}");
                            else if(match.Groups["explict"].Success)
                                uri = new Uri(match.Value);
                        }
                        catch(UriFormatException) { }
                        if(uri != null)
                        {
                            try
                            {
                                t.Inlines.Add(CreateHyperlink(match.Value, uri));
                            }
                            catch(Exception)
                            {
                                t.Inlines.Add(CreateRun(match.Value));
                            }
                        }
                        else
                        {
                            t.Inlines.Add(CreateRun(match.Value));
                        }
                        currentPos = match.Index + match.Length;
                    }
                    t.Inlines.Add(CreateRun(text.Substring(currentPos)));
                    return t;
                }
                else
                {
                    return CreateRun(text);
                }
            }
            switch(node.Name)
            {
            case "br":
                return new LineBreak();
            case "strong"://[b]
                var b = new Bold();
                foreach(var item in createChildNodes(node, detectLink))
                    b.Inlines.Add(item);
                return b;
            case "em"://[i]
                var i = new Italic();
                foreach(var item in createChildNodes(node, detectLink))
                    i.Inlines.Add(item);
                return i;
            case "span"://[u]
                var u = new Underline();
                foreach(var item in createChildNodes(node, detectLink))
                    u.Inlines.Add(item);
                return u;
            case "del"://[s]
                var s = new Span() { Foreground = (Brush)Resources["SystemControlBackgroundChromeMediumBrush"] };
                foreach(var item in createChildNodes(node, detectLink))
                    s.Inlines.Add(item);
                return s;
            case "a"://[url]
                var container = (Span)null;
                var target = (Uri)null;
                try
                {
                    target = new Uri(HtmlEntity.DeEntitize(node.GetAttributeValue("href", "https://exhentai.org")));
                    container = CreateHyperlink(null, target);
                }
                catch(UriFormatException)
                {
                    container = new Span();
                }
                try
                {
                    foreach(var item in createChildNodes(node, false))
                        container.Inlines.Add(item);
                    return container;
                }
                catch(ArgumentException)// has InlineUIContainer in childnodes
                {
                    var aBtnContent = new RichTextBlock { IsTextSelectionEnabled = false };
                    loadHtml(aBtnContent, node, false);
                    var aBtn = CreateHyperlinkButton(aBtnContent, target);
                    return new InlineUIContainer { Child = aBtn };
                }
            case "img"://[img]
                var image = new Image
                {
                    Source = new BitmapImage
                    {
                        UriSource = new Uri(node.GetAttributeValue("src", ""))
                    },
                    Width = 0,
                    Height = 0
                };
                image.ImageOpened += Image_ImageOpened;
                var img = new InlineUIContainer { Child = image };
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

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            var scaleRate = Math.Sqrt(dpi.RawPixelsPerViewPixel);
            var image = (Image)sender;
            var bitmap = (BitmapImage)image.Source;
            image.Width = bitmap.PixelWidth / scaleRate;
            image.Height = bitmap.PixelHeight / scaleRate;
            image.ImageOpened -= Image_ImageOpened;
        }

        private static DisplayInformation dpi = DisplayInformation.GetForCurrentView();

        private IEnumerable<Inline> createChildNodes(HtmlNode node, bool detectLink)
        {
            return node.ChildNodes.Select(n => createNode(n, detectLink));
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            dpi.DpiChanged += Dpi_DpiChanged;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            dpi.DpiChanged -= Dpi_DpiChanged;
        }

        private void Dpi_DpiChanged(DisplayInformation sender, object args)
        {
            reload();
        }
    }
}
