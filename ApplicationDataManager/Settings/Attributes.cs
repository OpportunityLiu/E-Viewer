using System;

namespace ApplicationDataManager.Settings
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class SettingAttribute : Attribute
    {
        public SettingAttribute(string categoryNameKey)
        {
            this.CategoryNameKey = categoryNameKey;
        }

        public string CategoryNameKey
        {
            get;
        }

        public string Category => StringLoader.GetString(this.CategoryNameKey);

        public int Index
        {
            get; set;
        }
    }

    public interface IValueRangeAttribute
    {
        double Min { get; }

        double Max { get; }

        double Tick { get; }

        double Small { get; }

        double Large { get; }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public abstract class ValueRepresentAttribute : Attribute
    {
        public abstract ValueType TargetType { get; }
    }

    public sealed class CustomTemplateAttribute : ValueRepresentAttribute
    {
        public override ValueType TargetType => ValueType.Custom;

        public CustomTemplateAttribute(string templateName)
        {
            this.TemplateName = templateName;
        }

        public string TemplateName { get; }
    }

    public sealed class TextTemplateAttribute : ValueRepresentAttribute
    {
        public override ValueType TargetType => ValueType.String;

        public TextTemplateAttribute() { }

        public bool MultiLine { get; set; }
    }

    public sealed class RangeAttribute : ValueRepresentAttribute, IValueRangeAttribute
    {
        public RangeAttribute(double min, double max, ValueType targetType)
        {
            if (double.IsNaN(min))
                throw new ArgumentException("min is nan");
            if (double.IsNaN(max))
                throw new ArgumentException("max is nan");
            if (min > max)
                throw new ArgumentException("min > max");

            switch (targetType)
            {
            case ValueType.Int32:
            case ValueType.Int64:
            case ValueType.Single:
            case ValueType.Double:
                TargetType = targetType;
                break;
            default:
                throw new ArgumentException("targetType must be a numeric type.");
            }
            Min = min;
            Max = max;
        }

        public double Min { get; }

        public double Max { get; }

        public double Tick { get; set; } = double.NaN;

        public double Small { get; set; } = double.NaN;

        public double Large { get; set; } = double.NaN;

        public override ValueType TargetType { get; }
    }

    public sealed class EnumRepresentAttribute : ValueRepresentAttribute
    {
        public static EnumRepresentAttribute Default
        {
            get;
        } = new EnumRepresentAttribute(null);

        public EnumRepresentAttribute(string resourcePrefix)
        {
            this.ResourcePrefix = resourcePrefix;
        }

        public string ResourcePrefix
        {
            get;
        }

        public string GetFriendlyNameOf(string name)
        {
            if (ReferenceEquals(this, Default))
            {
                return name;
            }

            return StringLoader.GetString($"{this.ResourcePrefix}/{name}");
        }

        public override ValueType TargetType => ValueType.Enum;
    }
}
