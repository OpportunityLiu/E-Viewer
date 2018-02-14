using ExClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ExClient.Settings;
using System.Runtime.CompilerServices;
using Opportunity.MvvmUniverse;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Controls
{
    public sealed partial class ExcludedLanguagesSelector : UserControl
    {
        public ExcludedLanguagesSelector()
        {
            this.InitializeComponent();
            foreach (var item in this.filters)
            {
                item.PropertyChanged += this.filter_PropertyChanged;
            }
        }

        private List<ExcludedLanguageFilter> filters = new List<ExcludedLanguageFilter>()
        {
            new ExcludedLanguageFilter(ExcludedLanguage.Default),
            new ExcludedLanguageFilter(ExcludedLanguage.EnglishOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.ChineseOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.DutchOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.FrenchOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.GermanOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.HungarianOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.ItalianOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.KoreanOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.PolishOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.PortugueseOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.RussianOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.SpanishOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.ThaiOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.VietnameseOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.NotApplicableOriginal),
            new ExcludedLanguageFilter(ExcludedLanguage.OtherOriginal)
        };

        public string ExcludedLanguages
        {
            get => (string)GetValue(ExcludedLanguagesProperty);
            set => SetValue(ExcludedLanguagesProperty, value);
        }

        // Using a DependencyProperty as the backing store for ExcludedLanguages.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExcludedLanguagesProperty =
            DependencyProperty.Register("ExcludedLanguages", typeof(string), typeof(ExcludedLanguagesSelector), new PropertyMetadata("", excludedLanguagesChanged));

        private static char[] split = ", ".ToCharArray();

        private static void excludedLanguagesChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var oldS = e.OldValue.ToString();
            var newS = e.NewValue.ToString();
            if (string.Equals(oldS, newS))
                return;
            var el = ExcludedLanguagesSettingProvider.FromString(newS).OrderByDescending(k => k).ToList();
            var s = (ExcludedLanguagesSelector)sender;
            s.changing = true;
            try
            {
                var fs = s.filters;
                foreach (var item in fs)
                {
                    item.Original = false;
                    item.Translated = false;
                    item.Rewrite = false;
                }
                ushort offset = 0;
                foreach (var item in fs)
                {
                    if (el.Count == 0)
                        break;
                    var lan = item.Language;
                    var last = el[el.Count - 1];
                    if (lan + offset == last)
                    {
                        el.RemoveAt(el.Count - 1);
                        item.Original = true;
                    }
                    else
                    {
                        item.Original = false;
                    }
                }
                offset = 1024;
                foreach (var item in fs)
                {
                    if (el.Count == 0)
                        break;
                    var lan = item.Language;
                    var last = el[el.Count - 1];
                    if (lan + offset == last)
                    {
                        el.RemoveAt(el.Count - 1);
                        item.Translated = true;
                    }
                    else
                    {
                        item.Translated = false;
                    }
                }
                offset = 2048;
                foreach (var item in fs)
                {
                    if (el.Count == 0)
                        break;
                    var lan = item.Language;
                    var last = el[el.Count - 1];
                    if (lan + offset == last)
                    {
                        el.RemoveAt(el.Count - 1);
                        item.Rewrite = true;
                    }
                    else
                    {
                        item.Rewrite = false;
                    }
                }
            }
            finally
            {
                s.changing = false;
            }
        }

        private bool changing;

        private void filter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (this.changing)
                return;
            if (e.PropertyName == nameof(ExcludedLanguageFilter.All))
                return;
            var r = new List<ExcludedLanguage>();
            foreach (var item in this.filters)
            {
                if (item.Original)
                    r.Add(item.Language);
                if (item.Translated)
                    r.Add(item.Language + 1024);
                if (item.Rewrite)
                    r.Add(item.Language + 2048);
            }
            this.ExcludedLanguages = ExcludedLanguagesSettingProvider.ToString(r);
        }

        private void userControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var contentWidth = this.cdColumn.MinWidth * 4;
            var leftWidth = this.lvLeftHeader.ActualWidth;
            contentWidth = Math.Max(contentWidth, this.ActualWidth - leftWidth);
            this.bdTopLeftHeader.Width = leftWidth;
            this.gdTopHeader.Width = contentWidth;
            this.lvContent.Width = contentWidth;
        }
    }

    internal sealed class ExcludedLanguageFilter : ObservableObject
    {
        public ExcludedLanguageFilter(ExcludedLanguage language)
        {
            var lan = language.ToString();
            lan = lan.Substring(0, lan.Length - "Original".Length);
            this.Name = Strings.Resources.Controls.ExcludedLanguagesSelector.ExcludedLanguage.GetValue(lan);
            this.Language = language;
        }

        public ExcludedLanguage Language { get; }

        public string Name { get; }

        public bool OriginalEnabled => Language != ExcludedLanguage.Default;

        private bool original;

        public bool Original
        {
            get => this.original;
            set => setValue(value, ref this.original);
        }

        private bool translated;

        public bool Translated
        {
            get => this.translated;
            set => setValue(value, ref this.translated);
        }

        private bool rewrite;

        public bool Rewrite
        {
            get => this.rewrite;
            set => setValue(value, ref this.rewrite);
        }

        private void setValue(bool value, ref bool field, [CallerMemberName]string propName = null)
        {
            var oldAll = this.All;
            if (Set(ref field, value, propName) && oldAll != this.All)
                OnPropertyChanged(nameof(All));
        }

        public bool All
        {
            get
            {
                if (Language != ExcludedLanguage.Default)
                    return this.original && this.translated && this.rewrite;
                return this.translated && this.rewrite;
            }
            set
            {
                if (value == this.All)
                    return;
                if (Language != ExcludedLanguage.Default)
                    this.original = value;
                this.translated = value;
                this.rewrite = value;
                OnPropertyChanged(default(string));
            }
        }
    }
}
