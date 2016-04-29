using ExClient;
using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchPage : Page, IMainPageController
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
                if(Client.Current != null)
                {
                    client = Client.Current;
                    asb.IsEnabled = true;
                    SearchResult = await client.SearchAsync(searchKeyWord, searchFilter);
                }
                else
                {
                    await logOn.ShowAsync();
                }
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        LogOnDialog logOn = new LogOnDialog();

        Client client;

        string searchKeyWord = "";

        Category searchFilter = Category.Unspecified;

        public event EventHandler<MainPageControlCommand> CommandExecuted;

        private void lv_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(GalleryPage), e.ClickedItem);
        }

        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            ab.IsOpen = false;
            FindName(nameof(gv_AdvancedSearch));
            var category = Category.Unspecified;
            if(tb_Doujinshi.IsChecked == true)
                category |= Category.Doujinshi;
            if(tb_Manga.IsChecked == true)
                category |= Category.Manga;
            if(tb_ArtistCG.IsChecked == true)
                category |= Category.ArtistCG;
            if(tb_GameCG.IsChecked == true)
                category |= Category.GameCG;
            if(tb_Western.IsChecked == true)
                category |= Category.Western;
            if(tb_NonH.IsChecked == true)
                category |= Category.NonH;
            if(tb_ImageSet.IsChecked == true)
                category |= Category.ImageSet;
            if(tb_Cosplay.IsChecked == true)
                category |= Category.Cosplay;
            if(tb_AsianPorn.IsChecked == true)
                category |= Category.AsianPorn;
            if(tb_Misc.IsChecked == true)
                category |= Category.Misc;
            this.Focus(FocusState.Pointer);
            if(category == searchFilter && args.QueryText == searchKeyWord)
                return;
            searchKeyWord = args.QueryText;
            searchFilter = category;
            SearchResult = null;
            SearchResult = await client.SearchAsync(searchKeyWord, searchFilter);
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
            CommandExecuted?.Invoke(this, MainPageControlCommand.SwitchSplitView);
        }

        private void ab_Opening(object sender, object e)
        {
            FindName(nameof(gv_AdvancedSearch));
        }

        bool isWideState;

        private void ab_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var newState = e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch;
            if(newState == isWideState)
                return;
            if(!abOpened)
            {
                FindName(nameof(gv_AdvancedSearch));
                if(newState)
                {
                    foreach(FrameworkElement item in gv_AdvancedSearch.Items)
                    {
                        item.Margin = new Thickness(8);
                    }
                }
                else
                {
                    foreach(FrameworkElement item in gv_AdvancedSearch.Items)
                    {
                        item.Margin = new Thickness(0);
                    }
                }
                isWideState = newState;
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

    internal class RateStringConverter : IValueConverter
    {
        const char halfL = '\xE7C6';
        const char full = '\xE00A';

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var rating = ((double)value) * 2;
            var x = (int)Math.Round(rating);
            var fullCount = x / 2;
            var halfCount = x - 2 * fullCount;
            return new string(full, fullCount) + new string(halfL, halfCount);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
