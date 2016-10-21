using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            get
            {
                return (SettingInfo)GetValue(SettingValueProperty);
            }
            set
            {
                SetValue(SettingValueProperty, value);
            }
        }

        private Type settingType;

        // Using a DependencyProperty as the backing store for SettingValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SettingValueProperty =
            DependencyProperty.Register("SettingValue", typeof(SettingInfo), typeof(SettingComboBox), new PropertyMetadata(null, SettingValueChangedCallback));

        private static void SettingValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var s = (SettingComboBox)d;
            if(e.OldValue != null)
            {
                ((SettingInfo)e.OldValue).PropertyChanged -= s.SettingInfoPropertyChanged;
            }
            s.SelectionChanged -= s.SelectionChangedCallback;
            var sv = e.NewValue as SettingInfo;
            if(sv != null)
            {
                s.settingType = sv.PropertyInfo.PropertyType;
                var selections = Enum.GetNames(s.settingType)
                    .Select(name =>
                        new EnumSelection(name, sv.EnumRepresent.GetFriendlyNameOf(name)))
                    .ToList();
                s.ItemsSource = selections;
                s.SelectedItem = selections.Single(sel => sel.EnumName == sv.Value.ToString());
                s.SelectionChanged += s.SelectionChangedCallback;
                sv.PropertyChanged += s.SettingInfoPropertyChanged;
            }
            else
            {
                s.ClearValue(SelectedItemProperty);
                s.ClearValue(ItemsSourceProperty);
            }
        }

        private void SelectionChangedCallback(object sender, SelectionChangedEventArgs e)
        {
            var s = (SettingComboBox)sender;
            s.SettingValue.Value = Enum.Parse(s.settingType, ((EnumSelection)s.SelectedItem).EnumName);
        }

        private void SettingInfoPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName != nameof(SettingInfo.Value))
                return;
            var s = (SettingInfo)sender;
            this.SelectedItem = ((IEnumerable<EnumSelection>)ItemsSource).Single(sel => sel.EnumName == s.Value.ToString());
        }

        private class EnumSelection
        {
            public EnumSelection(string name, string friendlyName)
            {
                FriendlyName = friendlyName;
                EnumName = name;
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
                return FriendlyName;
            }
        }
    }
}
