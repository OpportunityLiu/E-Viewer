using ExClient.Settings;
using Opportunity.MvvmUniverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ExViewer.Controls
{
    public sealed partial class ExcludedLanguagesSelector : UserControl
    {
        public ExcludedLanguagesSelector()
        {
            InitializeComponent();
            foreach (var item in filters)
            {
                item.PropertyChanged += filter_PropertyChanged;
            }
        }

        private readonly ExcludedLanguageFilter[] filters = new []
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

        private static void excludedLanguagesChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var oldS = e.OldValue.ToString();
            var newS = e.NewValue.ToString();
            if (string.Equals(oldS, newS))
            {
                return;
            }

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
                    {
                        break;
                    }

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
                    {
                        break;
                    }

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
                    {
                        break;
                    }

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
            if (changing)
            {
                return;
            }

            if (e.PropertyName == nameof(ExcludedLanguageFilter.All))
            {
                return;
            }

            var r = new List<ExcludedLanguage>();
            foreach (var item in filters)
            {
                if (item.Original)
                {
                    r.Add(item.Language);
                }

                if (item.Translated)
                {
                    r.Add(item.Language + 1024);
                }

                if (item.Rewrite)
                {
                    r.Add(item.Language + 2048);
                }
            }
            ExcludedLanguages = ExcludedLanguagesSettingProvider.ToString(r);
        }

        private void userControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var contentWidth = cdColumn.MinWidth * 4;
            var leftWidth = lvLeftHeader.ActualWidth;
            contentWidth = Math.Max(contentWidth, ActualWidth - leftWidth);
            bdTopLeftHeader.Width = leftWidth;
            gdTopHeader.Width = contentWidth;
            lvContent.Width = contentWidth;
        }
    }

    internal sealed class ExcludedLanguageFilter : ObservableObject
    {
        public ExcludedLanguageFilter(ExcludedLanguage language)
        {
            Name = language.ToDisplayNameString();
            Language = language;
        }

        public ExcludedLanguage Language { get; }

        public string Name { get; }

        public bool OriginalEnabled => Language != ExcludedLanguage.Default;

        private bool original;

        public bool Original
        {
            get => original;
            set => setValue(value, ref original);
        }

        private bool translated;

        public bool Translated
        {
            get => translated;
            set => setValue(value, ref translated);
        }

        private bool rewrite;

        public bool Rewrite
        {
            get => rewrite;
            set => setValue(value, ref rewrite);
        }

        private void setValue(bool value, ref bool field, [CallerMemberName]string propName = null)
        {
            var oldAll = All;
            if (Set(ref field, value, propName) && oldAll != All)
            {
                OnPropertyChanged(nameof(All));
            }
        }

        public bool All
        {
            get
            {
                if (Language != ExcludedLanguage.Default)
                {
                    return original && translated && rewrite;
                }

                return translated && rewrite;
            }
            set
            {
                if (value == All)
                {
                    return;
                }

                if (Language != ExcludedLanguage.Default)
                {
                    original = value;
                }

                translated = value;
                rewrite = value;
                OnPropertyChanged(default(string));
            }
        }
    }
}
