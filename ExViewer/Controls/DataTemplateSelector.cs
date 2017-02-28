using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using System.Reflection;

namespace ExViewer.Controls
{
    [ContentProperty(Name = nameof(Templates))]
    public class DataTemplateSelector : Windows.UI.Xaml.Controls.DataTemplateSelector
    {
        private class DataTemplateCollection : Collection<DataTemplateKeyValuePair>
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

        public DataTemplateSelector()
        {
            this.Templates = new DataTemplateCollection(this);
        }

        public ICollection<DataTemplateKeyValuePair> Templates
        {
            get;
        }

        public DataTemplate DefaultTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if(item != null)
            {
                var tName = item.GetType().FullName;
                foreach(var template in this.Templates)
                {
                    if(template.Key == tName)
                        return template.Value;
                }
            }
            if(this.DefaultTemplate != null)
                return this.DefaultTemplate;
            return base.SelectTemplateCore(item);
        }
    }

    [ContentProperty(Name = nameof(Value))]
    public class DataTemplateKeyValuePair : DependencyObject
    {
        public string Key
        {
            get { return (string)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Key.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(string), typeof(DataTemplateKeyValuePair), new PropertyMetadata(""));

        public DataTemplate Value
        {
            get { return (DataTemplate)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(DataTemplate), typeof(DataTemplateKeyValuePair), new PropertyMetadata(null));
    }
}
