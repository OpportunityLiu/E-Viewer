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
        private static readonly ResourceDictionary categoryBrushes = getResource();

        private static ResourceDictionary getResource()
        {
            var r = new ResourceDictionary();
            Application.LoadComponent(r, new Uri("ms-appx:///Themes/Categories.xaml"));
            return r;
        }

        public CategoryTag()
        {
            this.DefaultStyleKey = typeof(CategoryTag);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.textPresenter = ((TextBlock)this.GetTemplateChild("TextPresenter"));
            if(this.textPresenter != null)
                this.textPresenter.Text = this.Category.ToFriendlyNameString().ToUpper();
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
            object brush;
            if(categoryBrushes.TryGetValue($"Category{newValue}BackgroundBrush", out brush))
            {
                sender.Background = (Brush)brush;
            }
            else
            {
                sender.ClearValue(BackgroundProperty);
            }
            if(sender.textPresenter != null)
                sender.textPresenter.Text = newValue.ToFriendlyNameString().ToUpper();
        }
    }
}
