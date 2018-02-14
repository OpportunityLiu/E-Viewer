using Microsoft.Graphics.Canvas.Effects;
using Opportunity.MvvmUniverse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Power;
using Windows.UI.Composition;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Controls
{
    public sealed partial class CoverPresenter : UserControl
    {
        public CoverPresenter()
        {
            this.InitializeComponent();
        }

        public ImageSource Source
        {
            get => (ImageSource)GetValue(SourceProperty); set => SetValue(SourceProperty, value);
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(CoverPresenter), new PropertyMetadata(null));

        private static UISettings uiSettings = new UISettings();

        private void UserControl_Loading(FrameworkElement sender, object args)
        {
            if (!ApiInfo.RS3)
                return;
            setEffectStatus(uiSettings, null);
            PowerManager.EnergySaverStatusChanged += this.setEffectStatus;
            uiSettings.AdvancedEffectsEnabledChanged += this.setEffectStatus;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!ApiInfo.RS3)
                return;
            PowerManager.EnergySaverStatusChanged -= this.setEffectStatus;
            uiSettings.AdvancedEffectsEnabledChanged -= this.setEffectStatus;
        }

        private async void setEffectStatus(object sender, object args)
        {
            await DispatcherHelper.YieldIdle();
            if (uiSettings.AdvancedEffectsEnabled && PowerManager.EnergySaverStatus != EnergySaverStatus.On)
            {
                FindName(nameof(this.BackgroundImage));
                this.BackgroundImage.Visibility = Visibility.Visible;
            }
            else
            {
                if (this.BackgroundImage != null)
                    this.BackgroundImage.Visibility = Visibility.Collapsed;
            }
        }
    }
}
