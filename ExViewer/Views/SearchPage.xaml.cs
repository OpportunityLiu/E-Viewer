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
                await RootControl.RootController.RequireLogOn();
            }
            VM = new SearchVM(e.Parameter?.ToString());
            Bindings.Update();
            await Task.Yield();
            ab.Focus(FocusState.Pointer);
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

        private void init_sv_AdvancedSearch()
        {
            FindName(nameof(sv_AdvancedSearch));
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
            init_sv_AdvancedSearch();
            sv_AdvancedSearch.IsEnabled = true;
        }

        private void ab_Tapped(object sender, TappedRoutedEventArgs e)
        {
            init_sv_AdvancedSearch();
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

        private void asb_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if(args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                sender.ItemsSource = Enum.GetNames(typeof(NameSpace)).Where(s => s.StartsWith(sender.Text, StringComparison.CurrentCultureIgnoreCase));
            }
        }

        private void asb_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Focus(FocusState.Programmatic);
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if(args.ChosenSuggestion != null)
                return;
            ab.IsOpen = false;
            VM.Search.Execute(null);
        }

        private void btn_Advanced_Click(object sender, RoutedEventArgs e)
        {
            switch(lv_AdvancedSearch.Visibility)
            {
            case Visibility.Visible:
                lv_AdvancedSearch.Visibility = Visibility.Collapsed;
                btn_Advanced.Content = "Show advanced options";
                break;
            case Visibility.Collapsed:
                lv_AdvancedSearch.Visibility = Visibility.Visible;
                btn_Advanced.Content = "Hide advanced options";
                break;
            }
        }

        private void PullToRefreshBox_RefreshInvoked(DependencyObject sender, object args)
        {
            VM?.SearchResult.Reset();
        }
    }
}
