using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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

        public static void HookItemPropertyChanged<TItem>(this ObservableCollection<TItem> bindableCollection, PropertyChangedEventHandler handler)
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
    }
}
