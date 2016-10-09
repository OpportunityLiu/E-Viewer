﻿using ExClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExViewer.Settings
{
    public enum SettingType
    {
        Unknown,
        Int32,
        Int64,
        Single,
        Double,
        String,
        Enum,
        Boolean,
        Custom
    }

    public class SettingInfo : ObservableObject
    {
        internal SettingInfo(PropertyInfo info)
        {
            PropertyInfo = info;

            var setting = info.GetCustomAttribute<SettingAttribute>();
            Name = info.Name;
            FriendlyName = LocalizedStrings.Settings.GetString(Name);
            Category = setting.Category;
            Index = setting.Index;

            Range = info.GetCustomAttributes().Select(a => a as IValueRange).SingleOrDefault(a => a != null);
            BooleanRepresent = info.GetCustomAttribute<BooleanRepresentAttribute>() ?? BooleanRepresentAttribute.Default;
            EnumRepresent = info.GetCustomAttribute<EnumRepresentAttribute>() ?? EnumRepresentAttribute.Default;

            var type = info.PropertyType;
            if(setting.SettingPresenterTemplate != null)
            {
                Type = SettingType.Custom;
                SettingPresenterTemplate = setting.SettingPresenterTemplate;
            }
            else if(type == typeof(float))
                Type = SettingType.Single;
            else if(type == typeof(double))
                Type = SettingType.Double;
            else if(type == typeof(int))
                Type = SettingType.Int32;
            else if(type == typeof(long))
                Type = SettingType.Int64;
            else if(type == typeof(bool))
                Type = SettingType.Boolean;
            else if(type == typeof(string))
                Type = SettingType.String;
            else if(type.GetTypeInfo().IsEnum)
                Type = SettingType.Enum;
            else
                throw new InvalidOperationException($"Unsupported property type: {{{type}}}");
            SettingCollection.Current.PropertyChanged += SettingsChanged;
        }

        private void SettingsChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == Name)
                RaisePropertyChanged(nameof(Value));
        }

        public PropertyInfo PropertyInfo
        {
            get;
        }

        public string Category
        {
            get;
        }

        public string Name
        {
            get;
        }

        public string FriendlyName
        {
            get;
        }

        public SettingType Type
        {
            get;
        }

        public int Index
        {
            get;
        }

        public IValueRange Range
        {
            get;
        }

        public BooleanRepresentAttribute BooleanRepresent
        {
            get;
        }

        public EnumRepresentAttribute EnumRepresent
        {
            get;
        }

        public string SettingPresenterTemplate
        {
            get;
        }

        public object Value
        {
            get
            {
                return PropertyInfo.GetValue(SettingCollection.Current);
            }
            set
            {
                if(value == null)
                    return;
                PropertyInfo.SetValue(SettingCollection.Current, value);
            }
        }
    }
}
