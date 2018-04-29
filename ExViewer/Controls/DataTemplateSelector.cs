using System;
using System.Collections.ObjectModel;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;

namespace ExViewer.Controls
{
    [ContentProperty(Name = nameof(Templates))]
    public class DataTemplateSelector : Windows.UI.Xaml.Controls.DataTemplateSelector
    {
        public DataTemplateSelector()
        {
            this.Templates = new DataTemplateCollection(this);
        }

        public DataTemplateCollection Templates
        {
            get;
        }

        public DataTemplate DefaultTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item != null)
            {
                foreach (var template in this.Templates)
                {
                    if (template.KeyType is null)
                    {
                        continue;
                    }

                    if (template.KeyType.IsInstanceOfType(item))
                    {
                        return template.Value;
                    }
                }
            }
            if (this.DefaultTemplate != null)
            {
                return this.DefaultTemplate;
            }

            return base.SelectTemplateCore(item);
        }
    }

    public class DataTemplateCollection : Collection<DataTemplateKeyValuePair>
    {
        internal DataTemplateCollection(DataTemplateSelector parent)
        {
            this.parent = parent;
        }

        private readonly DataTemplateSelector parent;

        protected override void SetItem(int index, DataTemplateKeyValuePair item)
        {
            base.SetItem(index, item);
        }

        protected override void InsertItem(int index, DataTemplateKeyValuePair item)
        {
            base.InsertItem(index, item);
        }

        protected override void ClearItems()
        {
            base.ClearItems();
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
        }
    }

    [ContentProperty(Name = nameof(Value))]
    public class DataTemplateKeyValuePair : DependencyObject
    {
        public string Key
        {
            get => (string)GetValue(KeyProperty); set => SetValue(KeyProperty, value);
        }

        public Type KeyType
        {
            get; private set;
        }

        // Using a DependencyProperty as the backing store for Key.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(string), typeof(DataTemplateKeyValuePair), new PropertyMetadata("", KeyChangedCallback));

        private static void KeyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (DataTemplateKeyValuePair)d;
            var n = e.NewValue.ToString();
            if (string.IsNullOrWhiteSpace(n))
            {
                sender.KeyType = null;
                return;
            }
            sender.KeyType = Type.GetType(n, true);
        }

        public DataTemplate Value
        {
            get => (DataTemplate)GetValue(ValueProperty); set => SetValue(ValueProperty, value);
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(DataTemplate), typeof(DataTemplateKeyValuePair), new PropertyMetadata(null));
    }
}
