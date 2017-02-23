using ExClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ExViewer.Controls
{
    public class FavoriteCategoryPresenter : Control
    {
        private static readonly ResourceDictionary favoritesBrushes = getResource();

        private static ResourceDictionary getResource()
        {
            var r = new ResourceDictionary();
            Application.LoadComponent(r, new Uri("ms-appx:///Themes/Favorites.xaml"));
            return r;
        }

        public FavoriteCategoryPresenter()
        {
            this.DefaultStyleKey = typeof(FavoriteCategoryPresenter);
            this.Loaded += FavoriteCategoryPresenter_Loaded;
            this.Unloaded += FavoriteCategoryPresenter_Unloaded;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Icon = null;
            Label = null;
            set();
        }

        private TextBlock Icon, Label;

        private void set()
        {
            var cat = getCategory(Category);
            var labelVisibility = IsLabelVisible;
            setIcon(cat);
            setLabel(cat, labelVisibility);
        }

        private static FavoriteCategory getCategory(FavoriteCategory category)
        {
            if(category == null || category.Index < 0)
                category = FavoriteCategory.All;
            return category;
        }

        private void setIcon(FavoriteCategory category)
        {
            var icon = Icon;
            if(icon == null)
            {
                icon = GetTemplateChild("Icon") as TextBlock;
                if(icon == null)
                    return;
                else
                    Icon = icon;
            }
            if(category.Index < 0)
                icon.Visibility = Visibility.Collapsed;
            else
            {
                icon.Visibility = Visibility.Visible;
                icon.Foreground = (Brush)favoritesBrushes[$"FavoriteCategory{category.Index}"];
            }
        }

        private void setLabel(FavoriteCategory category, bool value)
        {
            if(value)
            {
                var label = Label;
                if(label == null)
                {
                    label = GetTemplateChild("Label") as TextBlock;
                    if(label == null)
                        return;
                    else
                        Label = label;
                }
                label.Visibility = Visibility.Visible;
                label.Text = category.Name ?? "";
            }
            else
            {
                if(Label == null)
                    return;
                Label.Visibility = Visibility.Collapsed;
            }
        }

        private void setLabel(FavoriteCategory category)
        {
            if(!IsLabelVisible)
                return;
            setLabel(category, true);
        }

        public FavoriteCategory Category
        {
            get { return (FavoriteCategory)GetValue(CategoryProperty); }
            set { SetValue(CategoryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Category.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CategoryProperty =
            DependencyProperty.Register("Category", typeof(FavoriteCategory), typeof(FavoriteCategoryPresenter), new PropertyMetadata(FavoriteCategory.All, CategoryPropertyChanged));

        private bool loaded;

        private static void CategoryPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var dp = (FavoriteCategoryPresenter)sender;
            var o = (FavoriteCategory)e.OldValue;
            var n = (FavoriteCategory)e.NewValue;
            if(o != null)
                o.PropertyChanged -= dp.Category_PropertyChanged;
            if(dp.loaded && n != null)
                n.PropertyChanged += dp.Category_PropertyChanged;
            var cat = getCategory(dp.Category);
            dp.setIcon(cat);
            dp.setLabel(cat);
            if(n == null || n.Index < 0)
            {
                dp.ClearValue(CategoryProperty);
                return;
            }
        }

        private void Category_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            setLabel(getCategory(Category));
        }

        private void FavoriteCategoryPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            loaded = true;
            Category.PropertyChanged += Category_PropertyChanged;
            set();
        }

        private void FavoriteCategoryPresenter_Unloaded(object sender, RoutedEventArgs e)
        {
            loaded = false;
            Category.PropertyChanged -= Category_PropertyChanged;
        }

        public bool IsLabelVisible
        {
            get { return (bool)GetValue(IsLabelVisibleProperty); }
            set { SetValue(IsLabelVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsLabelVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLabelVisibleProperty =
            DependencyProperty.Register("IsLabelVisible", typeof(bool), typeof(FavoriteCategoryPresenter), new PropertyMetadata(false, IsLabelVisiblePropertyChanged));

        private static void IsLabelVisiblePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var dp = (FavoriteCategoryPresenter)sender;
            var cat = getCategory(dp.Category);
            dp.setLabel(cat, (bool)e.NewValue);
        }
    }
}
