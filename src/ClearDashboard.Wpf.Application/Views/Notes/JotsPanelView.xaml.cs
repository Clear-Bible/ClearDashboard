﻿using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Events.Notes;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.Notes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ClearDashboard.Wpf.Application.Views.Notes
{
    public partial class JotsPanelView : INotifyPropertyChanged
    {
        private JotsPanelViewModel _vm;
        public JotsPanelView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _vm = (JotsPanelViewModel)DataContext;
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

        private async void RadioButton_Open_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = e.Source as RadioButton;
            var notesViewModel = radioButton?.DataContext as JotsPanelViewModel;
            var noteViewModel = notesViewModel.SelectedNoteViewModel;
            if (notesViewModel != null && noteViewModel != null)
            {
                await notesViewModel.UpdateNoteStatus(noteViewModel, NoteStatus.Open);
            }
        }
        private async void RadioButton_Resolved_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = e.Source as RadioButton;
            var notesViewModel = radioButton?.DataContext as JotsPanelViewModel;
            var noteViewModel = notesViewModel.SelectedNoteViewModel;
            if (notesViewModel != null && noteViewModel != null)
            {
                await notesViewModel.UpdateNoteStatus(noteViewModel, NoteStatus.Resolved);
            }
        }

        private async void OnNoteSeen(object sender, RoutedEventArgs e)
        {
            var args = e as NoteSeenEventArgs;
            if (_vm != null && 
                args != null && 
                args.NoteViewModel != null &&
                args.Seen != null
            )
            {
                await _vm.UpdateNoteSeen(args.NoteViewModel, (bool) args.Seen);
            }
        }   
        private async void OnNoteReplyAdd(object sender, RoutedEventArgs e)
        {
            var args = e as NoteReplyAddEventArgs;
            if ( _vm != null &&
                args != null && 
                args.Text != null && 
                args.Text != string.Empty && 
                args.NoteViewModelWithReplies != null
            )
            {
                await _vm.AddReplyToNote(args.NoteViewModelWithReplies, args.Text);
                //NoteEditorScrollView.ScrollToEnd();
                Telemetry.IncrementMetric(Telemetry.TelemetryDictionaryKeys.NoteReplyCount, 1);
            }
            
        }

        public bool RepliesExpanded { get; set; } = true;
        public Visibility RepliesVisibility => RepliesExpanded ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ExpandButtonVisibility => RepliesExpanded ? Visibility.Collapsed : Visibility.Visible;
        public Visibility CollapseButtonVisibility => RepliesExpanded ? Visibility.Visible : Visibility.Hidden;
        private void OnRepliesButtonClick(object sender, RoutedEventArgs e)
        {
            RepliesExpanded = !RepliesExpanded;
            OnPropertyChanged(nameof(RepliesExpanded));
            OnPropertyChanged(nameof(RepliesVisibility));
            OnPropertyChanged(nameof(ExpandButtonVisibility));
            OnPropertyChanged(nameof(CollapseButtonVisibility));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkbox && checkbox.DataContext is NoteViewModel note)
            {
                note.IsSelectedForBulkAction = true;
            }
        }

        private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkbox && checkbox.DataContext is NoteViewModel note)
            {
                note.IsSelectedForBulkAction = false;
            }
            
        }

        private void ToggleButton_OnCheckedAll(object sender, RoutedEventArgs e)
        {
            _vm.CheckAllFilteredNoteViewModels();
        }

        private void ToggleButton_OnUncheckedAll(object sender, RoutedEventArgs e)
        {
            _vm.UncheckAllFilteredNoteViewModels();
        }

        private void NoteTextButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.DataContext is NoteViewModel note)
            {
                var mousePosition = this.PointToScreen(System.Windows.Input.Mouse.GetPosition(button));
                _vm.DisplayJotsEditor(null, note);
            }
            
        }

        private void GridNotes_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                ScrollBar.LineDownCommand.Execute(null, e.OriginalSource as IInputElement);
            }
            else if (e.Delta > 0)
            {
                ScrollBar.LineUpCommand.Execute(null, e.OriginalSource as IInputElement);
            }
            e.Handled = true;
        }
    }
}
