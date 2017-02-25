using System;

namespace ApplicationDataManager.Settings
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class SettingAttribute : Attribute
    {
        public SettingAttribute(string categoryNameKey)
        {
            CategoryNameKey = categoryNameKey;
        }

        public string CategoryNameKey
        {
            get;
        }

        public string Category => StringLoader.GetString(CategoryNameKey);

        public int Index
        {
            get; set;
        }

        public string SettingPresenterTemplate
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

        Type ValueType
        {
            get;
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class Int32RangeAttribute : Attribute, IValueRange
    {
        private readonly int min, max;

        public Int32RangeAttribute(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public object Min => min;

        public object Max => max;

        public Type ValueType => typeof(int);

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
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class Int64RangeAttribute : Attribute, IValueRange
    {
        private readonly long min, max;

        public Int64RangeAttribute(long min, long max)
        {
            this.min = min;
            this.max = max;
        }

        public object Min => min;

        public object Max => max;

        public Type ValueType => typeof(long);

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
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class DoubleRangeAttribute : Attribute, IValueRange
    {
        private readonly double min, max;

        public DoubleRangeAttribute(double min, double max)
        {
            this.min = min;
            this.max = max;
        }

        public object Min => min;

        public object Max => max;

        public Type ValueType => typeof(double);

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
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class SingleRangeAttribute : Attribute, IValueRange
    {
        private readonly float min, max;

        public SingleRangeAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public object Min => min;

        public object Max => max;

        public Type ValueType => typeof(float);

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
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class BooleanRepresentAttribute : Attribute
    {
        public static BooleanRepresentAttribute Default
        {
            get;
        } = new BooleanRepresentAttribute("BooleanOn", "BooleanOff");

        public BooleanRepresentAttribute(string trueStringKey, string falseStringKey)
        {
            TrueStringKey = trueStringKey;
            FalseStringKey = falseStringKey;
        }

        public string TrueStringKey
        {
            get;
        }

        public string FalseStringKey
        {
            get;
        }

        public string TrueString => StringLoader.GetString(TrueStringKey);

        public string FalseString => StringLoader.GetString(FalseStringKey);
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class EnumRepresentAttribute : Attribute
    {
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
            if(ReferenceEquals(this, Default))
                return name;
            return StringLoader.GetString(ResourcePrefix + name);
        }

        public static EnumRepresentAttribute Default
        {
            get;
        } = new EnumRepresentAttribute(null);
    }
}
