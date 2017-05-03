using System;

namespace EhWikiClient
{
    [Flags]
    public enum TagType
    {
        Unknown = 0,
        /// <summary>
        /// Character Tag
        /// </summary>
        Character = 0b1,
        /// <summary>
        /// Creator Tag‏‎
        /// </summary>
        Creator = 0b10,
        /// <summary>
        /// Language Tag
        /// </summary>
        Language = 0b100,
        /// <summary>
        /// Series Tag‏‎
        /// </summary>
        Series = 0b1000,
        /// <summary>
        /// Tag
        /// </summary>
        Fetish = 0b10000
    }
}