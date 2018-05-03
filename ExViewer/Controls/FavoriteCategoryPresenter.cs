using ExClient;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ExViewer.Controls
{
    [TemplatePart(Name = nameof(Label), Type = typeof(TextBlock))]
    [TemplatePart(Name = nameof(Icon), Type = typeof(UIElement))]
    public class FavoriteCategoryPresenter : Control
    {
        public FavoriteCategoryPresenter()
        {
            this.DefaultStyleKey = typeof(FavoriteCategoryPresenter);
            this.Loaded += this.favoriteCategoryPresenter_Loaded;
            this.Unloaded += this.favoriteCategoryPresenter_Unloaded;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.Icon = (UIElement)GetTemplateChild(nameof(Icon));
            this.Label = null;
            set(this.Category, this.IsLabelVisible);
        }

        private TextBlock Label;
        private UIElement Icon;

        private void set(FavoriteCategory category, bool labelVisible)
        {
            category = category ?? Client.Current.Favorites.All;
            var icon = this.Icon;
            if (icon != null)
            {
                icon.Visibility = category.Index < 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            this.BorderBrush = category.GetThemeBrush();
            if (labelVisible)
            {
                var label = this.Label;
                if (label is null)
                {
                    label = GetTemplateChild("Label") as TextBlock;
                    if (label is null)
                    {
                        return;
                    }
                    else
                    {
                        this.Label = label;
                    }
                }
                label.Visibility = Visibility.Visible;
                label.Text = category.Name ?? "";
            }
            else
            {
                if (this.Label is null)
                {
                    return;
                }

                this.Label.Visibility = Visibility.Collapsed;
            }
        }

        public FavoriteCategory Category
        {
            get => (FavoriteCategory)GetValue(CategoryProperty);
            set => SetValue(CategoryProperty, value);
        }

        // Using a DependencyProperty as the backing store for Category.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CategoryProperty =
            DependencyProperty.Register("Category", typeof(FavoriteCategory), typeof(FavoriteCategoryPresenter), new PropertyMetadata(null, categoryPropertyChanged));

        private bool loaded;

        private static void categoryPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var dp = (FavoriteCategoryPresenter)sender;
            var o = (FavoriteCategory)e.OldValue;
            var n = (FavoriteCategory)e.NewValue;
            if (o != null)
            {
                o.PropertyChanged -= dp.category_PropertyChanged;
            }

            if (dp.loaded && n != null)
            {
                n.PropertyChanged += dp.category_PropertyChanged;
            }

            dp.set(n, dp.IsLabelVisible);
            if (n is null)
            {
                dp.ClearValue(CategoryProperty);
                return;
            }
        }

        private void category_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.set(this.Category, this.IsLabelVisible);
        }

        private void favoriteCategoryPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            this.loaded = true;
            if (this.Category != null)
                this.Category.PropertyChanged += this.category_PropertyChanged;
            this.set(this.Category, this.IsLabelVisible);
        }

        private void favoriteCategoryPresenter_Unloaded(object sender, RoutedEventArgs e)
        {
            this.loaded = false;
            if (this.Category != null)
                this.Category.PropertyChanged -= this.category_PropertyChanged;
        }

        public bool IsLabelVisible
        {
            get => (bool)GetValue(IsLabelVisibleProperty); set => SetValue(IsLabelVisibleProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsLabelVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLabelVisibleProperty =
            DependencyProperty.Register("IsLabelVisible", typeof(bool), typeof(FavoriteCategoryPresenter), new PropertyMetadata(false, isLabelVisiblePropertyChanged));

        private static void isLabelVisiblePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var o = (bool)e.OldValue;
            var n = (bool)e.NewValue;
            if (o == n)
            {
                return;
            }

            var dp = (FavoriteCategoryPresenter)sender;
            dp.set(dp.Category, n);
        }
    }
}
