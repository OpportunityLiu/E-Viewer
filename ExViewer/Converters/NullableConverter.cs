using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExViewer.Converters
{
    [Windows.UI.Xaml.Markup.ContentProperty(Name = nameof(InnerConverter))]
    public class NullableConverter : ChainConverter
    {
        protected override object ConvertBackImpl(object value, Type targetType, object parameter, string language)
        {
            return convert(value, targetType, parameter, language);
        }

        protected override object ConvertImpl(object value, Type targetType, object parameter, string language)
        {
            return convert(value, targetType, parameter, language);
        }

        private static Type openNullableType = typeof(Nullable<>);
        private static Dictionary<Type, MethodInfo> getDefaultMethodCache = new Dictionary<Type, MethodInfo>();

        private object convert(object value, Type targetType, object parameter, string language)
        {
            var nullableInner = Nullable.GetUnderlyingType(targetType);
            if(nullableInner == null)
            {
                // target is not Nullable
                var m = getDefaultMethod(targetType);
                return m.Invoke(value, null);
            }
            else
            {
                // target is Nullable
                return Activator.CreateInstance(targetType, value);
            }
        }

        private static MethodInfo getDefaultMethod(Type underlyingType)
        {
            var method = (MethodInfo)null;
            if(!getDefaultMethodCache.TryGetValue(underlyingType, out method))
            {
                var nullableType = openNullableType.MakeGenericType(underlyingType);
                method = nullableType.GetMethod("GetValueOrDefault", Type.EmptyTypes);
                getDefaultMethodCache[underlyingType] = method;
            }
            return method;
        }
    }
}
