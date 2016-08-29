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
    sealed class SettingAttribute : Attribute
    {
        readonly string category;

        public SettingAttribute(string categoryNameKey)
        {
            this.category = LocalizedStrings.Settings.GetString(categoryNameKey);
        }

        public string Category => category;

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
            TrueString = LocalizedStrings.Settings.GetString(trueStringKey);
            FalseString = LocalizedStrings.Settings.GetString(falseStringKey);
        }

        public string TrueString
        {
            get;
        }

        public string FalseString
        {
            get;
        }
    }
}
