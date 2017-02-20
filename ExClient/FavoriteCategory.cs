using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExClient
{
    public sealed class FavoriteCategory
    {
        internal FavoriteCategory(int index)
        {
            CollectionIndex = index;
        }

        public int CollectionIndex
        {
            get;
        }

        public string CollectionName
        {
            get;
            internal set;
        }
    }
}
