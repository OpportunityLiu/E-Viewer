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
        Enum
    }

    public class SettingInfo
    {
        internal SettingInfo(PropertyInfo info)
        {
            PropertyInfo = info;
            var setting = info.GetCustomAttribute<SettingAttribute>();
            Name = info.Name;
            FriendlyName = setting.FriendlyName;
            Category = setting.Category;
            Index = setting.Index;
            Range = info.GetCustomAttributes().Select(a => a as IValueRange).SingleOrDefault(a => a != null);

            var type = info.PropertyType;
            if(type == typeof(float))
                Type = SettingType.Single;
            else if(type == typeof(double))
                Type = SettingType.Double;
            else if(type == typeof(int))
                Type = SettingType.Int32;
            else if(type == typeof(long))
                Type = SettingType.Int64;
            else if(type == typeof(string))
                Type = SettingType.String;
            else if(type.GetTypeInfo().IsEnum)
                Type = SettingType.Enum;
        }

        public PropertyInfo PropertyInfo
        {
            get;
            private set;
        }

        public string Category
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public string FriendlyName
        {
            get;
            private set;
        }

        public SettingType Type
        {
            get;
            private set;
        }

        public int Index
        {
            get;
            private set;
        }

        public IValueRange Range
        {
            get;
            private set;
        }

        public object Value
        {
            get
            {
                return PropertyInfo.GetValue(Setting.Current);
            }
            set
            {
                if(value == null)
                    return;
                PropertyInfo.SetValue(Setting.Current, value);
            }
        }
    }
}
