using Windows.UI.Xaml;

namespace ApplicationDataManager.Settings
{
    public interface IPrimitiveTypeTemplateProvider
    {
        DataTemplate GetTemplateOf(ValueType type);
    }
}
