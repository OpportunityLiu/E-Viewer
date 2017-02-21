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
using Windows.UI.ViewManagement;
using Windows.UI;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FavoritesPage : Page, IHasAppBar
    {
        public FavoritesPage()
        {
            this.InitializeComponent();
            var l = new List<FavoriteCategory>(11);
            l.Add(FavoriteCategory.All);
            l.AddRange(Client.Current.Favorites);
            this.cbCategory.ItemsSource = l;
        }

        private int navId;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            navId++;
            if(Client.Current.NeedLogOn)
            {
                await RootControl.RootController.RequestLogOn();
            }
            VM = FavoritesVM.GetVM(e.Parameter?.ToString());
            if(e.NavigationMode == NavigationMode.New && e.Parameter != null)
            {
                VM?.SearchResult.Reset();
                await Task.Delay(100);
                btnPane.Focus(FocusState.Programmatic);
            }
            if(e.NavigationMode == NavigationMode.Back)
            {
                VM.SetQueryWithSearchResult();
                var selectedGallery = VM.SelectedGallery;
                if(selectedGallery != null)
                {
                    await Task.Delay(100);
                    this.lv.ScrollIntoView(selectedGallery);
                    ((Control)this.lv.ContainerFromItem(selectedGallery))?.Focus(FocusState.Programmatic);
                }
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

        public FavoritesVM VM
        {
            get
            {
                return (FavoritesVM)GetValue(VMProperty);
            }
            set
            {
                SetValue(VMProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for VM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty =
            DependencyProperty.Register("VM", typeof(FavoritesVM), typeof(FavoritesPage), new PropertyMetadata(null));

        private void btnPane_Click(object sender, RoutedEventArgs e)
        {
            RootControl.RootController.SwitchSplitView();
        }

        private async void asb_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var needAutoComplete = args.Reason == AutoSuggestionBoxTextChangeReason.UserInput
                || args.Reason == AutoSuggestionBoxTextChangeReason.SuggestionChosen;
            var currentId = navId;
            if(needAutoComplete)
            {
                var r = await VM.LoadSuggestion(sender.Text);
                if(args.CheckCurrent() && currentId == navId)
                {
                    asb.ItemsSource = r;
                }
            }
        }

        private void asb_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.ItemsSource = null;
        }

        private void asb_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if(args.ChosenSuggestion == null || VM.AutoCompleteFinished(args.ChosenSuggestion))
            {
                CloseAll();
                VM.Search.Execute(args.QueryText);
            }
            else
            {
                asb.Focus(FocusState.Keyboard);
            }
        }

        private void lv_RefreshRequested(object sender, EventArgs e)
        {
            VM?.SearchResult.Reset();
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = true;
            switch(e.Key)
            {
            case Windows.System.VirtualKey.GamepadY:
                asb.Focus(FocusState.Keyboard);
                break;
            case Windows.System.VirtualKey.GamepadMenu:
            case Windows.System.VirtualKey.Application:
                break;
            default:
                e.Handled = false;
                break;
            }
        }

        public void CloseAll()
        {
            asb.IsSuggestionListOpen = false;
            cbCategory.IsDropDownOpen = false;
        }
    }
}
