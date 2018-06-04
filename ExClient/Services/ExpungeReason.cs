using System;

namespace ExClient.Services
{
    public enum ExpungeReason
    {
        None = 0,
        Duplicate = 2,
        Replaced = 5,
        Forbidden = 4,
    }

    public static class ExpungeReasonExtension
    {
        public static string ToFriendlyNameString(this ExpungeReason that)
            => that.ToFriendlyNameString(name => LocalizedStrings.ExpungeReason[name].GetValue("Name"));
        public static string GetDescription(this ExpungeReason that)
            => that.ToFriendlyNameString(name => LocalizedStrings.ExpungeReason[name].GetValue("Description"));
    }
}
