using System;

namespace ExClient.Galleries.Renaming
{
    public readonly struct RenameRecord : IEquatable<RenameRecord>
    {
        public int ID { get; }

        public string Title { get; }

        public int Power { get; }

        internal RenameRecord(int id, string title, int power)
        {
            this.ID = id;
            this.Title = title;
            this.Power = power;
        }

        public bool Equals(RenameRecord other)
        {
            return this.ID == other.ID
                && this.Title == other.Title
                && this.Power == other.Power;
        }

        public override bool Equals(object obj) => (obj is RenameRecord rec) ? Equals(rec) : false;

        public override int GetHashCode() => (this.ID * 17) ^ this.Title.GetHashCode() ^ (this.Power * 19260817);
    }
}
