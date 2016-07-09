using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace HtmlAgilityPack
{
    internal static class HtmlHelper
    {
        public static string DeEntitize(this string that)
        {
            return HtmlEntity.DeEntitize(that);
        }
    }
}
