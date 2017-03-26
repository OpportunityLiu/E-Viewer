using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
            this.DefaultStyleKey = typeof(HtmlTextBlock);
            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;
            RegisterPropertyChangedCallback(XYFocusDownProperty, XYPropertyChanged);
            RegisterPropertyChangedCallback(XYFocusUpProperty, XYPropertyChanged);
            RegisterPropertyChangedCallback(XYFocusLeftProperty, XYPropertyChanged);
            RegisterPropertyChangedCallback(XYFocusRightProperty, XYPropertyChanged);
        }

        private void XYPropertyChanged(DependencyObject sender, DependencyProperty e)
        {
            if(!this.HasHyperlinks)
                return;
            var u = this.XYFocusUp;
            var d = this.XYFocusDown;
            var l = this.XYFocusLeft;
            var r = this.XYFocusRight;
            var begin = this.FirstLink;
            switch(begin)
            {
            case Hyperlink hl:
                hl.XYFocusUp = this.XYFocusUp;
                break;
            case HyperlinkButton hlb:
                hlb.XYFocusUp = this.XYFocusUp;
                break;
            default:
                throw new InvalidOperationException();
            }
            var end = this.LastLink;
            switch(end)
            {
            case Hyperlink hl:
                hl.XYFocusDown = this.XYFocusDown;
                break;
            case HyperlinkButton hlb:
                hlb.XYFocusDown = this.XYFocusDown;
                break;
            default:
                throw new InvalidOperationException();
            }
            var o = begin;
            while(o != null)
            {
                switch(o)
                {
                case Hyperlink hl:
                    hl.XYFocusLeft = this.XYFocusLeft;
                    hl.XYFocusRight = this.XYFocusRight;
                    o = hl.XYFocusDown;
                    break;
                case HyperlinkButton hlb:
                    hlb.XYFocusLeft = this.XYFocusLeft;
                    hlb.XYFocusRight = this.XYFocusRight;
                    o = hlb.XYFocusDown;
                    break;
                default:
                    throw new InvalidOperationException();
                }
            }
        }

        private RichTextBlock Presenter;

        protected override void OnApplyTemplate()
        {
            this.Presenter = GetTemplateChild(nameof(Presenter)) as RichTextBlock;
            reload();
        }

        public HtmlNode HtmlContent
        {
            get => (HtmlNode)GetValue(HtmlContentProperty);
            set => SetValue(HtmlContentProperty, value);
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
            get => (bool)GetValue(DetectLinkProperty);
            set => SetValue(DetectLinkProperty, value);
        }

        // Using a DependencyProperty as the backing store for DetectLink.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DetectLinkProperty =
            DependencyProperty.Register("DetectLink", typeof(bool), typeof(HtmlTextBlock), new PropertyMetadata(false, DetectLinkPropertyChanged));

        public DependencyObject FirstLink
        {
            get => (DependencyObject)GetValue(FirstLinkProperty);
            private set => SetValue(FirstLinkProperty, value);
        }

        public static readonly DependencyProperty FirstLinkProperty =
            DependencyProperty.Register(nameof(FirstLink), typeof(DependencyObject), typeof(HtmlTextBlock), new PropertyMetadata(null));

        public DependencyObject LastLink
        {
            get => (DependencyObject)GetValue(LastLinkProperty);
            private set => SetValue(LastLinkProperty, value);
        }

        public static readonly DependencyProperty LastLinkProperty =
            DependencyProperty.Register(nameof(LastLink), typeof(DependencyObject), typeof(HtmlTextBlock), new PropertyMetadata(null));

        public static void DetectLinkPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var s = (HtmlTextBlock)sender;
            s.reload();
        }

        private async void reload()
        {
            var presenter = this.Presenter;
            var htmlContent = this.HtmlContent;
            if(presenter == null)
                return;
            if(this.HasHyperlinks)
            {
                var l = this.FirstLink;
                while(l != null)
                {
                    switch(l)
                    {
                    case Hyperlink hl:
                        hl.XYFocusLeft = null;
                        hl.XYFocusRight = null;
                        hl.XYFocusUp = null;
                        l = hl.XYFocusDown;
                        hl.XYFocusDown = null;
                        break;
                    case HyperlinkButton hlb:
                        hlb.XYFocusLeft = null;
                        hlb.XYFocusRight = null;
                        hlb.XYFocusUp = null;
                        l = hlb.XYFocusDown;
                        hlb.XYFocusDown = null;
                        break;
                    default:
                        throw new InvalidOperationException();
                    }
                }
            }
            presenter.Blocks.Clear();
            if(htmlContent == null)
            {
                this.HasHyperlinks = false;
                this.FirstLink = null;
                this.LastLink = null;
                return;
            }
            var para = new Paragraph();
            presenter.Blocks.Add(para);
            var links = await loadHtmlAsync(para, htmlContent, this.DetectLink);
            if(presenter == this.Presenter && htmlContent == this.HtmlContent)
            {
                if(links.Count > 1)
                {
                    for(var i = 0; i < links.Count - 1; i++)
                    {
                        link(links[i], links[i + 1]);
                    }
                }
                if(links.Count != 0)
                {
                    this.HasHyperlinks = true;
                    this.FirstLink = links.First();
                    this.LastLink = links.Last();
                    XYPropertyChanged(this, null);
                }
                else
                {
                    this.HasHyperlinks = false;
                    this.FirstLink = null;
                    this.LastLink = null;
                }
            }
        }

        private void link(DependencyObject obj1, DependencyObject obj2)
        {
            var o1h = obj1 as Hyperlink;
            var o1b = obj1 as HyperlinkButton;
            var o2h = obj2 as Hyperlink;
            var o2b = obj2 as HyperlinkButton;
            if(o1h != null)
            {
                if(o2h != null)
                {
                    o1h.XYFocusDown = o2h;
                    o2h.XYFocusUp = o1h;
                }
                else
                {
                    o1h.XYFocusDown = o2b;
                    o2b.XYFocusUp = o1h;
                }
            }
            else
            {
                if(o2h != null)
                {
                    o1b.XYFocusDown = o2h;
                    o2h.XYFocusUp = o1b;
                }
                else
                {
                    o1b.XYFocusDown = o2b;
                    o2b.XYFocusUp = o1b;
                }
            }
        }

        private bool canChangeHasHyperlinks;

        public bool HasHyperlinks
        {
            get => (bool)GetValue(HasHyperlinksProperty);
            private set
            {
                this.canChangeHasHyperlinks = true;
                SetValue(HasHyperlinksProperty, value);
                this.canChangeHasHyperlinks = false;
            }
        }

        // Using a DependencyProperty as the backing store for HasHyperlinks.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HasHyperlinksProperty =
            DependencyProperty.Register("HasHyperlinks", typeof(bool), typeof(HtmlTextBlock), new PropertyMetadata(false, HasHyperlinksPropertyChangedCallback));

        private static void HasHyperlinksPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var s = (HtmlTextBlock)sender;
            if(!s.canChangeHasHyperlinks)
            {
                s.HasHyperlinks = (bool)e.OldValue;
            }
        }

        private async Task<List<DependencyObject>> loadHtmlAsync(Paragraph target, HtmlNode content, bool detectLink)
        {
            var hyperlinks = new List<DependencyObject>();
            foreach(var node in content.ChildNodes)
            {
                var tbNode = createNode(node, hyperlinks, detectLink);
                target.Inlines.Add(tbNode);
                await Task.Yield();
            }
            target.Inlines.Add(new Run { Text = eof });
            return hyperlinks;
        }

        private Inline createNode(HtmlNode node, List<DependencyObject> hyperlinks, bool detectLink)
        {
            if(node is HtmlTextNode)
            {
                var text = HtmlEntity.DeEntitize(node.InnerText);
                MatchCollection matches;
                if(detectLink && (matches = linkDetector.Matches(text)).Count > 0)
                {
                    var t = new Span();
                    var currentPos = 0;
                    foreach(var match in matches.Cast<Match>())
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
                                var hl = CreateHyperlink(match.Value, uri);
                                hyperlinks.Add(hl);
                                t.Inlines.Add(hl);
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
                foreach(var item in createChildNodes(node, hyperlinks, detectLink))
                    b.Inlines.Add(item);
                return b;
            case "em"://[i]
                var i = new Italic();
                foreach(var item in createChildNodes(node, hyperlinks, detectLink))
                    i.Inlines.Add(item);
                return i;
            case "span"://[u]
                var u = new Underline();
                foreach(var item in createChildNodes(node, hyperlinks, detectLink))
                    u.Inlines.Add(item);
                return u;
            case "del"://[s]
                var s = new Span() { Foreground = (Brush)this.Resources["SystemControlBackgroundChromeMediumBrush"] };
                foreach(var item in createChildNodes(node, hyperlinks, detectLink))
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
                    foreach(var item in createChildNodes(node, hyperlinks, false))
                        container.Inlines.Add(item);
                    if(container is Hyperlink)
                        hyperlinks.Add(container);
                    return container;
                }
                catch(ArgumentException)// has InlineUIContainer in childnodes
                {
                    var aBtnContent = new RichTextBlock { IsTextSelectionEnabled = false };
                    var para = new Paragraph();
                    aBtnContent.Blocks.Add(para);
                    var ignore = loadHtmlAsync(para, node, false);
                    var aBtn = CreateHyperlinkButton(aBtnContent, target);
                    hyperlinks.Add(aBtn);
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

        private static void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            var image = (Image)sender;
            setScale(image);
            image.ImageOpened -= Image_ImageOpened;
        }

        private IEnumerable<Inline> createChildNodes(HtmlNode node, List<DependencyObject> hyperlinks, bool detectLink)
        {
            return node.ChildNodes.Select(n => createNode(n, hyperlinks, detectLink));
        }

        private static void setScale(Image image)
        {
            var scaleRate = Math.Sqrt(dpi.RawPixelsPerViewPixel);
            var bitmap = (BitmapImage)image.Source;
            image.Width = bitmap.PixelWidth / scaleRate;
            image.Height = bitmap.PixelHeight / scaleRate;
        }

        protected override void OnDisconnectVisualChildren()
        {
            ClearValue(HtmlContentProperty);
            base.OnDisconnectVisualChildren();
        }

        private static DisplayInformation dpi = DisplayInformation.GetForCurrentView();

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            dpi.DpiChanged += this.Dpi_DpiChanged;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            dpi.DpiChanged -= this.Dpi_DpiChanged;
        }

        private void Dpi_DpiChanged(DisplayInformation sender, object args)
        {
            foreach(var item in this.Descendants<Image>())
            {
                setScale(item);
            }
        }
    }
}
