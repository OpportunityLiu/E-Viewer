using System.Collections.Generic;
using Windows.ApplicationModel.Resources;

namespace ApplicationDataManager.Settings
{
    internal static class StringLoader
    {
        private static readonly Dictionary<string, string> cache = new Dictionary<string, string>();

        private static readonly ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("/Settings");

        public static string GetString(string resourceKey)
        {
            if(cache.TryGetValue(resourceKey, out var value))
            {
                return value;
            }
            else
            {
                return cache[resourceKey] = loader.GetString(resourceKey);
            }
        }

        public static void ClearCache()
        {
            cache.Clear();
        }
    }
}
