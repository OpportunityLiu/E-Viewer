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
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected async void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            var temp = PropertyChanged;
            if(temp == null)
                return;
            await DispatcherHelper.RunNormalAsync(() => temp(this, new PropertyChangedEventArgs(propertyName)));
        }
    }
}
