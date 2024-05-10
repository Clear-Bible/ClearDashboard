using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace ClearDashboard.Wpf.Application.Extensions
{

    public static class BindableCollectionExtensions
    {
        public static void SetOnCollectionItemPropertyChanged<T, TItem>(this T _this, PropertyChangedEventHandler handler)
            where T : INotifyCollectionChanged, ICollection<TItem> 
            where TItem : INotifyPropertyChanged
        {
            _this.CollectionChanged += (sender, e) => {
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems)
                    {
                        ((TItem)item).PropertyChanged += handler;
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems)
                    {
                        ((TItem)item).PropertyChanged -= handler;
                    }
                }
            };
        }

        public static void HookItemPropertyChanged<TItem>(this BindableCollection<TItem> bindableCollection, PropertyChangedEventHandler handler)
            where TItem : INotifyPropertyChanged
        {
            foreach (var item in bindableCollection)
            {
                item.PropertyChanged += handler;
            }
        }

        public static void UnhookItemPropertyChanged<TItem>(this BindableCollection<TItem> bindableCollection, PropertyChangedEventHandler handler)
            where TItem : INotifyPropertyChanged
        {
            foreach (var item in bindableCollection)
            {
                item.PropertyChanged -= handler;
            }
        }


        /// <summary>
        /// Inserts a range of items into the collection.
        /// </summary>
        /// <remarks>This method uses reflection to add a range of items to the private 'Items' collection in the underlying ObservableCollection.</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="items"></param>
        public static void InsertRange<T>(this BindableCollection<T>? collection, IEnumerable<T>? items)
        {
         
            if (collection == null || items == null)
            {
                return;
            }

            var list = items as List<T> ?? items.ToList();

            if (list.Count == 0)
            {
                return;
            }

            var type = collection.GetType();

            if (type.BaseType == null)
            {
                return;
            }

            type.InvokeMember("CheckReentrancy", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, collection, null);
          
            var itemsProperty = type.BaseType.GetProperty("Items", BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
            if (itemsProperty == null)
            {
                return;
            }

            if (itemsProperty.GetValue(collection) is not IList<T> privateItems)
            {
                return;
            }

            foreach (var item in list)
            {
                privateItems.Add(item);
            }

            type.InvokeMember("OnPropertyChanged", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null,
                collection, new object[] { new PropertyChangedEventArgs("Count") });

            type.InvokeMember("OnPropertyChanged", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null,
                collection, new object[] { new PropertyChangedEventArgs("Item[]") });

            type.InvokeMember("OnCollectionChanged", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null,
                collection, new object[] { new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset) });
        }
    }
}
