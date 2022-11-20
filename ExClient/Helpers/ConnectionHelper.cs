using Windows.Networking.Connectivity;

namespace ExClient
{
    internal static class ConnectionHelper
    {
        public static bool IsLofiRequired(ConnectionStrategy strategy)
        {
            switch (strategy)
            {
            case ConnectionStrategy.AllLofi:
                return true;
            case ConnectionStrategy.AllFull:
                return false;
            case ConnectionStrategy.LofiOnMetered:
            default:
                var netProfile = NetworkInformation.GetInternetConnectionProfile();
                var cost = netProfile.GetConnectionCost();
                if (cost.NetworkCostType != NetworkCostType.Unrestricted
                    || cost.OverDataLimit || cost.ApproachingDataLimit)
                    return true;
                return false;
            }
        }
    }
}
