using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient.Galleries.Renaming
{
    public struct RenameRecord
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
    }
}
