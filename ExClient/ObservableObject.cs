using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ExClient
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Set<TProp>(ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            if(Equals(field, value))
                return;
            ForceSet(ref field, value, propertyName);
        }

        protected void ForceSet<TProp>(ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            RaisePropertyChanged(propertyName);
        }

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            var temp = PropertyChanged;
            if(temp == null)
                return;
            DispatcherHelper.CheckBeginInvokeOnUI(() => temp(this, new PropertyChangedEventArgs(propertyName)));
        }
    }
}
