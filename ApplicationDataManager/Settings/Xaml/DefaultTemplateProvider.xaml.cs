using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ApplicationDataManager.Settings.Xaml
{
    internal sealed partial class DefaultTemplateProvider : ResourceDictionary, IPrimitiveTypeTemplateProvider
    {
        public DefaultTemplateProvider()
        {
            InitializeComponent();
        }

        public static DefaultTemplateProvider Default
        {
            get;
        } = new DefaultTemplateProvider();

        public DataTemplate GetTemplateOf(ValueType type)
        {
            switch (type)
            {
            case ValueType.Int32:
            case ValueType.Int64:
            case ValueType.Single:
            case ValueType.Double:
                return Number;
            case ValueType.String:
                return String;
            case ValueType.Enum:
                return Enum;
            case ValueType.BooleanCheckBox:
                return BooleanCheckBox;
            case ValueType.BooleanToggleSwitch:
                return BooleanToggleSwitch;
            default:
                return null;
            }
        }
    }

    internal sealed class InputScopeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var t = value as Type;
            if (t == typeof(int)
                || t == typeof(long)
                || t == typeof(float)
                || t == typeof(double))
            {
                return new InputScope
                {
                    Names =
                    {
                        new InputScopeName(InputScopeNameValue.Number)
                    }
                };
            }
            else
            {
                return new InputScope
                {
                    Names =
                    {
                        new InputScopeName(InputScopeNameValue.AlphanumericHalfWidth),
                        new InputScopeName(InputScopeNameValue.ChineseHalfWidth),
                        new InputScopeName(InputScopeNameValue.KatakanaHalfWidth),
                        new InputScopeName(InputScopeNameValue.HangulHalfWidth),
                    }
                };
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }

    internal sealed class DefaultConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
            => System.Convert.ChangeType(value, targetType);
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => System.Convert.ChangeType(value, targetType);
    }
}
