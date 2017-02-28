using GalaSoft.MvvmLight;
using System;
using System.Linq;
using System.Reflection;

namespace ApplicationDataManager.Settings
{
    public enum ValueType
    {
        Unknown,
        Int32,
        Int64,
        Single,
        Double,
        String,
        Enum,
        BooleanCheckBox,
        BooleanToggleSwitch,
        Custom
    }

    public sealed class SettingInfo : ObservableObject
    {
        internal SettingInfo(PropertyInfo info, ApplicationSettingCollection settingCollection, SettingAttribute settingAttribute)
        {
            this.PropertyInfo = info;
            this.Info = settingAttribute;
            this.Name = info.Name;
            this.FriendlyName = StringLoader.GetString(this.Name);

            this.ValueRepresent = info.GetCustomAttribute<ValueRepresentAttribute>();

            var pType = info.PropertyType;
            if(this.ValueRepresent == null)
            {
                if(pType == typeof(float))
                    this.type = ValueType.Single;
                else if(pType == typeof(double))
                    this.type = ValueType.Double;
                else if(pType == typeof(int))
                    this.type = ValueType.Int32;
                else if(pType == typeof(long))
                    this.type = ValueType.Int64;
                else if(pType == typeof(bool))
                    this.ValueRepresent = ToggleSwitchRepresentAttribute.Default;
                else if(pType == typeof(string))
                    this.type = ValueType.String;
                else if(pType.GetTypeInfo().IsEnum)
                    this.ValueRepresent = EnumRepresentAttribute.Default;
                else
                    throw new InvalidOperationException($"Unsupported property type: {{{pType}}}");
            }
            settingCollection.PropertyChanged += this.settingsChanged;
            this.settingCollection = settingCollection;
        }

        private ApplicationSettingCollection settingCollection;

        private void settingsChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == this.Name)
                RaisePropertyChanged(nameof(Value));
        }

        internal PropertyInfo PropertyInfo { get; }

        internal SettingAttribute Info { get; }

        public string Category => this.Info.Category;

        public string Name { get; }

        public string FriendlyName { get; }

        private ValueType type;

        public ValueType Type => this.ValueRepresent?.TargetType ?? this.type;

        public int Index => this.Info.Index;

        public ValueRepresentAttribute ValueRepresent { get; }

        public object Value
        {
            get => this.PropertyInfo.GetValue(this.settingCollection);
            set
            {
                if(value == null)
                    return;
                this.PropertyInfo.SetValue(this.settingCollection, value);
            }
        }
    }
}
