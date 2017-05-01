using ExClient;
using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Opportunity.MvvmUniverse;

namespace ExViewer.Controls
{
    public class TagPresenter : Control
    {
        public TagPresenter()
        {
            this.DefaultStyleKey = typeof(TagPresenter);
        }

        private TextBlock presenter;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.presenter = GetTemplateChild("Presenter") as TextBlock;
            applyTag(this.GalleryTag);
        }

        private void applyTag(Tag value)
        {
            if (this.presenter == null)
                return;
            this.presenter.ClearValue(TextBlock.TextProperty);
            if (value.Content == null)
                return;
            var dc = value.GetDisplayContentAsync();
            if (dc.Status != AsyncStatus.Completed)
                this.presenter.Text = value.Content;
            dc.Completed = this.loadDisplayContentCompleted;
        }

        private void loadDisplayContentCompleted(IAsyncOperation<string> sender, AsyncStatus e)
        {
            if (e == AsyncStatus.Completed && this.presenter != null)
            {
                var t = sender.GetResults();
                Opportunity.MvvmUniverse.Helpers.DispatcherHelper.BeginInvokeOnUIThread(() => this.presenter.Text = t);
            }
        }

        public Tag GalleryTag
        {
            get => (Tag)GetValue(GalleryTagProperty);
            set => SetValue(GalleryTagProperty, value);
        }

        // Using a DependencyProperty as the backing store for GalleryTag.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GalleryTagProperty =
            DependencyProperty.Register("GalleryTag", typeof(Tag), typeof(TagPresenter), new PropertyMetadata(default(Tag), galleryTagChanged));

        private static void galleryTagChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((TagPresenter)sender).applyTag((Tag)args.NewValue);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var r = base.ArrangeOverride(finalSize);
            if (this.presenter == null)
                return r;
            if (this.presenter.ActualHeight > r.Height || this.presenter.ActualWidth > r.Width)
                ToolTipService.SetToolTip(this, this.presenter.Text);
            else
                this.ClearValue(ToolTipService.ToolTipProperty);
            return r;
        }
    }
}
