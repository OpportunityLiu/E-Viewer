using ExClient;
using Opportunity.MvvmUniverse;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Controls
{
    public sealed partial class CategorySelector : UserControl
    {
        public CategorySelector()
        {
            InitializeComponent();
            foreach (var item in _Filter)
            {
                item.PropertyChanged += _FilterItem_PropertyChanged;
            }
        }

        private void _FilterItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var category = Category.Unspecified;
            foreach (var item in _Filter)
            {
                if (item.IsChecked)
                    category |= item.Category;
            }
            SelectedCategory = category;
        }

        public Category SelectedCategory
        {
            get => (Category)GetValue(SelectedCategoryProperty);
            set => SetValue(SelectedCategoryProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectedCategory.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedCategoryProperty =
            DependencyProperty.Register("SelectedCategory", typeof(Category), typeof(CategorySelector), new PropertyMetadata(Category.All, _SelectedCategoryPropertyChangedCallback));

        private static void _SelectedCategoryPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var s = (CategorySelector)d;
            var oldValue = (Category)e.OldValue;
            var newValue = (Category)e.NewValue;
            if (oldValue == newValue)
                return;

            foreach (var item in s._Filter)
            {
                item.IsChecked = newValue.HasFlag(item.Category);
            }
        }

        private readonly List<FilterRecord> _Filter = new List<FilterRecord>()
        {
            new FilterRecord(Category.Doujinshi),
            new FilterRecord(Category.Manga),
            new FilterRecord(Category.ArtistCG),
            new FilterRecord(Category.GameCG),
            new FilterRecord(Category.Western),
            new FilterRecord(Category.NonH),
            new FilterRecord(Category.ImageSet),
            new FilterRecord(Category.Cosplay),
            new FilterRecord(Category.AsianPorn),
            new FilterRecord(Category.Misc)
        };
    }

    internal class FilterRecord : ObservableObject
    {
        public FilterRecord(Category category)
        {
            Category = category;
        }

        public Category Category { get; }

        private bool _IsChecked = true;
        public bool IsChecked
        {
            get => _IsChecked;
            set => Set(ref _IsChecked, value);
        }
    }
}
