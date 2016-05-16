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
    public sealed partial class SearchPage : Page, IRootController
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
                    if(client == null)
                    {
                        client = Client.Current;
                        asb.IsEnabled = true;
                        asb.Text = searchKeyWord;
                        SearchResult = await client.SearchAsync(searchKeyWord, searchFilter);
                    }
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

        private LogOnDialog logOn = new LogOnDialog();

        private Client client;

        private string searchKeyWord = Settings.Settings.Current.DefaultSearchString;

        private Category searchFilter = Settings.Settings.Current.DefaultSearchCategory;

        public event EventHandler<RootControlCommand> CommandExecuted;

        private ObservableCollection<FilterRecord> filter;

        private void lv_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(GalleryPage), e.ClickedItem);
        }

        private bool init_gv_AdvancedSearch()
        {
            if(gv_AdvancedSearch != null)
                return false;
            filter = new ObservableCollection<FilterRecord>()
            {
                new FilterRecord(Category.Doujinshi,searchFilter.HasFlag(Category.Doujinshi)),
                new FilterRecord(Category.Manga, searchFilter.HasFlag(Category.Manga)),
                new FilterRecord(Category.ArtistCG, searchFilter.HasFlag(Category.ArtistCG)),
                new FilterRecord(Category.GameCG, searchFilter.HasFlag(Category.GameCG)),
                new FilterRecord(Category.Western, searchFilter.HasFlag(Category.Western)),
                new FilterRecord(Category.NonH, searchFilter.HasFlag(Category.NonH)),
                new FilterRecord(Category.ImageSet, searchFilter.HasFlag(Category.ImageSet)),
                new FilterRecord(Category.Cosplay, searchFilter.HasFlag(Category.Cosplay)),
                new FilterRecord(Category.AsianPorn, searchFilter.HasFlag(Category.AsianPorn)),
                new FilterRecord(Category.Misc, searchFilter.HasFlag(Category.Misc))
            };
            FindName(nameof(gv_AdvancedSearch));
            Bindings.Update();
            return true;
        }

        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            ab.IsOpen = false;
            init_gv_AdvancedSearch();
            var category = Category.Unspecified;
            foreach(var item in filter)
            {
                if(item.IsChecked)
                    category |= item.Category;
            }
            this.Focus(FocusState.Pointer);
            if(category == searchFilter && args.QueryText == searchKeyWord)
                return;
            searchKeyWord = args.QueryText;
            searchFilter = category;
            Settings.Settings.Current.DefaultSearchCategory = category;
            Settings.Settings.Current.DefaultSearchString = this.searchKeyWord;
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
            CommandExecuted?.Invoke(this, RootControlCommand.SwitchSplitView);
        }

        private void ab_Opening(object sender, object e)
        {
            //init_gv_AdvancedSearch();
        }

        bool isWideState;

#pragma warning disable UWP001 // Platform-specific
        Thickness mouseThickness = new Thickness(4), touchThickness = new Thickness(12);
#pragma warning restore UWP001 // Platform-specific

        private void ab_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var gvLoaded = !init_gv_AdvancedSearch();
            var newState = e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch;
            if(newState == isWideState)
                return;
            if(!abOpened)
            {
                if(gvLoaded)
                {
                    if(newState)
                    {
                        for(int i = 0; i < filter.Count; i++)
                        {
                            ((FrameworkElement)gv_AdvancedSearch.ContainerFromIndex(i)).Margin = touchThickness;
                        }
                    }
                    else
                    {
                        for(int i = 0; i < filter.Count; i++)
                        {
                            ((FrameworkElement)gv_AdvancedSearch.ContainerFromIndex(i)).Margin = mouseThickness;

                        }
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

        private void gv_AdvancedSearch_Loaded(object sender, RoutedEventArgs e)
        {
            if(isWideState)
            {
                for(int i = 0; i < filter.Count; i++)
                {
                    ((FrameworkElement)gv_AdvancedSearch.ContainerFromIndex(i)).Margin = touchThickness;
                }
            }
            else
            {
                for(int i = 0; i < filter.Count; i++)
                {
                    ((FrameworkElement)gv_AdvancedSearch.ContainerFromIndex(i)).Margin = mouseThickness;

                }
            }
        }
    }

    internal class FilterRecord
    {
        public FilterRecord(Category category, bool isChecked)
        {
            Category = category;
            IsChecked = isChecked;
        }

        public readonly Category Category;
        public bool IsChecked;
    }
}
