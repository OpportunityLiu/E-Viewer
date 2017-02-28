using ExClient;
using ExClient.ExClient_ResourceInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    internal static class EnumExtension
    {
        public static string ToFriendlyNameString<T>(T that, IResourceProvider nameProvider)
            where T : struct
        {
            if(Enum.IsDefined(typeof(T), that))
                return nameProvider[that.ToString()];
            else
            {
                var represent = new StringBuilder(that.ToString());
                foreach(var item in Enum.GetNames(typeof(T)))
                {
                    represent.Replace(item, nameProvider[item]);
                }
                return represent.ToString();
            }
        }
    }
}
