using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace ApplicationDataManager.Settings.Xaml
{
    class SettingSlider : Slider
    {
        protected override void OnDisconnectVisualChildren()
        {
            base.OnDisconnectVisualChildren();
            ClearValue(SettingValueProperty);
        }

        public SettingInfo SettingValue
        {
            get => (SettingInfo)GetValue(SettingValueProperty);
            set => SetValue(SettingValueProperty, value);
        }

        // Using a DependencyProperty as the backing store for SettingValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SettingValueProperty =
            DependencyProperty.Register("SettingValue", typeof(SettingInfo), typeof(SettingSlider), new PropertyMetadata(null, settingValueChangedCallback));

        private static void settingValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var s = (SettingSlider)d;
            s.ValueChanged -= s.valueChangedCallback;
            var ov = (SettingInfo)e.OldValue;
            var nv = (SettingInfo)e.NewValue;
            if(ov != null)
            {
                ov.PropertyChanged -= s.settingInfoPropertyChanged;
            }
            if(nv != null)
            {
                var range = (IValueRangeAttribute)nv.ValueRepresent;

                s.Minimum = range.Min;
                s.Maximum = range.Max;
                s.Value = Convert.ToDouble(nv.Value);

                var small = (range.Max - range.Min) / 100;
                var large = small * 10;

                if(!double.IsNaN(range.Small))
                    small = range.Small;

                if (!double.IsNaN(range.Large))
                    large = range.Large;

                s.SmallChange = small;
                s.LargeChange = large;
                s.StepFrequency = small;

                if (!double.IsNaN(range.Tick))
                    s.TickFrequency = range.Tick;
                else
                    s.ClearValue(TickFrequencyProperty);

                s.ValueChanged += s.valueChangedCallback;
                nv.PropertyChanged += s.settingInfoPropertyChanged;
            }
            else
            {
                s.ClearValue(ValueProperty);
                s.ClearValue(MinimumProperty);
                s.ClearValue(MaximumProperty);

                s.ClearValue(TickFrequencyProperty);
                s.ClearValue(SmallChangeProperty);
                s.ClearValue(LargeChangeProperty);
                s.ClearValue(StepFrequencyProperty);
            }
        }

        private void settingInfoPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName != nameof(Value))
            {
                return;
            }

            var s = (SettingInfo)sender;
            this.Value = Convert.ToDouble(s.Value);
        }

        private void valueChangedCallback(object sender, RangeBaseValueChangedEventArgs e)
        {
            this.SettingValue.Value = ConvertToBack(e.NewValue, this.SettingValue.Type);
        }

        public static object ConvertToBack(double value, ValueType parameter)
        {
            switch(parameter)
            {
            case ValueType.Int32:
                return Convert.ToInt32(value);
            case ValueType.Int64:
                return Convert.ToInt64(value);
            case ValueType.Single:
                return Convert.ToSingle(value);
            case ValueType.Double:
                return Convert.ToDouble(value);
            }
            throw new InvalidCastException();
        }
    }
}
