using ExClient;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace ExViewer.Controls
{
    [TemplatePart(Name = nameof(TextPresenter), Type = typeof(TextBlock))]
    public sealed class CategoryTag : Control
    {
        private static readonly ResourceDictionary categoryBrushes = getResource();

        private static ResourceDictionary getResource()
        {
#if DEBUG
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                return null;
            }
#endif
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
            this.TextPresenter = (TextBlock)this.GetTemplateChild(nameof(this.TextPresenter));
            setValue(this.Category);
        }

        private void setValue(Category cat)
        {
#if DEBUG
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                if (this.TextPresenter != null)
                {
                    this.TextPresenter.Text = "TESTVALUE";
                }

                return;
            }
#endif
            if (this.TextPresenter != null)
            {
                this.TextPresenter.Text = cat.ToFriendlyNameString().ToUpper();
            }
        }

        private TextBlock TextPresenter;

        public Category Category
        {
            get => (Category)GetValue(CategoryProperty);
            set => SetValue(CategoryProperty, value);
        }

        // Using a DependencyProperty as the backing store for Category.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CategoryProperty =
            DependencyProperty.Register("Category", typeof(Category), typeof(CategoryTag), new PropertyMetadata(Category.Unspecified, CategoryPropertyChangedCallback));

        private static void CategoryPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (CategoryTag)d;
            var oldValue = (Category)e.OldValue;
            var newValue = (Category)e.NewValue;
            if (oldValue == newValue)
            {
                return;
            }

            sender.setValue(newValue);
#if DEBUG
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                return;
            }
#endif
            if (categoryBrushes.TryGetValue($"Category{newValue}BackgroundBrush", out var brush))
            {
                sender.Background = (Brush)brush;
            }
            else
            {
                sender.ClearValue(BackgroundProperty);
            }
        }
    }
}
