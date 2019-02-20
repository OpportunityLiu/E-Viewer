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
        public int Power { get => power; internal set => Set(ref power, value); }

        internal RenameRecord(int id, string title, int power)
        {
            ID = id;
            Title = title;
            this.power = power;
        }

        public bool Equals(RenameRecord other)
            => ID == other.ID
            && Title == other.Title;

        public override bool Equals(object obj) => obj is RenameRecord rec && Equals(rec);

        public override int GetHashCode() => unchecked((ID * 17) ^ Title.GetHashCode());

        public override string ToString() => Title;
    }
}
