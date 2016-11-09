using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Toolkit.Uwp.UI.Controls
{
    public class PullToRefreshIndicator : Control
    {
        public PullToRefreshIndicator()
        {
            this.DefaultStyleKey = typeof(PullToRefreshIndicator);
            this.Loading += PullToRefreshIndicator_Loading;
            this.Unloaded += PullToRefreshIndicator_Unloaded;
        }

        private void PullToRefreshIndicator_Loading(FrameworkElement sender, object args)
        {
            var s = (PullToRefreshIndicator)sender;
            var pv = s.FindVisualAscendant<PullToRefreshListView>();
            s.parent = pv;
        }

        private PullToRefreshListView p;

        private PullToRefreshListView parent
        {
            get
            {
                return p;
            }
            set
            {
                if(p != null)
                    p.PullProgressChanged -= Parent_PullProgressChanged;
                p = value;
                if(p != null)
                    p.PullProgressChanged += Parent_PullProgressChanged;
                this.ClearValue(PullProgressProperty);
            }
        }

        private void PullToRefreshIndicator_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ((PullToRefreshIndicator)sender).parent = null;
        }

        private void Parent_PullProgressChanged(object sender, RefreshProgressEventArgs e)
        {
            this.PullProgress = e.PullProgress;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        public double PullProgress
        {
            get
            {
                return (double)GetValue(PullProgressProperty);
            }
            set
            {
                SetValue(PullProgressProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for PullProgress.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PullProgressProperty =
            DependencyProperty.Register("PullProgress", typeof(double), typeof(PullToRefreshIndicator), new PropertyMetadata(0d, PullProgressChanged));

        private static void PullProgressChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var s = (PullToRefreshIndicator)sender;
            if((double)e.NewValue == 1.0)
                VisualStateManager.GoToState(s, "Actived", true);
            else
                VisualStateManager.GoToState(s, "Normal", true);
        }
    }
}
