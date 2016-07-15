using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ExViewer.Converters
{
    [Windows.UI.Xaml.Markup.ContentProperty(Name = nameof(InnerConverter))]
    public class NumberOffsetConverter : ChainConverter
    {
        public double Offset
        {
            get { return (double)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Offset.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof(double), typeof(NumberOffsetConverter), new PropertyMetadata(0d, OffsetPropertyChangedCallback));

        private static void OffsetPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(double.IsNaN((double)e.NewValue))
                throw new ArgumentOutOfRangeException(nameof(Offset));
        }

        protected override object ConvertBackImpl(object value, Type targetType, object parameter, string language)
        {
            var v = System.Convert.ToDouble(value);
            return SystemConverter.ChangeType(v - Offset, targetType);
        }

        protected override object ConvertImpl(object value, Type targetType, object parameter, string language)
        {
            var v = System.Convert.ToDouble(value);
            return SystemConverter.ChangeType(v + Offset, targetType);
        }
    }
}
