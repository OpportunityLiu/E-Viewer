using Windows.UI.Xaml;

namespace ApplicationDataManager.Settings
{
    public interface IPrimitiveTypeTemplateProvider
    {
        DataTemplate StringTemplate
        {
            get;
        }

        DataTemplate NumberTemplate
        {
            get;
        }

        DataTemplate EnumTemplate
        {
            get;
        }

        DataTemplate BooleanTemplate
        {
            get;
        }
    }
}
