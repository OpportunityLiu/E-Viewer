using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
