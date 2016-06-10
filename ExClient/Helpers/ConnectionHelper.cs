using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace ExClient
{
    internal static class ConnectionHelper
    {
        public static bool IsLofiRequired(ConnectionStrategy strategy)
        {
            switch(strategy)
            {
            case ConnectionStrategy.AllLofi:
                return true;
            case ConnectionStrategy.LofiOnMetered:
                var netProfile = NetworkInformation.GetInternetConnectionProfile();
                var cost = netProfile.GetConnectionCost();
                bool lofi = false;
                if(cost.NetworkCostType != NetworkCostType.Unrestricted)
                    lofi = true;
                if(cost.OverDataLimit || cost.ApproachingDataLimit)
                    lofi = true;
                return lofi;
            case ConnectionStrategy.AllFull:
                return false;
            }
            throw new ArgumentOutOfRangeException(nameof(strategy));
        }
    }
}
