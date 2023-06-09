using Microsoft.Xaml.Behaviors;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Application.Behaviors
{

    /*
     
             <ListView ItemsSource="{Binding Log}" x:Name="StatusUpdate">                
                <i:Interaction.Behaviors>
                    <behaviours:AutoScrollToLastItemBehavior />
                </i:Interaction.Behaviors>
                <ListView.ItemTemplate>
                    <DataTemplate>
                        <TextBlock Text="{Binding}"/>
                    </DataTemplate>
                </ListView.ItemTemplate>

            </ListView>
     */



    /// <summary>
    /// From: https://pmichaels.net/2014/10/31/automatically-focus-on-the-newest-item-in-a-listview-using-winrt-mvvm-and-behaviors-sdk/
    /// </summary>
    public sealed class AutoScrollToLastItemBehavior : Behavior<ListView>
    {
        // Need to track whether we've attached to the collection changed event
        bool _collectionChangedSubscribed = false;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += SelectionChanged;

            // The ItemSource of the listView will not be set yet, 
            // so get a method that we can hook up to later
            AssociatedObject.DataContextChanged += DataContextChanged;
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScrollIntoView();
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ScrollIntoView();
        }

        private void ScrollIntoView()
        {
            int count = AssociatedObject.Items.Count;
            if (count > 0)
            {
                var last = AssociatedObject.Items[count - 1];
                AssociatedObject.ScrollIntoView(last);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= SelectionChanged;
            AssociatedObject.DataContextChanged -= DataContextChanged;

            // Detach from the collection changed event
            var collection = AssociatedObject.ItemsSource as INotifyCollectionChanged;
            if (collection != null && _collectionChangedSubscribed)
            {
                collection.CollectionChanged -= CollectionChanged;
                _collectionChangedSubscribed = false;

            }
        }

        private void DataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            // The ObservableCollection implements the INotifyCollectionChanged interface
            // However, if this is bound to something that doesn't then just don't hook the event
            var collection = AssociatedObject.ItemsSource as INotifyCollectionChanged;
            if (collection != null && !_collectionChangedSubscribed)
            {
                // The data context has been changed, so now hook 
                // into the collection changed event
                collection.CollectionChanged += CollectionChanged;
                _collectionChangedSubscribed = true;
            }
        }
    }
}
