using Opportunity.MvvmUniverse;
using System;
using System.Diagnostics;

namespace ExClient.Services
{
    [DebuggerDisplay(@"{Power}% {Title}")]
    public class RenameRecord : ObservableObject, IEquatable<RenameRecord>
    {
        public int ID { get; }

        public string Title { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int power;
        public int Power { get => this.power; internal set => Set(ref this.power, value); }

        internal RenameRecord(int id, string title, int power)
        {
            this.ID = id;
            this.Title = title;
            this.power = power;
        }

        public bool Equals(RenameRecord other)
        {
            return this.ID == other.ID
                && this.Title == other.Title;
        }

        public override bool Equals(object obj) => (obj is RenameRecord rec) ? Equals(rec) : false;

        public override int GetHashCode() => (this.ID * 17) ^ this.Title.GetHashCode();

        public override string ToString() => Title;
    }
}
