using ExClient;
using ExViewer.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ExViewer.ViewModels
{
    public class PopularVM : ViewModelBase
    {
        public PopularVM()
        {
            this.Refresh = new RelayCommand(() => this.Galleries.Reset());
            this.Open = new RelayCommand<Gallery>(g =>
            {
                GalleryVM.GetVM(g);
                RootControl.RootController.Frame.Navigate(typeof(GalleryPage), g.Id);
            }, g => g != null);
        }

        public PopularCollection Galleries { get; } = PopularCollection.Instance;

        public ICommand Refresh { get; }

        public ICommand Open { get; }
    }
}
