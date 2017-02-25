using Windows.UI.Xaml;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ApplicationDataManager.Settings.Xaml
{
    internal sealed partial class DefaultTemplateProvider : ResourceDictionary, IPrimitiveTypeTemplateProvider
    {
        public DefaultTemplateProvider()
        {
            this.InitializeComponent();
        }

        public DataTemplate StringTemplate => String;
        public DataTemplate NumberTemplate => Number;
        public DataTemplate EnumTemplate => Enum;
        public DataTemplate BooleanTemplate => Boolean;

        public static DefaultTemplateProvider Default
        {
            get;
        } = new DefaultTemplateProvider();
    }
}
