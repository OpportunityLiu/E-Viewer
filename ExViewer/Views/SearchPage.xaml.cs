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
            if(e.NavigationMode == NavigationMode.New)
            {
                if(Client.Current.NeedLogOn)
                {
                    await RootControl.RootController.RequireLogOn();
                }
                setHah();
                client = Client.Current;
                asb.IsEnabled = true;
                asb.Text = searchKeyWord;
                if(e.Parameter == null)
                {
                    if(SearchResult == null)
                        SearchResult = await client.SearchAsync(searchKeyWord, searchFilter);
                }
                else
                    SearchResult = (SearchResult)e.Parameter;
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        private Client client;

        private string searchKeyWord = SettingCollection.Current.DefaultSearchString;

        private Category searchFilter = SettingCollection.Current.DefaultSearchCategory;

        private void lv_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(GalleryPage), e.ClickedItem);
        }

        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            ab.IsOpen = false;
            init_cs_AdvancedSearch();
            var category = cs_AdvancedSearch.SelectedCategory;
            this.Focus(FocusState.Pointer);

            searchKeyWord = args.QueryText;
            searchFilter = category;
            if(SettingCollection.Current.SaveLastSearch)
            {
                SettingCollection.Current.DefaultSearchCategory = category;
                SettingCollection.Current.DefaultSearchString = this.searchKeyWord;
            }

            setHah();

            SearchResult = null;
            SearchResult = await client.SearchAsync(searchKeyWord, searchFilter);
        }

        private void init_cs_AdvancedSearch()
        {
            if(cs_AdvancedSearch != null)
                return;
            FindName(nameof(cs_AdvancedSearch));
            cs_AdvancedSearch.SelectedCategory = searchFilter;
        }

        private static void setHah()
        {
            // set H@H proxy
            var hah = SettingCollection.Current.HahAddress;
            if(!string.IsNullOrEmpty(hah))
                Client.Current.SetHahProxy(new HahProxyConfig(hah, SettingCollection.Current.HahPasskey));
            else
                Client.Current.SetHahProxy(null);
        }

        public SearchResult SearchResult
        {
            get
            {
                return (SearchResult)GetValue(SearchResultProperty);
            }
            set
            {
                SetValue(SearchResultProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for SearchResult.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchResultProperty =
            DependencyProperty.Register("SearchResult", typeof(SearchResult), typeof(SearchPage), new PropertyMetadata(null));

        private void btn_pane_Click(object sender, RoutedEventArgs e)
        {
            ab.IsOpen = false;
            RootControl.RootController.SwitchSplitView();
        }

        private void ab_Opening(object sender, object e)
        {
            init_cs_AdvancedSearch();
        }

        private void ab_Tapped(object sender, TappedRoutedEventArgs e)
        {
            init_cs_AdvancedSearch();
            var newState = e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch;
            if(newState == cs_AdvancedSearch.TouchAdaptive)
                return;
            if(!abOpened)
            {
                cs_AdvancedSearch.TouchAdaptive = newState;
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
        }
    }
}
