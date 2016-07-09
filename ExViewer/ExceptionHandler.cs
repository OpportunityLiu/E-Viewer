using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExViewer
{
    internal static class ExceptionHandler
    {
        public static string GetMessage(this Exception ex)
        {
            var msg = ex.Message.TrimStart();
            var prefix = LocalizedStrings.Resources.ErrorPrefix;
            if(msg.StartsWith(prefix))
            {
                msg = msg.Substring(prefix.Length);
            }
            return msg.Trim();
        }
    }
}
