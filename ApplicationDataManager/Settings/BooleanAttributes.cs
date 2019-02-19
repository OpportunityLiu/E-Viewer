using System;

namespace ApplicationDataManager.Settings
{

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public abstract class BooleanRepresentAttribute : ValueRepresentAttribute
    {

    }

    public enum PredefinedToggleSwitchRepresent
    {
        OnOff,
        TrueFalse,
        YesNo,
        EnabledDisabled,
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ToggleSwitchRepresentAttribute : BooleanRepresentAttribute
    {
        public static ToggleSwitchRepresentAttribute Default
        {
            get;
        } = new ToggleSwitchRepresentAttribute(PredefinedToggleSwitchRepresent.OnOff);

        public ToggleSwitchRepresentAttribute(string trueStringKey, string falseStringKey)
        {
            TrueString = StringLoader.GetString(trueStringKey);
            FalseString = StringLoader.GetString(falseStringKey);
        }

        public ToggleSwitchRepresentAttribute(PredefinedToggleSwitchRepresent represent)
        {
            var provider = Strings.Resources.Boolean[represent.ToString()];
            TrueString = provider.GetValue("True");
            FalseString = provider.GetValue("False");
        }

        public string TrueString { get; }

        public string FalseString { get; }

        public override ValueType TargetType => ValueType.BooleanToggleSwitch;
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class CheckBoxRepresentAttribute : BooleanRepresentAttribute
    {
        public CheckBoxRepresentAttribute()
        {
        }

        public override ValueType TargetType => ValueType.BooleanCheckBox;
    }
}