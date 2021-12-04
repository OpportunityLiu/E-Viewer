using ExClient.Tagging;
using ExViewer.Controls.TagSuggestion;
using ExViewer.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace ExViewer.Controls
{
    public static class TagSuggestionService
    {
        public static InputScope GetInputScope(DependencyObject obj)
        {
            return (InputScope)obj.GetValue(InputScopeProperty);
        }

        public static void SetInputScope(DependencyObject obj, InputScope value)
        {
            obj.SetValue(InputScopeProperty, value);
        }

        // Using a DependencyProperty as the backing store for InputScope.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputScopeProperty =
            DependencyProperty.RegisterAttached("InputScope", typeof(InputScope), typeof(TagSuggestionService), new PropertyMetadata(null, InputScopePropertyChanged));

        private static async void InputScopePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (AutoSuggestBox)d;
            var tb = default(TextBox);
            tb = sender.Descendants<TextBox>().FirstOrDefault();
            if (tb != null)
            {
                tb.InputScope = (InputScope)e.NewValue;
                return;
            }
            await sender.Dispatcher.Yield();
            tb = sender.Descendants<TextBox>().FirstOrDefault();
            if (tb != null)
            {
                tb.InputScope = (InputScope)e.NewValue;
                return;
            }
            await sender.Dispatcher.YieldIdle();
            tb = sender.Descendants<TextBox>().FirstOrDefault();
            if (tb != null)
            {
                tb.InputScope = (InputScope)e.NewValue;
                return;
            }
        }

        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(TagSuggestionService), new PropertyMetadata(false, IsEnabledPropertyChanged));

        private static void IsEnabledPropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            var sender = (AutoSuggestBox)dp;
            var oldValue = (bool)e.OldValue;
            var newValue = (bool)e.NewValue;
            if (oldValue == newValue)
            {
                return;
            }

            if (newValue)
            {
                sender.TextChanged += asb_TextChanged;
                sender.QuerySubmitted += asb_QuerySubmitted;
                sender.LostFocus += asb_LostFocus;
            }
            else
            {
                sender.TextChanged -= asb_TextChanged;
                sender.QuerySubmitted -= asb_QuerySubmitted;
                sender.LostFocus -= asb_LostFocus;
            }
        }

        public static string GetSeparator(DependencyObject obj)
        {
            return (string)obj.GetValue(SeparatorProperty);
        }

        public static void SetSeparator(DependencyObject obj, string value)
        {
            obj.SetValue(SeparatorProperty, value);
        }

        // Using a DependencyProperty as the backing store for Separator.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SeparatorProperty =
            DependencyProperty.RegisterAttached("Separator", typeof(string), typeof(TagSuggestionService), new PropertyMetadata(" "));

        public static bool GetUseHistory(DependencyObject obj)
        {
            return (bool)obj.GetValue(UseHistoryProperty);
        }

        public static void SetUseHistory(DependencyObject obj, bool value)
        {
            obj.SetValue(UseHistoryProperty, value);
        }

        // Using a DependencyProperty as the backing store for UseHistory.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UseHistoryProperty =
            DependencyProperty.RegisterAttached("UseHistory", typeof(bool), typeof(TagSuggestionService), new PropertyMetadata(true));

        public static ICommand GetSubmitCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(SubmitCommandProperty);
        }

        public static void SetSubmitCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(SubmitCommandProperty, value);
        }

        // Using a DependencyProperty as the backing store for SubmitCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SubmitCommandProperty =
            DependencyProperty.RegisterAttached("SubmitCommand", typeof(ICommand), typeof(TagSuggestionService), new PropertyMetadata(null));

        public static int GetStateCode(DependencyObject obj)
        {
            return (int)obj.GetValue(StateCodeProperty);
        }

        public static void SetStateCode(DependencyObject obj, int value)
        {
            obj.SetValue(StateCodeProperty, value);
        }
        public static void IncreaseStateCode(DependencyObject obj)
        {
            SetStateCode(obj, GetStateCode(obj) + 1);
        }

        // Using a DependencyProperty as the backing store for StateCode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StateCodeProperty =
            DependencyProperty.RegisterAttached("StateCode", typeof(int), typeof(TagSuggestionService), new PropertyMetadata(0));

        private static async void asb_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var needAutoComplete = args.Reason == AutoSuggestionBoxTextChangeReason.UserInput;
            var currentState = GetStateCode(sender);
            if (needAutoComplete)
            {
                var r = await loadSuggestion(sender);
                if (args.CheckCurrent() && currentState == GetStateCode(sender))
                {
                    sender.ItemsSource = r;
                }
            }
        }

        private static async void asb_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            sender.ItemsSource = null;
            if (args.ChosenSuggestion is null || autoCompleteFinished(args.ChosenSuggestion))
            {
                var command = GetSubmitCommand(sender);
                if (command.CanExecute(args.QueryText))
                {
                    command.Execute(args.QueryText);
                }
            }
            else
            {
                sender.Focus(FocusState.Keyboard);
                // workaround for IME candidates, which will clean input.
                await sender.Dispatcher.YieldIdle();
                sender.Text = args.ChosenSuggestion.ToString();
            }
        }

        private static void asb_LostFocus(object dp, RoutedEventArgs e)
        {
            var sender = (AutoSuggestBox)dp;
            sender.ItemsSource = null;
        }

        public class SearchHistory
        {
            public string Title { get; set; }
            public string Highlight { get; set; }
            public DateTimeOffset Time { get; set; }

            public override string ToString() => Title;
        }

        private class HistoryRecordEqulityComparer : IEqualityComparer<HistoryRecord>
        {
            public bool Equals(HistoryRecord x, HistoryRecord y)
            {
                return x.Title == y.Title;
            }

            public int GetHashCode(HistoryRecord obj)
            {
                return (obj?.Title ?? "").GetHashCode();
            }
        }

        private static readonly IEqualityComparer<HistoryRecord> historyComparer = new HistoryRecordEqulityComparer();

        private class TagRecordEqulityComparer : IEqualityComparer<ITagRecord>
        {
            public bool Equals(ITagRecord x, ITagRecord y)
            {
                return x.TagToString() == y.TagToString();
            }

            public int GetHashCode(ITagRecord obj)
            {
                return (obj?.TagToString() ?? "").GetHashCode();
            }
        }

        private static readonly IEqualityComparer<ITagRecord> tagComparer = new TagRecordEqulityComparer();

        private static IAsyncOperation<IReadOnlyList<object>> loadSuggestion(AutoSuggestBox sender)
        {
            var input = sender.Text;
            var sep = GetSeparator(sender);
            var useHistory = GetUseHistory(sender);
            return Task.Run<IReadOnlyList<object>>(() =>
            {
                input = input?.Trim() ?? "";
                using (var db = new HistoryDb())
                {
                    var history = default(IEnumerable<SearchHistory>);
                    if (useHistory)
                        history = db.HistorySet
                            .Where(sh => (sh.Type == HistoryRecordType.Search || sh.Type == HistoryRecordType.Favorites) && sh.Title.Contains(input))
                            .OrderByDescending(sh => sh.TimeStamp)
                            .ToList().Distinct(historyComparer)
                            .Select(sh => new SearchHistory
                            {
                                Title = sh.Title,
                                Time = sh.Time,
                                Highlight = input,
                            });
                    else
                        history = Enumerable.Empty<SearchHistory>();
                    splitKeyword(sep, input, out var lastwordNs, out var lastword, out var previous);
                    var dictionary = default(IEnumerable<ITagRecord>);
                    if (!string.IsNullOrEmpty(lastword) && lastwordNs != Namespace.Unknown)
                    {
                        var suffix = sep.EndsWith(" ") ? sep : sep + " ";
                        dictionary = TagRecordFactory.GetTranslatedRecords(lastword, lastwordNs)
                            .Concat<ITagRecord>(TagRecordFactory.GetRecords(lastword, lastwordNs))
                            .Where(t => t != null)
                            .OrderByDescending(t => t.Score)
                            .Take(25)
                            .Distinct(tagComparer)
                            .Select(tag => tag.SetPrefix(previous).SetSuffix(suffix));
                    }
                    else
                    {
                        dictionary = Enumerable.Empty<ITagRecord>();
                    }
                    try
                    {
                        return ((IEnumerable<object>)dictionary).Concat(history).ToList();
                    }
                    catch (InvalidOperationException)
                    {
                        //Collection changed
                        return null;
                    }
                }
            }).AsAsyncOperation();
        }

        private static void splitKeyword(string sep, string input, out Namespace lastwordNs, out string lastword, out string previous)
        {
            if (string.IsNullOrEmpty(input))
            {
                previous = "";
                lastword = null;
                lastwordNs = Namespace.Unknown;
                return;
            }
            var quoteCount = input.Count(c => c == '"');
            var lastterm = default(string);
            if (quoteCount == 0)
            {
                var index = input.LastIndexOf(sep);
                if (index < 0)
                {
                    index = 0;
                }
                else
                {
                    index += sep.Length;
                }

                lastterm = input.Substring(index);
                previous = input.Substring(0, input.Length - lastterm.Length);
            }
            else if (quoteCount % 2 == 0)
            {
                if (input[input.Length - 1] != '"')
                {
                    var qp = input.LastIndexOf('"');
                    var sp = input.LastIndexOf(sep, input.Length - 1, input.Length - qp);
                    if (sp != -1)
                    {
                        lastterm = input.Substring(sp + 1);
                        previous = input.Substring(0, input.Length - lastterm.Length);
                    }
                    else
                    {
                        lastterm = input.Substring(qp + 1);
                        previous = input.Substring(0, input.Length - lastterm.Length) + sep;
                    }
                }
                else
                {
                    lastterm = null;
                    previous = input;
                }
            }
            else
            {
                var qp = input.LastIndexOf('"');
                var sp = input.LastIndexOf(sep, qp, qp + 1);
                if (qp == 0)
                {
                    previous = "";
                    lastterm = input.Substring(qp + 1).Trim();
                }
                else if (sp == -1)
                {
                    previous = "";
                    lastterm = input;
                }
                else
                {
                    previous = input.Substring(0, sp + 1);
                    lastterm = input.Substring(sp + 1);
                }
            }
            if (string.IsNullOrEmpty(lastterm))
            {
                lastword = null;
                lastwordNs = Namespace.Unknown;
                return;
            }
            if (lastterm[0] == '-')
            {
                lastterm = lastterm.Substring(1);
                previous = previous + "-";
            }
            var splited = lastterm.Split(tagSplit, 2);
            if (splited.Length == 1)
            {
                lastwordNs = Namespace.Temp;
                lastword = lastterm;
            }
            else
            {
                if (!NamespaceExtention.TryParse(splited[0], out lastwordNs))
                {
                    lastwordNs = Namespace.Unknown;
                }

                lastword = splited[1];
            }
            lastword = lastword.Trim(wordTrim);
        }

        private static readonly char[] wordTrim = new[] { '"', '$', ' ' };
        private static readonly char[] tagSplit = new[] { ':' };

        private static bool autoCompleteFinished(object selectedSuggestion)
        {
            if (selectedSuggestion is SearchHistory)
                return true;
            return false;
        }
    }
}
