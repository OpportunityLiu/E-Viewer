using ExViewer.Controls;
using System;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media.Imaging;

namespace ExViewer.Helpers
{
    public static class DocumentHelper
    {
        public static Run CreateRun(string text)
        {
            return new Run { Text = text };
        }

        public static Bold CreateBold(string text)
        {
            var b = new Bold();
            b.Inlines.Add(new Run { Text = text });
            return b;
        }

        public static Italic CreateItalic(string text)
        {
            var i = new Italic();
            i.Inlines.Add(new Run { Text = text });
            return i;
        }

        public static Underline CreateUnderline(string text)
        {
            var u = new Underline();
            u.Inlines.Add(new Run { Text = text });
            return u;
        }

        public static Hyperlink CreateHyperlink(string text, Uri navigateUri)
        {
            var u = new Hyperlink();
            if(navigateUri != null)
            {
                u.Click += Uri_Click;
                InAppNavigator.SetInAppUri(u, navigateUri);
                ToolTipService.SetToolTip(u, navigateUri.ToString());
            }
            if(text != null)
                u.Inlines.Add(new Run { Text = text });
            return u;
        }

        public static HyperlinkButton CreateHyperlinkButton(object content, Uri navigateUri)
        {
            var aBtn = new HyperlinkButton()
            {
                Content = content,
                Padding = new Thickness()
            };
            if(navigateUri != null)
            {
                aBtn.Click += Uri_Click;
                InAppNavigator.SetInAppUri(aBtn, navigateUri);
                ToolTipService.SetToolTip(aBtn, navigateUri.ToString());
            }
            return aBtn;
        }

        public static Image CreateImage(Uri imgSrc)
        {
            var image = new Image
            {
                Source = new BitmapImage
                {
                    UriSource = imgSrc
                },
                Width = 0,
                Height = 0
            };
            image.ImageOpened += Image_ImageOpened;
            return image;
        }

        private static DisplayInformation dpi = DisplayInformation.GetForCurrentView();

        private static void setScale(Image image)
        {
            var scaleRate = Math.Sqrt(dpi.RawPixelsPerViewPixel);
            var bitmap = (BitmapImage)image.Source;
            image.Width = bitmap.PixelWidth / scaleRate;
            image.Height = bitmap.PixelHeight / scaleRate;
        }

        private static void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            var image = (Image)sender;
            setScale(image);
            image.ImageOpened -= Image_ImageOpened;
        }

        private static void Uri_Click(object sender, RoutedEventArgs e)
        {
            var uri = InAppNavigator.GetInAppUri((DependencyObject)sender);
            UriHandler.Handle(uri);
        }
    }
}
