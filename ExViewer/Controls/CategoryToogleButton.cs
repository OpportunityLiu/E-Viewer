using ExClient;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace ExViewer.Controls
{
    public sealed class CategoryToggleButton : ToggleButton
    {
        public CategoryToggleButton()
        {
            DefaultStyleKey = typeof(CategoryToggleButton);
        }
        public Category Category
        {
            get => (Category)GetValue(CategoryProperty);
            set => SetValue(CategoryProperty, value);
        }

        // Using a DependencyProperty as the backing store for Category.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CategoryProperty =
            DependencyProperty.Register("Category", typeof(Category), typeof(CategoryToggleButton), new PropertyMetadata(Category.Unspecified, CategoryPropertyChangedCallback));


        private static void CategoryPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
