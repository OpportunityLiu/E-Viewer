using Opportunity.MvvmUniverse;
using System;
using System.Collections.Generic;
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
        private static readonly Dictionary<Type, ValueType> typeDic = new Dictionary<Type, ValueType>
        {
            [typeof(float)] = ValueType.Single,
            [typeof(double)] = ValueType.Double,
            [typeof(int)] = ValueType.Int32,
            [typeof(long)] = ValueType.Int64,
            [typeof(string)] = ValueType.String
        };

        internal SettingInfo(PropertyInfo info, ApplicationSettingCollection settingCollection, SettingAttribute settingAttribute)
        {
            this.PropertyInfo = info;
            this.Info = settingAttribute;
            this.Name = info.Name;
            this.FriendlyName = StringLoader.GetString(this.Name);

            this.ValueRepresent = info.GetCustomAttribute<ValueRepresentAttribute>();

            var pType = info.PropertyType;
            if (this.ValueRepresent is null)
            {
                if (!typeDic.TryGetValue(pType, out this.type))
                {
                    if (pType == typeof(bool))
                    {
                        this.ValueRepresent = ToggleSwitchRepresentAttribute.Default;
                    }
                    else if (pType.GetTypeInfo().IsEnum)
                    {
                        this.ValueRepresent = EnumRepresentAttribute.Default;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unsupported property type: {{{pType}}}");
                    }
                }
            }
            settingCollection.PropertyChanged += this.settingsChanged;
            this.settingCollection = settingCollection;
        }

        private ApplicationSettingCollection settingCollection;

        private void settingsChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == this.Name)
            {
                OnPropertyChanged(nameof(Value));
            }
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
                if (value is null)
                {
                    return;
                }

                this.PropertyInfo.SetValue(this.settingCollection, value);
            }
        }
    }
}
