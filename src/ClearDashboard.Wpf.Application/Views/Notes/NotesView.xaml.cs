using System;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.ViewModels.Notes;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using SIL.IO.FileLock;
using ClearDashboard.DAL.ViewModels;

namespace ClearDashboard.Wpf.Application.Views.Notes
{
    public partial class NotesView : UserControl
    {
        private NotesViewModel _vm;
        public NotesView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _vm = (NotesViewModel)DataContext;
        }
        private void SelectionLabelsChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.FilterLabels.Clear();
            foreach (var label in ((ListBox)sender).SelectedItems)
            {
                if (label is DAL.Alignment.Notes.Label) 
                {
                    _vm.FilterLabels.Add((DAL.Alignment.Notes.Label) label);
                }
            }
            _vm.NotesCollectionView.Refresh();
        }
        private void SelectionUsersChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.FilterUsers.Clear();
            foreach (var user in ((ListBox)sender).SelectedItems)
            {
                if (user is string)
                {
                    _vm.FilterUsers.Add((string)user);
                }
            }
            _vm.NotesCollectionView.Refresh();
        }

        private void FiltersExpander_OnCollapsed(object sender, RoutedEventArgs e)
        {
            FiltersExpander.Width = 24;
            FiltersScrollViewer.Width = 24;

            FiltersColumn.Width = new GridLength(1, GridUnitType.Auto);

            //FiltersSplitter.Visibility = Visibility.Collapsed;
        }

        private void FiltersExpander_OnExpanded(object sender, RoutedEventArgs e)
        {
            if (FiltersExpander != null && FiltersScrollViewer != null)
            {
                FiltersExpander.Width = 200;
                FiltersScrollViewer.Width = 200;

                FiltersColumn.Width = new GridLength(1, GridUnitType.Auto);
                //FiltersSplitter.Visibility = Visibility.Visible;
            }
        }

        private void FiltersSplitter_OnDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (FiltersColumn.ActualWidth < 75)
            {
                FiltersScrollViewer.Visibility = Visibility.Collapsed;
            }
            else
            {
                FiltersScrollViewer.Visibility = Visibility.Visible;
            }

            FiltersExpander.Width = FiltersColumn.ActualWidth;
            FiltersScrollViewer.Width = FiltersColumn.ActualWidth;

        }
    }
}
