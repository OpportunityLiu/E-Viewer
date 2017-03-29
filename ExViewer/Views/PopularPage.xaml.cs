using ExViewer.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ExViewer.ViewModels;
using ExClient;
using System.Threading.Tasks;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PopularPage : MyPage, IHasAppBar
    {
        public PopularPage()
        {
            this.InitializeComponent();
            this.VisibleBoundHandledByDesign = true;
            this.VM = new PopularVM();
        }

        public PopularVM VM
        {
            get => (PopularVM)GetValue(VMProperty);
            set => SetValue(VMProperty, value);
        }

        public static readonly DependencyProperty VMProperty =
            DependencyProperty.Register(nameof(VM), typeof(PopularVM), typeof(PopularPage), new PropertyMetadata(null));
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(e.NavigationMode == NavigationMode.Back)
            {
                await Task.Delay(50);
                ((ListViewItem)this.lv.ContainerFromItem(this.opened))?.Focus(FocusState.Programmatic);
            }
        }

        private Gallery opened;

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void lv_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(this.VM.Open.CanExecute(e.ClickedItem))
            {
                this.VM.Open.Execute(e.ClickedItem);
                this.opened = (Gallery)e.ClickedItem;
            }
        }

        private void lv_RefreshRequested(object sender, EventArgs args)
        {
            this.VM.Refresh.Execute(null);
        }

        public void CloseAll()
        {
            this.cb_top.IsOpen = false;
        }
    }
}
