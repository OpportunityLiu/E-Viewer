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

    public interface IValueRange
    {
        object Min
        {
            get;
        }

        object Max
        {
            get;
        }

        double Tick
        {
            get;
            set;
        }

        double Small
        {
            get;
            set;
        }

        double Large
        {
            get;
            set;
        }
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

    public sealed class Int32RangeAttribute : ValueRepresentAttribute, IValueRange
    {
        private readonly int min, max;

        public Int32RangeAttribute(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public object Min => this.min;

        public object Max => this.max;

        public double Tick
        {
            get; set;
        } = double.NaN;

        public double Small
        {
            get; set;
        } = double.NaN;

        public double Large
        {
            get; set;
        } = double.NaN;

        public override ValueType TargetType => ValueType.Int32;
    }

    public sealed class Int64RangeAttribute : ValueRepresentAttribute, IValueRange
    {
        private readonly long min, max;

        public Int64RangeAttribute(long min, long max)
        {
            this.min = min;
            this.max = max;
        }

        public object Min => this.min;

        public object Max => this.max;

        public double Tick
        {
            get; set;
        } = double.NaN;

        public double Small
        {
            get; set;
        } = double.NaN;

        public double Large
        {
            get; set;
        } = double.NaN;

        public override ValueType TargetType => ValueType.Int64;
    }

    public sealed class DoubleRangeAttribute : ValueRepresentAttribute, IValueRange
    {
        private readonly double min, max;

        public DoubleRangeAttribute(double min, double max)
        {
            this.min = min;
            this.max = max;
        }

        public object Min => this.min;

        public object Max => this.max;

        public double Tick
        {
            get; set;
        } = double.NaN;

        public double Small
        {
            get; set;
        } = double.NaN;

        public double Large
        {
            get; set;
        } = double.NaN;

        public override ValueType TargetType => ValueType.Double;
    }

    public sealed class SingleRangeAttribute : ValueRepresentAttribute, IValueRange
    {
        private readonly float min, max;

        public SingleRangeAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public object Min => this.min;

        public object Max => this.max;

        public double Tick
        {
            get; set;
        } = double.NaN;

        public double Small
        {
            get; set;
        } = double.NaN;

        public double Large
        {
            get; set;
        } = double.NaN;

        public override ValueType TargetType => ValueType.Single;
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
