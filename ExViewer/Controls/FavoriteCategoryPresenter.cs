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
            DefaultStyleKey = typeof(FavoriteCategoryPresenter);
            Loaded += favoriteCategoryPresenter_Loaded;
            Unloaded += favoriteCategoryPresenter_Unloaded;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Icon = (UIElement)GetTemplateChild(nameof(Icon));
            Label = null;
            set(Category, IsLabelVisible);
        }

        private TextBlock Label;
        private UIElement Icon;

        private void set(FavoriteCategory category, bool labelVisible)
        {
            category = category ?? Client.Current.Favorites.All;
            var icon = Icon;
            if (icon != null)
            {
                icon.Visibility = category.Index < 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            BorderBrush = category.GetThemeBrush();
            if (labelVisible)
            {
                var label = Label;
                if (label is null)
                {
                    label = GetTemplateChild("Label") as TextBlock;
                    if (label is null)
                    {
                        return;
                    }
                    else
                    {
                        Label = label;
                    }
                }
                label.Visibility = Visibility.Visible;
                label.Text = category.Name ?? "";
            }
            else
            {
                if (Label is null)
                {
                    return;
                }

                Label.Visibility = Visibility.Collapsed;
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
            set(Category, IsLabelVisible);
        }

        private void favoriteCategoryPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            loaded = true;
            if (Category != null)
                Category.PropertyChanged += category_PropertyChanged;
            set(Category, IsLabelVisible);
        }

        private void favoriteCategoryPresenter_Unloaded(object sender, RoutedEventArgs e)
        {
            loaded = false;
            if (Category != null)
                Category.PropertyChanged -= category_PropertyChanged;
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
