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
            PropertyInfo = info;
            Info = settingAttribute;
            Name = info.Name;
            FriendlyName = StringLoader.GetString(Name).CoalesceNullOrEmpty(Name);

            ValueRepresent = info.GetCustomAttribute<ValueRepresentAttribute>();

            var pType = info.PropertyType;
            if (ValueRepresent is null)
            {
                if (!typeDic.TryGetValue(pType, out type))
                {
                    if (pType == typeof(bool))
                    {
                        ValueRepresent = ToggleSwitchRepresentAttribute.Default;
                    }
                    else if (pType.GetTypeInfo().IsEnum)
                    {
                        ValueRepresent = EnumRepresentAttribute.Default;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unsupported property type: {{{pType}}}");
                    }
                }
            }
            settingCollection.PropertyChanged += settingsChanged;
            this.settingCollection = settingCollection;
        }

        private readonly ApplicationSettingCollection settingCollection;

        private void settingsChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == Name)
            {
                OnPropertyChanged(nameof(Value));
            }
        }

        internal PropertyInfo PropertyInfo { get; }

        internal SettingAttribute Info { get; }

        public string Category => Info.Category;

        public string Name { get; }

        public string FriendlyName { get; }

        private readonly ValueType type;

        public ValueType Type => ValueRepresent?.TargetType ?? type;

        public int Index => Info.Index;

        public ValueRepresentAttribute ValueRepresent { get; }

        public object Value
        {
            get => PropertyInfo.GetValue(settingCollection);
            set
            {
                if (value is null)
                {
                    return;
                }

                PropertyInfo.SetValue(settingCollection, value);
            }
        }
    }
}
