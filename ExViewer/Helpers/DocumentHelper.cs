using ExViewer.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

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
            u.Click += Uri_Click;
            InAppNavigator.SetInAppUri(u, navigateUri);
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
            aBtn.Click += Uri_Click;
            InAppNavigator.SetInAppUri(aBtn, navigateUri);
            ToolTipService.SetToolTip(aBtn, navigateUri.ToString());
            return aBtn;
        }

        private static void Uri_Click(object sender, RoutedEventArgs e)
        {
            InAppNavigator.Navigate((DependencyObject)sender);
        }
    }
}
