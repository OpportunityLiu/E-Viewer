using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;
using Windows.Foundation.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExClient
{
    public class ObservableCollection<T>
        : System.Collections.ObjectModel.ObservableCollection<T>
    {
        public ObservableCollection()
        {
        }

        protected bool Set<TProp>(ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            if(Equals(field, value))
                return false;
            ForceSet(ref field, value, propertyName);
            return true;
        }

        protected bool Set<TProp>(ref TProp field, TProp value, string addtionalPropertyName, [CallerMemberName]string propertyName = null)
        {
            if(Equals(field, value))
                return false;
            ForceSet(ref field, value, addtionalPropertyName, propertyName);
            return true;
        }

        protected bool Set<TProp>(ref TProp field, TProp value, string addtionalPropertyName0, string addtionalPropertyName1, [CallerMemberName]string propertyName = null)
        {
            if(Equals(field, value))
                return false;
            ForceSet(ref field, value, addtionalPropertyName0, addtionalPropertyName1, propertyName);
            return true;
        }

        protected bool Set<TProp>(ref TProp field, TProp value, IEnumerable<string> addtionalPropertyNames, [CallerMemberName]string propertyName = null)
        {
            if(Equals(field, value))
                return false;
            ForceSet(ref field, value, addtionalPropertyNames, propertyName);
            return true;
        }

        protected void ForceSet<TProp>(ref TProp field, TProp value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            RaisePropertyChanged(propertyName);
        }

        protected void ForceSet<TProp>(ref TProp field, TProp value, string addtionalPropertyName, [CallerMemberName]string propertyName = null)
        {
            field = value;
            RaisePropertyChanged(propertyName, addtionalPropertyName);
        }

        protected void ForceSet<TProp>(ref TProp field, TProp value, string addtionalPropertyName0, string addtionalPropertyName1, [CallerMemberName]string propertyName = null)
        {
            field = value;
            RaisePropertyChanged(propertyName, addtionalPropertyName0, addtionalPropertyName1);
        }

        protected void ForceSet<TProp>(ref TProp field, TProp value, IEnumerable<string> addtionalPropertyNames, [CallerMemberName]string propertyName = null)
        {
            field = value;
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
                foreach(var item in addtionalPropertyNames)
                {
                    base.OnPropertyChanged(new PropertyChangedEventArgs(item));
                }
            });
        }

        /// <summary>
        /// Add items into collection.
        /// </summary>
        /// <param name="items">Items to add.</param>
        /// <returns>Count of added items.</returns>
        public int AddRange(IEnumerable<T> items)
        {
            CheckReentrancy();
            var count = 0;
            foreach(var item in items)
            {
                this.Items.Add(item);
                count++;
            }
            if(count == 0)
                return 0;
            var startingIndex = this.Count - count;
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new AddRangeInfo(this, startingIndex, count), startingIndex));
            RaisePropertyChanged(nameof(Count), "Item[]");
            return count;
        }

        private class AddRangeInfo : IList
        {
            private int count;
            private ObservableCollection<T> parent;
            private int startingIndex;

            public AddRangeInfo(ObservableCollection<T> parent, int startingIndex, int count)
            {
                this.parent = parent;
                this.startingIndex = startingIndex;
                this.count = count;
            }

            public object this[int index]
            {
                get
                {
                    if((uint)index > (uint)this.count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    return this.parent[this.startingIndex + index];
                }
                set
                {
                    throw new InvalidOperationException();
                }
            }

            public int Count => this.count;

            public bool IsFixedSize => true;

            public bool IsReadOnly => true;

            public bool IsSynchronized => false;

            public object SyncRoot => null;

            public int Add(object value)
            {
                throw new InvalidOperationException();
            }

            public void Clear()
            {
                throw new InvalidOperationException();
            }

            public bool Contains(object value)
            {
                foreach(var item in this)
                {
                    if(item == value)
                        return true;
                }
                return false;
            }

            public void CopyTo(Array array, int index)
            {
                for(var i = 0; i < this.count; i++)
                {
                    array.SetValue(this[i], i);
                }
            }

            public IEnumerator GetEnumerator()
            {
                for(var i = 0; i < this.count; i++)
                {
                    yield return this[i];
                }
            }

            public int IndexOf(object value)
            {
                for(var i = 0; i < this.count; i++)
                {
                    if(this[i] == value)
                        return i;
                }
                return -1;
            }

            public void Insert(int index, object value)
            {
                throw new InvalidOperationException();
            }

            public void Remove(object value)
            {
                throw new InvalidOperationException();
            }

            public void RemoveAt(int index)
            {
                throw new InvalidOperationException();
            }
        }

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void RaisePropertyChanged(string propertyName0, string propertyName1)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName0));
                base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName1));
            });
        }

        protected void RaisePropertyChanged(string propertyName0, string propertyName1, string propertyName2)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName0));
                base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName1));
                base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName2));
            });
        }

        protected void RaisePropertyChanged(params string[] propertyNames)
        {
            this.RaisePropertyChanged((IEnumerable<string>)propertyNames);
        }

        protected void RaisePropertyChanged(IEnumerable<string> propertyNames)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                foreach(var item in propertyNames)
                {
                    base.OnPropertyChanged(new PropertyChangedEventArgs(item));
                }
            });
        }

        protected sealed override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => base.OnCollectionChanged(e));
        }

        protected sealed override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => base.OnPropertyChanged(e));
        }
    }
}
