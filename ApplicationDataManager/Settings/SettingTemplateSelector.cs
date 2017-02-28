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
            if(i.Type == ValueType.Custom)
                return (DataTemplate)this.CustomTemplateDictionary[((CustomTemplateAttribute)i.ValueRepresent).TemplateName];
            else
                return this.PrimitiveTypeTemplateProvider.GetTemplateOf(i.Type) ?? base.SelectTemplateCore(item);
        }
    }
}
