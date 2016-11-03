using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Threading;

namespace ExViewer
{
    public class Box<T> : INotifyPropertyChanged
    {
        public Box(T value)
        {
            this.value = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName]string name = null)
        {
            var temp = PropertyChanged;
            if(temp == null)
                return;
            DispatcherHelper.CheckBeginInvokeOnUI(() => temp(this, new PropertyChangedEventArgs(name)));
        }

        private T value;

        public T Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                RaisePropertyChanged();
            }
        }
    }
}
