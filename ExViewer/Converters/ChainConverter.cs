using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ExViewer.Converters
{
    [Windows.UI.Xaml.Markup.ContentProperty(Name = nameof(InnerConverter))]
    public abstract class ChainConverter : ValueConverterBase
    {
        protected abstract object ConvertImpl(object value, Type targetType, object parameter, string language);
        protected abstract object ConvertBackImpl(object value, Type targetType, object parameter, string language);

        public sealed override object Convert(object value, Type targetType, object parameter, string language)
        {
            var convertedByThis = ConvertImpl(value, targetType, parameter, language);
            if(this.InnerConverter == null)
                return convertedByThis;
            else
                return this.InnerConverter.Convert(convertedByThis, targetType, parameter, language);
        }

        public sealed override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            object convertedByInner;
            if(this.InnerConverter != null)
                convertedByInner = this.InnerConverter.ConvertBack(value, targetType, parameter, language);
            else
                convertedByInner = value;
            return ConvertBackImpl(convertedByInner, targetType, parameter, language);
        }
    }
}
