using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using System.Reflection;

namespace ExViewer.Converters
{
    [Windows.UI.Xaml.Markup.ContentProperty(Name = nameof(InnerConverter))]
    public class ObjectToBooleanConverter : ChainConverter
    {
        public object ValueForTrue
        {
            get { return GetValue(ValueForTrueProperty); }
            set { SetValue(ValueForTrueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValueForTrue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueForTrueProperty =
            DependencyProperty.Register("ValueForTrue", typeof(object), typeof(ObjectToBooleanConverter), new PropertyMetadata(null, ValueChangedCallback));

        public object ValueForFalse
        {
            get { return GetValue(ValueForFalseProperty); }
            set { SetValue(ValueForFalseProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValueForFalse.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueForFalseProperty =
            DependencyProperty.Register("ValueForFalse", typeof(object), typeof(ObjectToBooleanConverter), new PropertyMetadata(null, ValueChangedCallback));

        /// <summary>
        /// Returns when <c>value != ValueForTrue && value != ValueForFalse</c>.
        /// </summary>
        public bool Others
        {
            get { return (bool)GetValue(OthersProperty); }
            set { SetValue(OthersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Others.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OthersProperty =
            DependencyProperty.Register("Others", typeof(bool), typeof(ObjectToBooleanConverter), new PropertyMetadata(false));

        /// <summary>
        /// Returns when <c>value == ValueForTrue && value == ValueForFalse</c>.
        /// </summary>
        public bool Default
        {
            get { return (bool)GetValue(DefaultProperty); }
            set { SetValue(DefaultProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Default.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DefaultProperty =
            DependencyProperty.Register("Default", typeof(bool), typeof(ObjectToBooleanConverter), new PropertyMetadata(false));

        private static void ValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var s = (ObjectToBooleanConverter)d;
            var tType = s.ValueForTrue?.GetType();
            var fType = s.ValueForFalse?.GetType();
            if(tType == null && fType == null)
            {
                s.valueType = typeof(object);
                return;
            }
            if(tType == null)
            {
                s.valueType = fType;
                return;
            }
            if(fType == null || tType == fType)
            {
                s.valueType = tType;
                return;
            }
            //FIXME:tType和fType的共同基类
            s.valueType = typeof(object);
        }

        private Type valueType = typeof(object);

        protected override object ConvertImpl(object value, Type targetType, object parameter, string language)
        {
            value = SystemConverter.ChangeType(value, this.valueType);
            var isTrue = Equals(value, this.ValueForTrue);
            var isFalse = Equals(value, this.ValueForFalse);
            if(isTrue && isFalse)
                return this.Default;
            if(isTrue)
                return true;
            if(isFalse)
                return false;
            return this.Others;
        }

        protected override object ConvertBackImpl(object value, Type targetType, object parameter, string language)
        {
            var v = (bool)value;
            if(v)
                return this.ValueForTrue;
            else
                return this.ValueForFalse;
        }
    }
}
