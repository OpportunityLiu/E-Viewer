using ExClient;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ExViewer.Controls
{
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
            this.Icon = null;
            this.Label = null;
            set();
        }

        private TextBlock Icon, Label;

        private void set()
        {
            var cat = getCategory(this.Category);
            var labelVisibility = this.IsLabelVisible;
            setIcon(cat);
            setLabel(cat, labelVisibility);
        }

        private static FavoriteCategory getCategory(FavoriteCategory category)
        {
            return category ?? FavoriteCategory.All;
        }

        private void setIcon(FavoriteCategory category)
        {
            if(category.Index>=0)
            {
                var icon = this.Icon;
                if(icon == null)
                {
                    icon = GetTemplateChild("Icon") as TextBlock;
                    if(icon == null)
                        return;
                    else
                        this.Icon = icon;
                }
                icon.Visibility = Visibility.Visible;
                icon.Foreground = category.GetThemeBrush();
            }
            else
            {
                if(this.Icon == null)
                    return;
                this.Icon.Visibility = Visibility.Collapsed;
            }
        }

        private void setLabel(FavoriteCategory category, bool labelVisible)
        {
            if(labelVisible)
            {
                var label = this.Label;
                if(label == null)
                {
                    label = GetTemplateChild("Label") as TextBlock;
                    if(label == null)
                        return;
                    else
                        this.Label = label;
                }
                label.Visibility = Visibility.Visible;
                label.Text = category.Name ?? "";
            }
            else
            {
                if(this.Label == null)
                    return;
                this.Label.Visibility = Visibility.Collapsed;
            }
        }

        private void setLabel(FavoriteCategory category)
        {
            if(!this.IsLabelVisible)
                return;
            setLabel(category, true);
        }

        public FavoriteCategory Category
        {
            get => (FavoriteCategory)GetValue(CategoryProperty); set => SetValue(CategoryProperty, value);
        }

        // Using a DependencyProperty as the backing store for Category.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CategoryProperty =
            DependencyProperty.Register("Category", typeof(FavoriteCategory), typeof(FavoriteCategoryPresenter), new PropertyMetadata(FavoriteCategory.All, categoryPropertyChanged));

        private bool loaded;

        private static void categoryPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var dp = (FavoriteCategoryPresenter)sender;
            var o = (FavoriteCategory)e.OldValue;
            var n = (FavoriteCategory)e.NewValue;
            if(o != null)
                o.PropertyChanged -= dp.category_PropertyChanged;
            if(dp.loaded && n != null)
                n.PropertyChanged += dp.category_PropertyChanged;
            var cat = getCategory(dp.Category);
            dp.setIcon(cat);
            dp.setLabel(cat);
            if(n == null)
            {
                dp.ClearValue(CategoryProperty);
                return;
            }
        }

        private void category_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            setLabel(getCategory(this.Category));
        }

        private void favoriteCategoryPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            this.loaded = true;
            this.Category.PropertyChanged += this.category_PropertyChanged;
            set();
        }

        private void favoriteCategoryPresenter_Unloaded(object sender, RoutedEventArgs e)
        {
            this.loaded = false;
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
            var dp = (FavoriteCategoryPresenter)sender;
            var cat = getCategory(dp.Category);
            dp.setLabel(cat, (bool)e.NewValue);
        }
    }
}
