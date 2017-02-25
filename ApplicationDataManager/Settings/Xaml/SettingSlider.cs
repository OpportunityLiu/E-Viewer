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
            get
            {
                return (SettingInfo)GetValue(SettingValueProperty);
            }
            set
            {
                SetValue(SettingValueProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for SettingValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SettingValueProperty =
            DependencyProperty.Register("SettingValue", typeof(SettingInfo), typeof(SettingSlider), new PropertyMetadata(null, SettingValueChangedCallback));

        private static void SettingValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var s = (SettingSlider)d;
            s.ValueChanged -= s.ValueChangedCallback;
            if(e.OldValue != null)
            {
                ((SettingInfo)e.OldValue).PropertyChanged -= s.SettingInfoPropertyChanged;
            }
            var sv = e.NewValue as SettingInfo;
            if(sv != null)
            {
                var min = Convert.ToDouble(sv.Range.Min);
                var max = Convert.ToDouble(sv.Range.Max);

                s.Minimum = min;
                s.Maximum = max;
                s.Value = Convert.ToDouble(sv.Value);

                var small = (max - min) / 100;
                var large = small * 10;

                if(!double.IsNaN(sv.Range.Small))
                    small = sv.Range.Small;
                if(!double.IsNaN(sv.Range.Large))
                    large = sv.Range.Large;

                s.SmallChange = small;
                s.LargeChange = large;
                s.StepFrequency = small;

                if(!double.IsNaN(sv.Range.Tick))
                    s.TickFrequency = sv.Range.Tick;
                else
                    s.ClearValue(TickFrequencyProperty);

                s.ValueChanged += s.ValueChangedCallback;
                sv.PropertyChanged += s.SettingInfoPropertyChanged;
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

        private void SettingInfoPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName != nameof(Value))
                return;
            var s = (SettingInfo)sender;
            this.Value = Convert.ToDouble(s.Value);
        }

        private void ValueChangedCallback(object sender, RangeBaseValueChangedEventArgs e)
        {
            var s = (SettingSlider)sender;
            s.SettingValue.Value = ConvertToBack(e.NewValue, s.SettingValue.Type);
        }

        public static object ConvertToBack(double value, SettingType parameter)
        {
            Type targetType = null;
            switch(parameter)
            {
            case SettingType.Int32:
                targetType = typeof(int);
                break;
            case SettingType.Int64:
                targetType = typeof(long);
                break;
            case SettingType.Single:
                targetType = typeof(float);
                break;
            case SettingType.Double:
                targetType = typeof(double);
                break;
            }
            return Convert.ChangeType(value, targetType);
        }
    }
}
