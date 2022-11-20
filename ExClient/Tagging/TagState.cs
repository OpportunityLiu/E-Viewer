using ExClient.Api;

using System;

namespace ExClient.Tagging
{
    [Flags]
    public enum TagState
    {
        Upvoted = 0b0001,
        // 0b0010 is an invalid state.
        Downvoted = 0b0011,

        Slave = 0b0001_0000,

        /// <summary>
        /// Mod-power &lt;= 0, not present in the <see cref="TagCollection"/>.
        /// </summary>
        NotPresented = 0,

        /// <summary>
        /// Mod-power of 1~9, visible.
        /// </summary>
        LowPower = 0b0001_0000_0000,
        /// <summary>
        /// Mod-power of 10~99, searchable.
        /// </summary>
        NormalPower = 0b0011_0000_0000,
        /// <summary>
        /// Mod-power &gt;= 100.
        /// </summary>
        HighPower = 0b0111_0000_0000
    }

    public static class TagStateExtension
    {
        private const TagState VOTE_MASK = (TagState)0b0011;
        private const TagState POWER_MASK = (TagState)0b0111_0000_0000;

        public static bool IsSlave(this TagState state)
        {
            return (state & TagState.Slave) != 0;
        }

        public static TagState GetVoteState(this TagState state)
        {
            return state & VOTE_MASK;
        }

        public static TagState GetPowerState(this TagState state)
        {
            return state & POWER_MASK;
        }

        public static VoteState AsVoteState(this TagState state)
        {
            switch (state & VOTE_MASK)
            {
            case TagState.Upvoted:
                return VoteState.Up;
            case TagState.Downvoted:
                return VoteState.Down;
            default:
                return VoteState.Default;
            }
        }
    }
}
