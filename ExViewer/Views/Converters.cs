using ExClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Reflection;
using ExViewer.Settings;
using ExViewer.ViewModels;
using Windows.UI;
using System.Globalization;
using Windows.UI.Xaml.Markup;
using MathExpression;

namespace ExViewer.Views
{
    [ContentProperty(Name = "Raw")]
    public class InvertConverter : IValueConverter
    {
        public IValueConverter Raw
        {
            get;
            set;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Raw.ConvertBack(value, targetType, parameter, language);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Raw.Convert(value, targetType, parameter, language);
        }
    }

    [ContentProperty(Name = "InnerConverter")]
    public abstract class ValueConverterChain : IValueConverter
    {
        public IValueConverter InnerConverter
        {
            get; set;
        }

        public abstract object ConvertImplementation(object value, Type targetType, object parameter, string language);
        public abstract object ConvertBackImplementation(object value, Type targetType, object parameter, string language);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var convertedByThis = ConvertImplementation(value, targetType, parameter, language);
            if(InnerConverter == null)
                return convertedByThis;
            else
                return InnerConverter.Convert(convertedByThis, targetType, parameter, language);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            object convertedByInner;
            if(InnerConverter != null)
                convertedByInner = InnerConverter.ConvertBack(value, targetType, parameter, language);
            else
                convertedByInner = value;
            return ConvertBackImplementation(convertedByInner, targetType, parameter, language);
        }
    }

    public class EmptyConverter : ValueConverterChain
    {
        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            return convert(value, targetType, parameter, language);
        }

        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            return convert(value, targetType, parameter, language);
        }

        private object convert(object value, Type targetType, object parameter, string language)
        {
            if(value != null)
                return value;
            if(targetType.GetTypeInfo().IsValueType)
                return Activator.CreateInstance(targetType);
            else
                return null;
        }
    }

    public class DefaultConverter : ValueConverterChain
    {
        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            return System.Convert.ChangeType(value, targetType);
        }

        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            return System.Convert.ChangeType(value, targetType);
        }
    }

    public class LoadStateToVisualStateConverter : ValueConverterChain
    {
        private static Brush accent;

        public static Brush AccentBrush
        {
            get
            {
                return System.Threading.LazyInitializer.EnsureInitialized(ref accent, () => (Brush)Application.Current.Resources["SystemControlForegroundAccentBrush"]);
            }
        }

        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            var state = (ExClient.ImageLoadingState)value;
            if(targetType == typeof(Visibility))
            {
                if(state == ExClient.ImageLoadingState.Loaded)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
            if(targetType == typeof(Brush))
            {
                if(state == ExClient.ImageLoadingState.Failed)
                    return new SolidColorBrush(Windows.UI.Colors.Red);
                else
                    return AccentBrush;
            }
            if(targetType == typeof(bool))
            {
                if(state == ExClient.ImageLoadingState.Waiting || state == ExClient.ImageLoadingState.Preparing)
                    return true;
                else
                    return false;
            }
            throw new NotImplementedException();
        }

        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class StringConverter : ValueConverterChain
    {
        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            return string.Format(CultureInfo.CurrentCulture, parameter.ToString(), value);
        }

        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var currentType = value.GetType();
            if(currentType == typeof(DateTimeOffset))
            {
                var date = (DateTimeOffset)value;
                return date.LocalDateTime.ToString(CultureInfo.CurrentCulture);
            }
            else if(currentType == typeof(DateTime))
            {
                var date = (DateTime)value;
                return date.ToString(CultureInfo.CurrentCulture);
            }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if(targetType == typeof(DateTimeOffset))
                return DateTimeOffset.Parse(value.ToString());
            else if(targetType == typeof(DateTime))
                return DateTime.Parse(value.ToString());
            throw new NotImplementedException();
        }
    }

    public class BooleanToVisibilityConverter : ValueConverterChain
    {
        public bool BooleanForVisible
        {
            get;
            set;
        } = true;

        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            var v = (bool)value;
            if(v == BooleanForVisible)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            return ((Visibility)value == Visibility.Visible) ? BooleanForVisible : !BooleanForVisible;
        }
    }

    public class ObjectToBooleanConverter : ValueConverterChain
    {
        public object ValueForTrue
        {
            get;
            set;
        }

        public object ValueForFalse
        {
            get;
            set;
        }

        /// <summary>
        /// Returns when <c>value != ValueForTrue && value != ValueForFalse</c>.
        /// </summary>
        public bool Others
        {
            get;
            set;
        }

        /// <summary>
        /// Returns when <c>value == ValueForTrue && value == ValueForFalse</c>.
        /// </summary>
        public bool Default
        {
            get;
            set;
        }

        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            if(Equals(value, ValueForTrue) && Equals(value, ValueForFalse))
                return Default;
            if(Equals(value, ValueForTrue))
                return true;
            if(Equals(value, ValueForFalse))
                return false;
            return Others;
        }

        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            var v = (bool)value;
            if(v)
                return ValueForTrue;
            else
                return ValueForFalse;
        }
    }

    public class EnumToBooleanConverter : ValueConverterChain
    {
        public object ValueForTrue
        {
            get;
            set;
        }

        public object ValueForFalse
        {
            get;
            set;
        }

        /// <summary>
        /// Returns when <c>value != ValueForTrue && value != ValueForFalse</c>.
        /// </summary>
        public bool Others
        {
            get;
            set;
        }

        private Type enumBaseType;

        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            if(enumBaseType == null)
            {
                enumBaseType = ValueForTrue?.GetType() ?? ValueForFalse?.GetType();
            }
            var v = System.Convert.ChangeType(value, enumBaseType);
            if(Equals(v, ValueForTrue))
                return true;
            if(Equals(v, ValueForFalse))
                return false;
            return Others;
        }

        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            var v = (bool)value;
            if(v)
                return ValueForTrue;
            else
                return ValueForFalse;
        }
    }

    public class LengthConverter : ValueConverterChain
    {
        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            var v = value as double?;
            if(v == null)
            {
                if(value is GridLength)
                {
                    var v2 = (GridLength)value;
                    if(v2.IsAbsolute)
                        v = v2.Value;
                    else
                        v = double.NaN;
                }
            }

            if(v == null)
                throw new InvalidOperationException();
            var d = v.Value;
            if(targetType == typeof(double))
                return d;
            if(targetType == typeof(GridLength))
                return new GridLength(d);
            throw new InvalidOperationException();
        }

        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            return ConvertBackImplementation(value, targetType, parameter, language);
        }
    }

    public class RateStringConverter : ValueConverterChain
    {
        const char halfL = '\xE7C6';
        const char full = '\xE00A';

        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            var rating = ((double)value) * 2;
            var x = (int)Math.Round(rating);
            var fullCount = x / 2;
            var halfCount = x - 2 * fullCount;
            return new string(full, fullCount) + new string(halfL, halfCount);
        }

        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class NullableConverter : ValueConverterChain
    {
        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            return convert(value, targetType, parameter, language);
        }

        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            return convert(value, targetType, parameter, language);
        }

        static Type NullableType = typeof(Nullable<>);

        private object convert(object value, Type targetType, object parameter, string language)
        {
            var nullableInner = Nullable.GetUnderlyingType(targetType);
            if(nullableInner == null)
            {
                // target is not Nullable
                var nullableType = NullableType.MakeGenericType(targetType);
                var m = nullableType.GetMethod("GetValueOrDefault", new Type[0]);
                return m.Invoke(value, null);
            }
            else
            {
                // target is Nullable
                return Activator.CreateInstance(targetType, value);
            }
        }
    }

    public class LogicalNotConverter : ValueConverterChain
    {
        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            return convert(value, targetType, parameter, language);
        }

        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            return convert(value, targetType, parameter, language);
        }

        private static object convert(object value, Type targetType, object parameter, string language)
        {
            var v = (bool)value;
            return !v;
        }
    }


    public class EnumToStringConverter : ValueConverterChain
    {
        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            return Enum.Parse(targetType, value.ToString());
        }

        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            return Enum.GetName(value.GetType(), value);
        }
    }

    public class GalleryToTitleStringConverter : ValueConverterChain
    {
        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            var g = value as Gallery;
            if(g == null)
                return "";
            return g.GetDisplayTitle();
        }
    }

    public class RangeToBooleanConverter : ValueConverterChain
    {
        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            var v = System.Convert.ToDouble(value);
            if(v >= Min && v < Max)
                return ResultIfInRange;
            else
                return !ResultIfInRange;
        }

        public double Min
        {
            get;
            set;
        }

        public double Max
        {
            get;
            set;
        }

        public bool ResultIfInRange
        {
            get;
            set;
        } = true;
    }

    public class OperationStateToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var v = (OperationState)value;
            switch(v)
            {
            case OperationState.NotStarted:
                return new SolidColorBrush(Colors.Transparent);
            case OperationState.Started:
                return (SolidColorBrush)Application.Current.Resources["SystemControlHighlightAccentBrush"];
            case OperationState.Failed:
                return new SolidColorBrush(Colors.Red);
            case OperationState.Completed:
                return new SolidColorBrush(Colors.Green);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class MathFuctionConverter : ValueConverterChain
    {
        public string Function
        {
            get
            {
                return function;
            }
            set
            {
                function = value;
                parsed = Parser.Parse1(function);
            }
        }

        private string function;

        private ParseResult<Func<double, double>> parsed;

        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            var v = System.Convert.ToDouble(value);
            return System.Convert.ChangeType(parsed.Compiled(v), targetType);
        }
    }

    public class DoubleOffsetConverter : ValueConverterChain
    {
        public double Offset
        {
            get;
            set;
        }

        public override object ConvertBackImplementation(object value, Type targetType, object parameter, string language)
        {
            return (double)value - Offset;
        }

        public override object ConvertImplementation(object value, Type targetType, object parameter, string language)
        {
            return (double)value + Offset;
        }
    }
}
