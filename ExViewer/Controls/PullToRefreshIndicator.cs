using Microsoft.Toolkit.Uwp.UI.Controls;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ExViewer.Controls
{
    public class PullToRefreshIndicator : Control
    {
        public PullToRefreshIndicator()
        {
            this.DefaultStyleKey = typeof(PullToRefreshIndicator);
            this.Loading += this.PullToRefreshIndicator_Loading;
            this.Unloaded += this.PullToRefreshIndicator_Unloaded;
        }

        private void PullToRefreshIndicator_Loading(FrameworkElement sender, object args)
        {
            var s = (PullToRefreshIndicator)sender;
            var pv = s.Ancestors<PullToRefreshListView>().First();
            s.parent = pv;
        }

        private PullToRefreshListView p;

        private PullToRefreshListView parent
        {
            get => this.p;
            set
            {
                if (this.p != null)
                    this.p.PullProgressChanged -= this.Parent_PullProgressChanged;
                this.p = value;
                if (this.p != null)
                    this.p.PullProgressChanged += this.Parent_PullProgressChanged;
                this.ClearValue(PullProgressProperty);
            }
        }

        private void PullToRefreshIndicator_Unloaded(object sender, RoutedEventArgs e)
        {
            ((PullToRefreshIndicator)sender).parent = null;
        }

        private void Parent_PullProgressChanged(object sender, RefreshProgressEventArgs e)
        {
            this.PullProgress = e.PullProgress;
        }

        public double PullProgress
        {
            get => (double)GetValue(PullProgressProperty);
            set => SetValue(PullProgressProperty, value);
        }

        // Using a DependencyProperty as the backing store for PullProgress.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PullProgressProperty =
            DependencyProperty.Register("PullProgress", typeof(double), typeof(PullToRefreshIndicator), new PropertyMetadata(0d, PullProgressChanged));

        private static void PullProgressChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var s = (PullToRefreshIndicator)sender;
            if ((double)e.NewValue == 1.0)
                VisualStateManager.GoToState(s, "Actived", true);
            else
                VisualStateManager.GoToState(s, "Normal", true);
        }
    }
}
