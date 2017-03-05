using GalaSoft.MvvmLight.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExClient
{
    public class ObservableObject : INotifyPropertyChanged
    {
        protected bool Set<TProp>(ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            if(Equals(field, value))
                return false;
            ForceSet(ref field, value, propertyName);
            return true;
        }

        protected bool Set<TProp>(ref TProp field, TProp value, string addtionalPropertyName, [CallerMemberName]string propertyName = null)
        {
            if(Equals(field, value))
                return false;
            ForceSet(ref field, value, addtionalPropertyName, propertyName);
            return true;
        }

        protected bool Set<TProp>(ref TProp field, TProp value, string addtionalPropertyName0, string addtionalPropertyName1, [CallerMemberName]string propertyName = null)
        {
            if(Equals(field, value))
                return false;
            ForceSet(ref field, value, addtionalPropertyName0, addtionalPropertyName1, propertyName);
            return true;
        }

        protected bool Set<TProp>(ref TProp field, TProp value, IEnumerable<string> addtionalPropertyNames, [CallerMemberName]string propertyName = null)
        {
            if(Equals(field, value))
                return false;
            ForceSet(ref field, value, addtionalPropertyNames, propertyName);
            return true;
        }

        protected void ForceSet<TProp>(ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            RaisePropertyChanged(propertyName);
        }

        protected void ForceSet<TProp>(ref TProp field, TProp value, string addtionalPropertyName, [CallerMemberName]string propertyName = null)
        {
            field = value;
            RaisePropertyChanged(propertyName, addtionalPropertyName);
        }

        protected void ForceSet<TProp>(ref TProp field, TProp value, string addtionalPropertyName0, string addtionalPropertyName1, [CallerMemberName]string propertyName = null)
        {
            field = value;
            RaisePropertyChanged(propertyName, addtionalPropertyName0, addtionalPropertyName1);
        }

        protected void ForceSet<TProp>(ref TProp field, TProp value, IEnumerable<string> addtionalPropertyNames, [CallerMemberName]string propertyName = null)
        {
            field = value;
            var temp = PropertyChanged;
            if(temp == null)
                return;
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                temp(this,new PropertyChangedEventArgs(propertyName));
                foreach(var item in addtionalPropertyNames)
                {
                    temp(this, new PropertyChangedEventArgs(item));
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            var temp = PropertyChanged;
            if(temp == null)
                return;
            DispatcherHelper.CheckBeginInvokeOnUI(() => temp(this, new PropertyChangedEventArgs(propertyName)));
        }

        protected void RaisePropertyChanged(string propertyName0, string propertyName1)
        {
            var temp = PropertyChanged;
            if(temp == null)
                return;
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                temp(this, new PropertyChangedEventArgs(propertyName0));
                temp(this, new PropertyChangedEventArgs(propertyName1));
            });
        }

        protected void RaisePropertyChanged(string propertyName0, string propertyName1, string propertyName2)
        {
            var temp = PropertyChanged;
            if(temp == null)
                return;
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                temp(this, new PropertyChangedEventArgs(propertyName0));
                temp(this, new PropertyChangedEventArgs(propertyName1));
                temp(this, new PropertyChangedEventArgs(propertyName2));
            });
        }

        protected void RaisePropertyChanged(params string[] propertyNames)
        {
            this.RaisePropertyChanged((IEnumerable<string>)propertyNames);
        }

        protected void RaisePropertyChanged(IEnumerable<string> propertyNames)
        {
            var temp = PropertyChanged;
            if(temp == null)
                return;
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                foreach(var item in propertyNames)
                {
                    temp(this, new PropertyChangedEventArgs(item));
                }
            });
        }
    }
}
