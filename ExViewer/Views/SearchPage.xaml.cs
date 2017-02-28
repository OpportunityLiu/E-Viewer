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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace ExViewer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchPage : Page, IHasAppBar
    {
        public SearchPage()
        {
            this.InitializeComponent();
        }

        private int navId;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.navId++;
            if(Client.Current.NeedLogOn)
            {
                await RootControl.RootController.RequestLogOn();
            }
            this.VM = SearchVM.GetVM(e.Parameter?.ToString());
            if(e.NavigationMode == NavigationMode.New && e.Parameter != null)
            {
                this.VM?.SearchResult.Reset();
                await Task.Delay(100);
                this.btnPane.Focus(FocusState.Programmatic);
            }
            if(e.NavigationMode == NavigationMode.Back)
            {
                this.VM.SetQueryWithSearchResult();
                var selectedGallery = this.VM.SelectedGallery;
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
            if(this.VM.Open.CanExecute(e.ClickedItem))
                this.VM.Open.Execute(e.ClickedItem);
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

        private void btnPane_Click(object sender, RoutedEventArgs e)
        {
            RootControl.RootController.SwitchSplitView();
        }

        private void ab_Opening(object sender, object e)
        {
            this.sv_AdvancedSearch.IsEnabled = true;
        }

        private void ab_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var newState = e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch;
            if(newState == this.cs_Category.TouchAdaptive)
                return;
            if(!this.abOpened)
            {
                this.cs_Category.TouchAdaptive = newState;
            }
        }

        bool abOpened;

        private void ab_Opened(object sender, object e)
        {
            this.abOpened = true;
        }

        private void ab_Closed(object sender, object e)
        {
            this.abOpened = false;
            this.sv_AdvancedSearch.IsEnabled = false;
        }

        private async void asb_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var needAutoComplete = args.Reason == AutoSuggestionBoxTextChangeReason.UserInput
                || args.Reason == AutoSuggestionBoxTextChangeReason.SuggestionChosen;
            var currentId = this.navId;
            if(needAutoComplete)
            {
                var r = await this.VM.LoadSuggestion(sender.Text);
                if(args.CheckCurrent() && currentId == this.navId)
                {
                    this.asb.ItemsSource = r;
                }
            }
        }

        private void asb_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.ItemsSource = null;
        }

        private void asb_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if(args.ChosenSuggestion == null || this.VM.AutoCompleteFinished(args.ChosenSuggestion))
            {
                CloseAll();
                this.VM.Search.Execute(args.QueryText);
            }
            else
            {
                this.asb.Focus(FocusState.Keyboard);
            }
        }

        private void lv_RefreshRequested(object sender, EventArgs e)
        {
            this.VM?.SearchResult.Reset();
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = true;
            switch(e.Key)
            {
            case Windows.System.VirtualKey.GamepadY:
                this.asb.Focus(FocusState.Keyboard);
                break;
            case Windows.System.VirtualKey.GamepadMenu:
            case Windows.System.VirtualKey.Application:
                this.ab.IsOpen = !this.ab.IsOpen;
                break;
            default:
                e.Handled = false;
                break;
            }
        }

        public void CloseAll()
        {
            this.asb.IsSuggestionListOpen = false;
            this.ab.IsOpen = false;
        }
    }
}
