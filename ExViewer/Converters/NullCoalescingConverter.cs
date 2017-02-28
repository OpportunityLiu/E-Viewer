using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ExViewer.Converters
{
    [Windows.UI.Xaml.Markup.ContentProperty(Name = nameof(Default))]
    public class NullCoalescingConverter : ChainConverter
    {
        public object Default
        {
            get { return GetValue(DefaultProperty); }
            set { SetValue(DefaultProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Default.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultProperty =
            DependencyProperty.Register("Default", typeof(object), typeof(NullCoalescingConverter), new PropertyMetadata(null));

        protected override object ConvertBackImpl(object value, Type targetType, object parameter, string language)
        {
            if(value == this.Default)
                return null;
            return value;
        }

        protected override object ConvertImpl(object value, Type targetType, object parameter, string language)
        {
            if(value == null)
                return this.Default;
            return value;
        }
    }
}
