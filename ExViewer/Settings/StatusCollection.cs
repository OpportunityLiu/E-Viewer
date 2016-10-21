using ApplicationDataManager;
using ApplicationDataManager.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExViewer.Settings
{
    class StatusCollection : ApplicationDataCollection
    {
        public static StatusCollection Current
        {
            get;
        } = new StatusCollection();

        private StatusCollection()
            : base("Status") { }

        public bool ImageViewTipShown
        {
            get
            {
                return GetLocal(false);
            }
            set
            {
                SetLocal(value);
            }
        }
    }
}
