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
