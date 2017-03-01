using ExClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.ComponentModel;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Controls
{
    public sealed partial class CategorySelector : UserControl
    {
        public CategorySelector()
        {
            this.InitializeComponent();
            this.filter = new List<FilterRecord>()
            {
                new FilterRecord(Category.Doujinshi,true),
                new FilterRecord(Category.Manga, true),
                new FilterRecord(Category.ArtistCG, true),
                new FilterRecord(Category.GameCG, true),
                new FilterRecord(Category.Western, true),
                new FilterRecord(Category.NonH, true),
                new FilterRecord(Category.ImageSet, true),
                new FilterRecord(Category.Cosplay, true),
                new FilterRecord(Category.AsianPorn, true),
                new FilterRecord(Category.Misc, true)
            };
            foreach(var item in this.filter)
            {
                item.PropertyChanged += this.filterItem_PropertyChanged;
            }
        }

        private void filterItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var category = Category.Unspecified;
            foreach(var item in this.filter)
            {
                if(item.IsChecked)
                    category |= item.Category;
            }
            this.SelectedCategory = category;
        }

        public Category SelectedCategory
        {
            get
            {
                return (Category)GetValue(SelectedCategoryProperty);
            }
            set
            {
                SetValue(SelectedCategoryProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for SelectedCategory.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedCategoryProperty =
            DependencyProperty.Register("SelectedCategory", typeof(Category), typeof(CategorySelector), new PropertyMetadata(Category.All, selectedCategoryPropertyChangedCallback));

        private static void selectedCategoryPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var s = (CategorySelector)d;
            var oldValue = (Category)e.OldValue;
            var newValue = (Category)e.NewValue;
            if(oldValue == newValue)
                return;
            foreach(var item in s.filter)
            {
                item.IsChecked = newValue.HasFlag(item.Category);
            }
        }

        private List<FilterRecord> filter;
    }

    internal class FilterRecord : ObservableObject
    {
        public FilterRecord(Category category, bool isChecked)
        {
            this.Category = category;
            this.IsChecked = isChecked;
        }

        public Category Category
        {
            get;
        }

        private bool isChecked;

        public bool IsChecked
        {
            get
            {
                return this.isChecked;
            }
            set
            {
                Set(ref this.isChecked, value);
            }
        }
    }
}
