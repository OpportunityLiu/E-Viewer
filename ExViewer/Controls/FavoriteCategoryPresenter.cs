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
        public FavoriteCategoryPresenter()
        {
            this.DefaultStyleKey = typeof(FavoriteCategoryPresenter);
            this.Loaded += FavoriteCategoryPresenter_Loaded;
            this.Unloaded += FavoriteCategoryPresenter_Unloaded;
        }

        private TextBlock Label;
        private TextBlock Icon;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Label = GetTemplateChild("Label") as TextBlock;
            Icon = GetTemplateChild("Icon") as TextBlock;
            setCategory();
            setLabelVisibility();
        }

        private static SolidColorBrush[] table = new SolidColorBrush[]
        {
            new SolidColorBrush(Color.FromArgb(255, 127, 127, 127)),
            new SolidColorBrush(Color.FromArgb(255, 222,35, 35)),
            new SolidColorBrush(Color.FromArgb(255, 222, 116,35)),
            new SolidColorBrush(Color.FromArgb(255, 222, 212, 35)),
            new SolidColorBrush(Color.FromArgb(255, 35, 222, 79)),
            new SolidColorBrush(Color.FromArgb(255, 147, 222, 35)),
            new SolidColorBrush(Color.FromArgb(255, 35, 221, 222)),
            new SolidColorBrush(Color.FromArgb(255, 47,35, 222)),
            new SolidColorBrush(Color.FromArgb(255, 131,35, 222)),
            new SolidColorBrush(Color.FromArgb(255, 222, 35, 165))
        };

        private void setCategory()
        {
            var cat = Category;
            if(cat == null || cat.Index < 0)
            {
                cat = FavoriteCategory.All;
                if(Icon != null)
                    Icon.Visibility = Visibility.Collapsed;
            }
            else
            {
                if(Icon != null)
                {
                    Icon.Visibility = Visibility.Visible;
                    Icon.Foreground = table[cat.Index];
                }
            }
            if(Label != null)
                Label.Text = cat.Name ?? "";
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
            dp.setCategory();
            if(n == null || n.Index < 0)
            {
                dp.ClearValue(CategoryProperty);
                return;
            }
        }

        private void Category_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            setCategory();
        }

        private void FavoriteCategoryPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            loaded = true;
            Category.PropertyChanged += Category_PropertyChanged;
            setCategory();
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
            DependencyProperty.Register("IsLabelVisible", typeof(bool), typeof(FavoriteCategoryPresenter), new PropertyMetadata(true, IsLabelVisiblePropertyChanged));

        private static void IsLabelVisiblePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var dp = (FavoriteCategoryPresenter)sender;
            dp.setLabelVisibility();
        }

        private void setLabelVisibility()
        {
            if(Label != null)
                Label.Visibility = IsLabelVisible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
