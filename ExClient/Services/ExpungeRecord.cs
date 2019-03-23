using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient.Services
{
    [DebuggerDisplay(@"+{Power} {Reason} on {Posted} by {Author}")]
    public readonly struct ExpungeRecord : IEquatable<ExpungeRecord>
    {
        internal ExpungeRecord(ExpungeReason reason, string explanation, string author, DateTimeOffset posted, int power)
        {
            Reason = reason;
            Explanation = explanation;
            Author = author;
            Posted = posted;
            Power = power;
        }

        public ExpungeReason Reason { get; }
        public string Explanation { get; }
        public string Author { get; }
        public DateTimeOffset Posted { get; }
        public int Power { get; }

        public bool Equals(ExpungeRecord other)
            => Posted == other.Posted
            && Power == other.Power
            && Author == other.Author
            && Reason == other.Reason
            && Explanation == other.Explanation;

        public override bool Equals(object obj) => obj is ExpungeRecord r && this.Equals(r);

        public override int GetHashCode() => unchecked(Author.GetHashCode() ^ Reason.GetHashCode() * 19260817 ^ Posted.GetHashCode() * 7 + Power);

        public static bool operator ==(in ExpungeRecord left, in ExpungeRecord right) => left.Equals(right);
        public static bool operator !=(in ExpungeRecord left, in ExpungeRecord right) => !left.Equals(right);
    }
}
