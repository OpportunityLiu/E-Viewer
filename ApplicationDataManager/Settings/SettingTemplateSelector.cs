using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ApplicationDataManager.Settings
{
    public class SettingTemplateSelector : DataTemplateSelector
    {
        public IPrimitiveTypeTemplateProvider PrimitiveTypeTemplateProvider
        {
            get;
            set;
        } = Xaml.DefaultTemplateProvider.Default;

        public ResourceDictionary CustomTemplateDictionary
        {
            get;
            set;
        }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            var i = (SettingInfo)item;
            switch(i.Type)
            {
            case SettingType.Int32:
            case SettingType.Int64:
            case SettingType.Single:
            case SettingType.Double:
                return PrimitiveTypeTemplateProvider.NumberTemplate;
            case SettingType.String:
                return PrimitiveTypeTemplateProvider.StringTemplate;
            case SettingType.Enum:
                return PrimitiveTypeTemplateProvider.EnumTemplate;
            case SettingType.Boolean:
                return PrimitiveTypeTemplateProvider.BooleanTemplate;
            case SettingType.Custom:
                return (DataTemplate)CustomTemplateDictionary[i.SettingPresenterTemplate];
            default:
                return base.SelectTemplateCore(item);
            }
        }
    }
}
