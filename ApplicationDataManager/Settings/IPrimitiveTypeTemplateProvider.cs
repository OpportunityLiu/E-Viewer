using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
