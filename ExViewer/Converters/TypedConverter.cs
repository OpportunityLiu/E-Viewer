using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ExViewer.Converters
{
    public abstract class TypedConverter<TFrom, TTo> : ValueConverter, IValueConverter
    {
        public sealed override object Convert(object value, Type targetType, object parameter, string language)
            => Convert(ConvertHelper.ChangeType<TFrom>(value), parameter, language);
        public sealed override object ConvertBack(object value, Type targetType, object parameter, string language)
            => ConvertBack(ConvertHelper.ChangeType<TTo>(value), parameter, language);

        public abstract TTo Convert(TFrom value, object parameter, string language);
        public abstract TFrom ConvertBack(TTo value, object parameter, string language);
    }
}
