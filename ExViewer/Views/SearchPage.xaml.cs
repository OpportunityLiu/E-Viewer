using ExClient;
using ExViewer.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Security.Credentials;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using ExViewer.ViewModels;
using System.Threading.Tasks;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        public SearchPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(Client.Current.NeedLogOn)
            {
                await RootControl.RootController.RequestLogOn();
            }
            VM = new SearchVM(e.Parameter?.ToString());
            if(e.NavigationMode == NavigationMode.New && e.Parameter != null)
                VM?.SearchResult.Reset();
            if(e.NavigationMode == NavigationMode.Back)
            {
                //TODO: restore scroll position.
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        private void lv_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(VM.Open.CanExecute(e.ClickedItem))
                VM.Open.Execute(e.ClickedItem);
        }

        public SearchVM VM
        {
            get
            {
                return (SearchVM)GetValue(VMProperty);
            }
            set
            {
                SetValue(VMProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for VM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty =
            DependencyProperty.Register("VM", typeof(SearchVM), typeof(SearchPage), new PropertyMetadata(null));

        private void btn_pane_Click(object sender, RoutedEventArgs e)
        {
            ab.IsOpen = false;
            RootControl.RootController.SwitchSplitView();
        }

        private void ab_Opening(object sender, object e)
        {
            sv_AdvancedSearch.IsEnabled = true;
        }

        private void ab_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var newState = e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch;
            if(newState == cs_Category.TouchAdaptive)
                return;
            if(!abOpened)
            {
                cs_Category.TouchAdaptive = newState;
            }
        }

        bool abOpened;

        private void ab_Opened(object sender, object e)
        {
            abOpened = true;
        }

        private void ab_Closed(object sender, object e)
        {
            abOpened = false;
            sv_AdvancedSearch.IsEnabled = false;
        }

        private async void asb_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var r = await VM.LoadSuggestion(sender.Text);
                if(args.CheckCurrent())
                    asb.ItemsSource = r;
            }
        }

        private void asb_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if(!VM.AutoCompleteFinished(args.SelectedItem))
                return;
            sender.Focus(FocusState.Programmatic);
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            ab.IsOpen = false;
            VM.Search.Execute(args.QueryText);
        }

        private void lv_RefreshRequested(object sender, EventArgs e)
        {
            VM?.SearchResult.Reset();
        }
    }
}
