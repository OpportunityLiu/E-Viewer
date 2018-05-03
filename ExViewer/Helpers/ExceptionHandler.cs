using System;
using System.Runtime.InteropServices;

namespace ExViewer
{
    internal static class ExceptionHandler
    {
        public static string GetMessage(this Exception ex)
        {
            if (ex.InnerException != null && (ex is System.Reflection.TargetInvocationException || ex is AggregateException))
            {
                // the outer exception is meaningless.
                ex = ex.InnerException;
            }

            var localizedMsg = Strings.Exceptions.GetValue(ex.HResult.ToString("X8"));
            if (!string.IsNullOrEmpty(localizedMsg))
                return localizedMsg;

            if (ex is COMException
                && ex.Data.Contains("RestrictedDescription")
                && ex.Data["RestrictedDescription"] is string rinfo
                && !rinfo.IsNullOrWhiteSpace())
                return rinfo.Trim();

            if (!ex.Message.IsNullOrWhiteSpace())
                return ex.Message.Trim();

            return $"HResult: {ex.HResult:X8}, {ex.GetType()}";
        }
    }
}
