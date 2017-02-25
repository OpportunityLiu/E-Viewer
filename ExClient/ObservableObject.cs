using GalaSoft.MvvmLight.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExClient
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool Set(ref bool field, bool value, [CallerMemberName]string propertyName = null)
        {
            if(field == value)
                return false;
            ForceSet(ref field, value, propertyName);
            return true;
        }

        protected void ForceSet(ref bool field, bool value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            RaisePropertyChanged(propertyName);
        }

        protected bool Set<TProp>(ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            if(Equals(field, value))
                return false;
            ForceSet(ref field, value, propertyName);
            return true;
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
