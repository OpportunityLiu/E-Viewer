using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExViewer.Settings
{
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class TestingValueAttribute : Attribute
    {
        readonly object value;

        public TestingValueAttribute(object value)
        {
            this.value = value;
        }

        public object Value
        {
            get
            {
                return value;
            }
        }
    }

    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
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

        public string Category => LocalizedStrings.Settings.GetString(CategoryNameKey);

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
    }

    public interface IValueRange<T> : IValueRange where T : struct
    {
        new T Min
        {
            get;
        }

        new T Max
        {
            get;
        }
    }

    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class Int32RangeAttribute : Attribute, IValueRange<int>
    {
        private readonly int min, max;

        public Int32RangeAttribute(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public int Min => min;

        public int Max => max;

        object IValueRange.Min => min;

        object IValueRange.Max => max;

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

    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class Int64RangeAttribute : Attribute, IValueRange<long>
    {
        private readonly long min, max;

        public Int64RangeAttribute(long min, long max)
        {
            this.min = min;
            this.max = max;
        }

        public long Min => min;

        public long Max => max;

        object IValueRange.Min => min;

        object IValueRange.Max => max;

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

    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class DoubleRangeAttribute : Attribute, IValueRange<double>
    {
        private readonly double min, max;

        public DoubleRangeAttribute(double min, double max)
        {
            this.min = min;
            this.max = max;
        }

        public double Min => min;

        public double Max => max;

        object IValueRange.Min => min;

        object IValueRange.Max => max;

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

    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class SingleRangeAttribute : Attribute, IValueRange<float>
    {
        private readonly float min, max;

        public SingleRangeAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public float Min => min;

        public float Max => max;

        object IValueRange.Min => min;

        object IValueRange.Max => max;

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

    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
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

        public string TrueString => LocalizedStrings.Settings.GetString(TrueStringKey);

        public string FalseString => LocalizedStrings.Settings.GetString(FalseStringKey);
    }

    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
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
            return LocalizedStrings.Settings.GetString(ResourcePrefix + name);
        }

        public static EnumRepresentAttribute Default
        {
            get;
        } = new EnumRepresentAttribute(null);
    }
}
