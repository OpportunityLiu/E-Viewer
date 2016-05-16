using ExClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace ExViewer.Controls
{
    public sealed class CategoryTag : Control
    {
        public CategoryTag()
        {
            this.DefaultStyleKey = typeof(CategoryTag);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            textPresenter = ((TextBlock)this.GetTemplateChild("TextPresenter"));
            if(textPresenter != null)
                textPresenter.Text = Category.ToFriendlyNameString().ToUpper();
        }

        private TextBlock textPresenter;

        public Category Category
        {
            get
            {
                return (Category)GetValue(CategoryProperty);
            }
            set
            {
                SetValue(CategoryProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Category.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CategoryProperty =
            DependencyProperty.Register("Category", typeof(Category), typeof(CategoryTag), new PropertyMetadata(Category.Unspecified, CategoryPropertyChangedCallback));

        private static void CategoryPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (CategoryTag)d;
            var newValue = (Category)e.NewValue;
            switch(newValue)
            {
            case Category.Doujinshi://#FFFF2525
                sender.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x25, 0x25));
                break;
            case Category.Manga://#FFFFB225
                sender.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xB2, 0x25));
                break;
            case Category.ArtistCG://#FFE8D825
                sender.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xE8, 0xD8, 0x25));
                break;
            case Category.GameCG://#FF259225
                sender.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x25, 0x92, 0x25));
                break;
            case Category.Western://#FF9AFF38
                sender.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x9A, 0xFF, 0x38));
                break;
            case Category.NonH://#FF38ACFF
                sender.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x38, 0xAC, 0xFF));
                break;
            case Category.ImageSet://#FF2525FF
                sender.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x25, 0x25, 0xFF));
                break;
            case Category.Cosplay://#FF652594
                sender.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x65, 0x25, 0x94));
                break;
            case Category.AsianPorn://#FFF2A7F2
                sender.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF2, 0xA7, 0xF2));
                break;
            case Category.Misc://#FFD3D3D3
                sender.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xD3, 0xD3, 0xD3));
                break;
            default:
                sender.ClearValue(BackgroundProperty);
                break;
            }
            if(sender.textPresenter != null)
                sender.textPresenter.Text = newValue.ToFriendlyNameString().ToUpper();
        }
    }
}
