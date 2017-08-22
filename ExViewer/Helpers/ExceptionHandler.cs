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
            "找不到與此錯誤碼關聯的文字。",
            "The text associated with this error code could not be found.",
            "No se pudo encontrar el texto asociado a este código de error."
        };

        public static string GetMessage(this Exception ex)
        {
            if (ex.InnerException != null)
            {
                if (ex is System.Reflection.TargetInvocationException)
                    ex = ex.InnerException;
                else if (ex is AggregateException)
                    ex = ex.InnerException;
            }
            var localizedMsg = Strings.Exceptions.GetValue(ex.HResult.ToString("X8"));
            if (!string.IsNullOrEmpty(localizedMsg))
                return localizedMsg;
            var msg = ex.Message.TrimStart();
            foreach (var prefix in prefixes)
            {
                if (msg.StartsWith(prefix))
                {
                    msg = msg.Substring(prefix.Length);
                    break;
                }
            }
            return msg.Trim();
        }
    }
}
