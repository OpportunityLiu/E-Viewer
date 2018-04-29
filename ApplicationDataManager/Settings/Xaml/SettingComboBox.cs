using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ApplicationDataManager.Settings.Xaml
{
    class SettingComboBox : ComboBox
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

        private Type settingType;

        // Using a DependencyProperty as the backing store for SettingValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SettingValueProperty =
            DependencyProperty.Register("SettingValue", typeof(SettingInfo), typeof(SettingComboBox), new PropertyMetadata(null, SettingValueChangedCallback));

        private static void SettingValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var s = (SettingComboBox)d;
            var ov = (SettingInfo)e.OldValue;
            var nv = (SettingInfo)e.NewValue;
            if(ov != null)
            {
                ov.PropertyChanged -= s.settingInfoPropertyChanged;
            }
            s.SelectionChanged -= s.selectionChangedCallback;
            if(nv!=null)
            {
                s.settingType = nv.PropertyInfo.PropertyType;
                var selections = Enum.GetNames(s.settingType)
                    .Select(name =>
                        new EnumSelection(name, ((EnumRepresentAttribute)nv.ValueRepresent).GetFriendlyNameOf(name)))
                    .ToList();
                s.ItemsSource = selections;
                s.SelectedItem = selections.Single(sel => sel.EnumName == nv.Value.ToString());
                s.SelectionChanged += s.selectionChangedCallback;
                nv.PropertyChanged += s.settingInfoPropertyChanged;
            }
            else
            {
                s.ClearValue(SelectedItemProperty);
                s.ClearValue(ItemsSourceProperty);
            }
        }

        private void selectionChangedCallback(object sender, SelectionChangedEventArgs e)
        {
            this.SettingValue.Value = Enum.Parse(this.settingType, ((EnumSelection)this.SelectedItem).EnumName);
        }

        private void settingInfoPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName != nameof(SettingInfo.Value))
            {
                return;
            }

            var s = (SettingInfo)sender;
            this.SelectedItem = ((IEnumerable<EnumSelection>)this.ItemsSource).Single(sel => sel.EnumName == s.Value.ToString());
        }

        private class EnumSelection
        {
            public EnumSelection(string name, string friendlyName)
            {
                this.FriendlyName = friendlyName;
                this.EnumName = name;
            }

            public string FriendlyName
            {
                get;
            }

            public string EnumName
            {
                get;
            }

            public override string ToString()
            {
                return this.FriendlyName;
            }
        }
    }
}
