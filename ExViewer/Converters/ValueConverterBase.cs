using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ExViewer.Converters
{
    [Windows.UI.Xaml.Markup.ContentProperty(Name = nameof(InnerConverter))]
    public abstract class ValueConverterBase : ValueConverter, IValueConverter
    {
        public IValueConverter InnerConverter
        {
            get { return (IValueConverter)GetValue(InnerConverterProperty); }
            set { SetValue(InnerConverterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InnerConverter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InnerConverterProperty =
            DependencyProperty.Register(nameof(InnerConverter), typeof(IValueConverter), typeof(ValueConverterBase), new PropertyMetadata(null, InnerConverterPropertyChangedCallback));

        private static void InnerConverterPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ValueConverterBase)d).OnInnerConverterChanged(e);
        }

        protected virtual void OnInnerConverterChanged(DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
