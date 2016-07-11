using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExViewer
{
    internal static class ExceptionHandler
    {
        private static string[] prefixes = new string[]
        {
            "无法找到与此错误代码关联的文本。",
            "The text associated with this error code could not be found."
        };

        public static string GetMessage(this Exception ex)
        {
            var msg = ex.Message.TrimStart();
            foreach(var prefix in prefixes)
            {
                if(msg.StartsWith(prefix))
                {
                    msg = msg.Substring(prefix.Length);
                    break;
                }
            }
            return msg.Trim();
        }
    }
}
