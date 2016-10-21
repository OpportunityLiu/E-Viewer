using ExClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExViewer.Settings
{
    internal static class SettingsHelper
    {
        public static ConnectionStrategy GetStrategy(this SettingCollection @this)
        {
            if(@this.LoadLofiOnAllInternetConnection)
                return ConnectionStrategy.AllLofi;
            if(@this.LoadLofiOnMeteredInternetConnection)
                return ConnectionStrategy.LofiOnMetered;
            return ConnectionStrategy.AllFull;
        }
    }
}
