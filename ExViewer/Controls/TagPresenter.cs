﻿using ExClient;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ExViewer.Controls
{
    public class TagPresenter : Control
    {
        public TagPresenter()
        {
            DefaultStyleKey = typeof(TagPresenter);
        }

        private TextBlock presenter;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            presenter = GetTemplateChild("Presenter") as TextBlock;
            applyTag(GalleryTag);
        }

        private void applyTag(Tag value)
        {
            if(presenter == null)
                return;
            presenter.ClearValue(TextBlock.TextProperty);
            if(value == null)
                return;
            var dc = value.GetDisplayContentAsync();
            if(dc.Status != AsyncStatus.Completed)
                presenter.Text = value.Content;
            dc.Completed = loadDisplayContentCompleted;
        }

        private void loadDisplayContentCompleted(IAsyncOperation<string> sender, AsyncStatus e)
        {
            if(e == AsyncStatus.Completed && presenter != null)
            {
                var t = sender.GetResults();
                DispatcherHelper.CheckBeginInvokeOnUI(() => presenter.Text = t);
            }
        }

        public Tag GalleryTag
        {
            get
            {
                return (Tag)GetValue(GalleryTagProperty);
            }
            set
            {
                SetValue(GalleryTagProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for GalleryTag.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GalleryTagProperty =
            DependencyProperty.Register("GalleryTag", typeof(Tag), typeof(TagPresenter), new PropertyMetadata(null, galleryTagChanged));

        private static void galleryTagChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((TagPresenter)sender).applyTag((Tag)args.NewValue);
        }
    }
}