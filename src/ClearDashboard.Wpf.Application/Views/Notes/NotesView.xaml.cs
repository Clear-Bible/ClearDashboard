using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.ViewModels.Notes;
using System.Windows;
using System.Windows.Controls;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Events.Notes;
using ClearBible.Engine.Utils;
using ClearDashboard.Wpf.Application.UserControls.Notes;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Collections.Notes;
using ClearDashboard.DataAccessLayer.Annotations;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Collections.Notes;
using ClearDashboard.Wpf.Application.Events.Notes;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Notes;
using Brushes = System.Windows.Media.Brushes;
using FontFamily = System.Windows.Media.FontFamily;
using FontStyle = System.Windows.FontStyle;
using NotesLabel = ClearDashboard.DAL.Alignment.Notes.Label;

namespace ClearDashboard.Wpf.Application.Views.Notes
{
    public partial class NotesView : INotifyPropertyChanged
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

        private async void RadioButton_Open_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = e.Source as RadioButton;
            var notesViewModel = radioButton?.DataContext as NotesViewModel;
            var noteViewModel = notesViewModel.SelectedNoteViewModel;
            if (notesViewModel != null && noteViewModel != null)
            {
                await notesViewModel.UpdateNoteStatus(noteViewModel, NoteStatus.Open);
            }
        }
        private async void RadioButton_Resolved_Checked(object sender, RoutedEventArgs e)
        {
            var radioButton = e.Source as RadioButton;
            var notesViewModel = radioButton?.DataContext as NotesViewModel;
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

        ////#region Static Routed Events

        /////// <summary>
        /////// Identifies the LabelAddedEvent routed event.
        /////// </summary>
        ////public static readonly RoutedEvent LabelAddedEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(LabelAdded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the LabelDeletedEvent routed event.
        /////// </summary>
        ////public static readonly RoutedEvent LabelDeletedEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(LabelDeleted), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the LabelDisassociatedEvent routed event.
        /////// </summary>
        ////public static readonly RoutedEvent LabelDisassociatedEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(LabelDisassociated), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the LabelGroupAddedEvent routed event.
        /////// </summary>
        ////public static readonly RoutedEvent LabelGroupAddedEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(LabelGroupAdded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the LabelGroupLabelAddedEvent routed event.
        /////// </summary>
        ////public static readonly RoutedEvent LabelGroupLabelAddedEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(LabelGroupLabelAdded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the LabelGroupLabelRemovedEvent routed event.
        /////// </summary>
        ////public static readonly RoutedEvent LabelGroupLabelRemovedEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(LabelGroupLabelRemoved), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the LabelGroupRemovedEvent routed event.
        /////// </summary>
        ////public static readonly RoutedEvent LabelGroupRemovedEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(LabelGroupRemoved), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the LabelGroupSelectedEvent routed event.
        /////// </summary>
        ////public static readonly RoutedEvent LabelGroupSelectedEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(LabelGroupSelected), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the LabelRemovedEvent routed event.
        /////// </summary>
        ////public static readonly RoutedEvent LabelRemovedEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(LabelRemoved), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the LabelSelectedEvent routed event.
        /////// </summary>
        ////public static readonly RoutedEvent LabelSelectedEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(LabelSelected), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the LabelUpdatedEvent routed event.
        /////// </summary>
        ////public static readonly RoutedEvent LabelUpdatedEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(LabelUpdated), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the NoteAdded routed event.
        /////// </summary>
        ////public static readonly RoutedEvent NoteAddedEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(NoteAdded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the NoteAssociationClicked routed event.
        /////// </summary>
        ////public static readonly RoutedEvent NoteAssociationClickedEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(NoteAssociationClicked), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the NoteAssociationDoubleClicked routed event.
        /////// </summary>
        ////public static readonly RoutedEvent NoteAssociationDoubleClickedEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(NoteAssociationDoubleClicked), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the NoteAssociationLeftButtonDown routed event.
        /////// </summary>
        ////public static readonly RoutedEvent NoteAssociationLeftButtonDownEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(NoteAssociationLeftButtonDown), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the NoteAssociationLeftButtonUp routed event.
        /////// </summary>
        ////public static readonly RoutedEvent NoteAssociationLeftButtonUpEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(NoteAssociationLeftButtonUp), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the NoteAssociationRightButtonDown routed event.
        /////// </summary>
        ////public static readonly RoutedEvent NoteAssociationRightButtonDownEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(NoteAssociationRightButtonDown), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the NoteAssociationRightButtonUp routed event.
        /////// </summary>
        ////public static readonly RoutedEvent NoteAssociationRightButtonUpEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(NoteAssociationRightButtonUp), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the NoteAssociationMouseEnter routed event.
        /////// </summary>
        ////public static readonly RoutedEvent NoteAssociationMouseEnterEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(NoteAssociationMouseEnter), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the NoteAssociationMouseLeaveEvent routed event.
        /////// </summary>
        ////public static readonly RoutedEvent NoteAssociationMouseLeaveEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(NoteAssociationMouseLeave), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the NoteDeleted routed event.
        /////// </summary>
        ////public static readonly RoutedEvent NoteDeletedEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(NoteDeleted), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the NoteEditorMouseEnter routed event.
        /////// </summary>
        ////public static readonly RoutedEvent NoteEditorMouseEnterEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(NoteEditorMouseEnter), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the NoteEditorMouseLeave routed event.
        /////// </summary>
        ////public static readonly RoutedEvent NoteEditorMouseLeaveEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(NoteEditorMouseLeave), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the NoteSeen routed event.
        /////// </summary>
        ////public static readonly RoutedEvent NoteSeenEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(NoteSeen), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the NoteReplyAdded routed event.
        /////// </summary>
        ////public static readonly RoutedEvent NoteReplyAddedEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(NoteReplyAdded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        ///// <summary>
        ///// Identifies the NoteSendToParatext routed event.
        ///// </summary>
        //public static readonly RoutedEvent NoteSendToParatextEvent = EventManager.RegisterRoutedEvent
        //    (nameof(NoteSendToParatext), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the NoteUpdated routed event.
        /////// </summary>
        ////public static readonly RoutedEvent NoteUpdatedEvent = EventManager.RegisterRoutedEvent
        ////    (nameof(NoteUpdated), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NoteDisplay));

        ////#endregion Static Routed Events
        ////#region Static Dependency Properties

        /////// <summary>
        /////// Identifies the AddMode dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty AddModeProperty = DependencyProperty.Register(nameof(AddMode), typeof(bool), typeof(NoteDisplay),
        ////    new PropertyMetadata(false, OnAddModeChanged));

        /////// <summary>
        /////// Identifies the CurrentUserId dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty CurrentUserIdProperty = DependencyProperty.Register(nameof(CurrentUserId), typeof(UserId), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the DefaultLabelGroup dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty DefaultLabelGroupProperty = DependencyProperty.Register(nameof(DefaultLabelGroup), typeof(LabelGroupViewModel), typeof(NoteDisplay));

        ///// <summary>
        ///// Identifies the EntityId dependency property.
        ///// </summary>
        //public static readonly DependencyProperty EntityIdsProperty = DependencyProperty.Register(nameof(EntityIds), typeof(EntityIdCollection), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the LabelBackground dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty LabelBackgroundProperty = DependencyProperty.Register(nameof(LabelBackground), typeof(SolidColorBrush), typeof(NoteDisplay),
        ////    new PropertyMetadata(Brushes.BlanchedAlmond));

        /////// <summary>
        /////// Identifies the LabelCornerRadius dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty LabelCornerRadiusProperty = DependencyProperty.Register(nameof(LabelCornerRadius), typeof(CornerRadius), typeof(NoteDisplay),
        ////    new PropertyMetadata(new CornerRadius(0)));

        /////// <summary>
        /////// Identifies the LabelFontSize dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty LabelFontSizeProperty = DependencyProperty.Register(nameof(LabelFontSize), typeof(double), typeof(NoteDisplay),
        ////    new PropertyMetadata(14d));

        /////// <summary>
        /////// Identifies the LabelGroups dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty LabelGroupsProperty = DependencyProperty.Register(nameof(LabelGroups), typeof(LabelGroupViewModelCollection), typeof(NoteDisplay));

        /////// <summary>
        /////// Identifies the LabelMargin dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty LabelMarginProperty = DependencyProperty.Register(nameof(LabelMargin), typeof(Thickness), typeof(NoteDisplay),
        ////    new PropertyMetadata(new Thickness(3, 0, 3, 0)));

        /////// <summary>
        /////// Identifies the LabelPadding dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty LabelPaddingProperty = DependencyProperty.Register(nameof(LabelPadding), typeof(Thickness), typeof(NoteDisplay),
        ////    new PropertyMetadata(new Thickness(10, 6, 10, 5)));

        /////// <summary>
        /////// Identifies the LabelSuggestions dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty LabelSuggestionsProperty = DependencyProperty.Register(nameof(LabelSuggestions), typeof(LabelCollection), typeof(NoteDisplay));

        ///// <summary>
        ///// Identifies the Note dependency property.
        ///// </summary>
        //public static readonly DependencyProperty NoteProperty = DependencyProperty.Register(nameof(Note), typeof(NoteViewModel), typeof(NoteDisplay),
        //    new PropertyMetadata(null, OnNotePropertyChanged));

        /////// <summary>
        /////// Identifies the NoteAssociationFontFamily dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty NoteAssociationFontFamilyProperty = DependencyProperty.Register(nameof(NoteAssociationFontFamily), typeof(FontFamily), typeof(NoteDisplay),
        ////    new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /////// <summary>
        /////// Identifies the NoteAssociationFontSize dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty NoteAssociationFontSizeProperty = DependencyProperty.Register(nameof(NoteAssociationFontSize), typeof(double), typeof(NoteDisplay),
        ////    new PropertyMetadata(14d));

        /////// <summary>
        /////// Identifies the NoteAssociationFontStyle dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty NoteAssociationFontStyleProperty = DependencyProperty.Register(nameof(NoteAssociationFontStyle), typeof(FontStyle), typeof(NoteDisplay),
        ////    new PropertyMetadata(FontStyles.Normal));

        /////// <summary>
        /////// Identifies the NoteAssociationFontWeight dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty NoteAssociationFontWeightProperty = DependencyProperty.Register(nameof(NoteAssociationFontWeight), typeof(FontWeight), typeof(NoteDisplay),
        ////    new PropertyMetadata(FontWeights.Normal));

        /////// <summary>
        /////// Identifies the NoteAssociationMargin dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty NoteAssociationMarginProperty = DependencyProperty.Register(nameof(NoteAssociationMargin), typeof(Thickness), typeof(NoteDisplay),
        ////    new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /////// <summary>
        /////// Identifies the NoteAssociationPadding dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty NoteAssociationPaddingProperty = DependencyProperty.Register(nameof(NoteAssociationPadding), typeof(Thickness), typeof(NoteDisplay),
        ////    new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /////// <summary>
        /////// Identifies the NoteBorderBrush dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty NoteBorderBrushProperty = DependencyProperty.Register(nameof(NoteBorderBrush), typeof(SolidColorBrush), typeof(NoteDisplay),
        ////    new PropertyMetadata(new SolidColorBrush(Colors.LightGray)));

        /////// <summary>
        /////// Identifies the NoteBorderCornerRadius dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty NoteBorderCornerRadiusProperty = DependencyProperty.Register(nameof(NoteBorderCornerRadius), typeof(CornerRadius), typeof(NoteDisplay),
        ////    new PropertyMetadata(new CornerRadius(6)));

        /////// <summary>
        /////// Identifies the NoteBorderPadding dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty NoteBorderPaddingProperty = DependencyProperty.Register(nameof(NoteBorderPadding), typeof(Thickness), typeof(NoteDisplay),
        ////    new PropertyMetadata(new Thickness(10)));

        /////// <summary>
        /////// Identifies the NoteBorderThickness dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty NoteBorderThicknessProperty = DependencyProperty.Register(nameof(NoteBorderThickness), typeof(Thickness), typeof(NoteDisplay),
        ////    new PropertyMetadata(new Thickness(0.5)));

        /////// <summary>
        /////// Identifies the NoteHoverBrush dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty NoteHoverBrushProperty = DependencyProperty.Register(nameof(NoteHoverBrush), typeof(SolidColorBrush), typeof(NoteDisplay),
        ////    new PropertyMetadata(new SolidColorBrush(Colors.AliceBlue)));

        /////// <summary>
        /////// Identifies the NoteTextFontFamily dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty NoteTextFontFamilyProperty = DependencyProperty.Register(nameof(NoteTextFontFamily), typeof(FontFamily), typeof(NoteDisplay),
        ////    new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /////// <summary>
        /////// Identifies the NoteTextFontSize dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty NoteTextFontSizeProperty = DependencyProperty.Register(nameof(NoteTextFontSize), typeof(double), typeof(NoteDisplay),
        ////    new PropertyMetadata(15d));

        /////// <summary>
        /////// Identifies the NoteTextFontStyle dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty NoteTextFontStyleProperty = DependencyProperty.Register(nameof(NoteTextFontStyle), typeof(FontStyle), typeof(NoteDisplay),
        ////    new PropertyMetadata(FontStyles.Normal));

        /////// <summary>
        /////// Identifies the NoteTextFontWeight dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty NoteTextFontWeightProperty = DependencyProperty.Register(nameof(NoteTextFontWeight), typeof(FontWeight), typeof(NoteDisplay),
        ////    new PropertyMetadata(FontWeights.Normal));

        /////// <summary>
        /////// Identifies the NoteTextMargin dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty NoteTextMarginProperty = DependencyProperty.Register(nameof(NoteTextMargin), typeof(Thickness), typeof(NoteDisplay),
        ////    new PropertyMetadata(new Thickness(2, 2, 2, 0)));

        /////// <summary>
        /////// Identifies the NoteTextMargin dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty NoteTextPaddingProperty = DependencyProperty.Register(nameof(NoteTextPadding), typeof(Thickness), typeof(NoteDisplay),
        ////    new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /////// <summary>
        /////// Identifies the TimestampFontFamily dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty TimestampFontFamilyProperty = DependencyProperty.Register(nameof(TimestampFontFamily), typeof(FontFamily), typeof(NoteDisplay),
        ////    new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /////// <summary>
        /////// Identifies the TimestampFontSize dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty TimestampFontSizeProperty = DependencyProperty.Register(nameof(TimestampFontSize), typeof(double), typeof(NoteDisplay),
        ////    new PropertyMetadata(11d));

        /////// <summary>
        /////// Identifies the TimestampFontStyle dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty TimestampFontStyleProperty = DependencyProperty.Register(nameof(TimestampFontStyle), typeof(FontStyle), typeof(NoteDisplay),
        ////    new PropertyMetadata(FontStyles.Italic));

        /////// <summary>
        /////// Identifies the TimestampFontWeight dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty TimestampFontWeightProperty = DependencyProperty.Register(nameof(TimestampFontWeight), typeof(FontWeight), typeof(NoteDisplay),
        ////    new PropertyMetadata(FontWeights.Normal));

        /////// <summary>
        /////// Identifies the TimestampMargin dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty TimestampMarginProperty = DependencyProperty.Register(nameof(TimestampMargin), typeof(Thickness), typeof(NoteDisplay),
        ////    new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /////// <summary>
        /////// Identifies the UserFontFamily dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty UserFontFamilyProperty = DependencyProperty.Register(nameof(UserFontFamily), typeof(FontFamily), typeof(NoteDisplay),
        ////        new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /////// <summary>
        /////// Identifies the UserFontSize dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty UserFontSizeProperty = DependencyProperty.Register(nameof(UserFontSize), typeof(double), typeof(NoteDisplay),
        ////    new PropertyMetadata(11d));

        /////// <summary>
        /////// Identifies the UserFontStyle dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty UserFontStyleProperty = DependencyProperty.Register(nameof(UserFontStyle), typeof(FontStyle), typeof(NoteDisplay),
        ////    new PropertyMetadata(FontStyles.Normal));

        /////// <summary>
        /////// Identifies the UserFontWeight dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty UserFontWeightProperty = DependencyProperty.Register(nameof(UserFontWeight), typeof(FontWeight), typeof(NoteDisplay),
        ////    new PropertyMetadata(FontWeights.SemiBold));

        /////// <summary>
        /////// Identifies the UserMargin dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty UserMarginProperty = DependencyProperty.Register(nameof(UserMargin), typeof(Thickness), typeof(NoteDisplay),
        ////    new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /////// <summary>
        /////// Identifies the Watermark dependency property.
        /////// </summary>
        ////public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register(nameof(Watermark), typeof(string), typeof(NoteDisplay));

        ////#endregion
        ////#region Private Event Handlers

        ////private void UpdateControlLayout()
        ////{
        ////    if (AddMode)
        ////    {
        ////        IsEditing = true;
        ////    }
        ////}


        ////private void RaiseLabelEvent(RoutedEvent routedEvent, LabelEventArgs args)
        ////{
        ////    RaiseEvent(new LabelEventArgs
        ////    {
        ////        RoutedEvent = routedEvent,
        ////        Label = args.Label,
        ////        LabelGroup = args.LabelGroup,
        ////        Note = Note
        ////    });
        ////}

        ////private void RaiseLabelGroupAddedEvent(RoutedEvent routedEvent, LabelGroupAddedEventArgs args)
        ////{
        ////    RaiseEvent(new LabelGroupAddedEventArgs
        ////    {
        ////        RoutedEvent = routedEvent,
        ////        LabelGroup = args.LabelGroup,
        ////        SourceLabelGroup = args.SourceLabelGroup
        ////    });
        ////}

        ////private void RaiseLabelGroupEvent(RoutedEvent routedEvent, LabelGroupEventArgs args)
        ////{
        ////    RaiseEvent(new LabelGroupEventArgs
        ////    {
        ////        RoutedEvent = routedEvent,
        ////        LabelGroup = args.LabelGroup
        ////    });
        ////}

        ////private void RaiseLabelGroupLabelEvent(RoutedEvent routedEvent, LabelGroupLabelEventArgs args)
        ////{
        ////    RaiseEvent(new LabelGroupLabelEventArgs
        ////    {
        ////        RoutedEvent = routedEvent,
        ////        LabelGroup = args.LabelGroup,
        ////        Label = args.Label,
        ////    });
        ////}

        ////private void OnLabelAdded(object sender, RoutedEventArgs e)
        ////{
        ////    if (e is LabelEventArgs labelEventArgs)
        ////    {
        ////        if (!string.IsNullOrEmpty(labelEventArgs.Label.TemplateText))
        ////        {
        ////            Note.Text += labelEventArgs.Label.TemplateText;
        ////            OnPropertyChanged(nameof(Note));
        ////        }

        ////        // If the label text matches an existing label, act as if that was selected rather than adding a new one.
        ////        var existingLabel = LabelGroups.GetLabel(labelEventArgs.Label.Text);
        ////        if (existingLabel != null)
        ////        {
        ////            labelEventArgs.Label = existingLabel;
        ////            RaiseLabelEvent(LabelSelectedEvent, labelEventArgs);
        ////        }
        ////        else
        ////        {
        ////            RaiseLabelEvent(LabelAddedEvent, labelEventArgs!);
        ////        }
        ////    }
        ////}

        ////private void OnLabelDeleted(object sender, RoutedEventArgs e)
        ////{
        ////    var labelEventArgs = e as LabelEventArgs;
        ////    RaiseLabelEvent(LabelDeletedEvent, labelEventArgs!);
        ////}

        ////private void OnLabelDisassociated(object sender, RoutedEventArgs e)
        ////{
        ////    var labelEventArgs = e as LabelEventArgs;
        ////    RaiseLabelEvent(LabelDisassociatedEvent, labelEventArgs!);
        ////}

        ////private void OnLabelGroupAdded(object sender, RoutedEventArgs e)
        ////{
        ////    var labelGroupAddedEventArgs = e as LabelGroupAddedEventArgs;

        ////    RaiseLabelGroupAddedEvent(LabelGroupAddedEvent, labelGroupAddedEventArgs!);
        ////}

        ////private void OnLabelGroupLabelAdded(object sender, RoutedEventArgs e)
        ////{
        ////    var labelGroupLabelEventArgs = e as LabelGroupLabelEventArgs;

        ////    RaiseLabelGroupLabelEvent(LabelGroupLabelAddedEvent, labelGroupLabelEventArgs!);
        ////}

        ////private void OnLabelGroupRemoved(object sender, RoutedEventArgs e)
        ////{
        ////    var labelGroupEventArgs = e as LabelGroupEventArgs;

        ////    RaiseLabelGroupEvent(LabelGroupRemovedEvent, labelGroupEventArgs!);
        ////}

        ////private void OnLabelGroupSelected(object sender, RoutedEventArgs e)
        ////{
        ////    var labelGroupEventArgs = e as LabelGroupEventArgs;

        ////    RaiseLabelGroupEvent(LabelGroupSelectedEvent, labelGroupEventArgs!);
        ////}

        ////private void OnLabelSelected(object sender, RoutedEventArgs e)
        ////{
        ////    var labelEventArgs = e as LabelEventArgs;

        ////    if (labelEventArgs != null && !string.IsNullOrEmpty(labelEventArgs.Label.TemplateText))
        ////    {
        ////        Note.Text += labelEventArgs.Label.TemplateText;
        ////        RaiseNoteEvent(NoteUpdatedEvent);
        ////    }

        ////    RaiseLabelEvent(LabelSelectedEvent, labelEventArgs!);
        ////}

        ////private void OnLabelRemoved(object sender, RoutedEventArgs e)
        ////{
        ////    var labelEventArgs = e as LabelEventArgs;

        ////    RaiseLabelEvent(LabelRemovedEvent, labelEventArgs!);
        ////}

        ////private void OnLabelUpdated(object sender, RoutedEventArgs e)
        ////{
        ////    var labelEventArgs = e as LabelEventArgs;

        ////    RaiseLabelEvent(LabelUpdatedEvent, labelEventArgs!);
        ////}

        //private void RaiseNoteEvent(RoutedEvent routedEvent)
        //{
        //    RaiseEvent(new NoteEventArgs
        //    {
        //        RoutedEvent = routedEvent,
        //        EntityIds = EntityIds,
        //        Note = Note
        //    });
        //}

        ////private void RaiseNoteSeenEvent(NoteSeenEventArgs args)
        ////{
        ////    RaiseEvent(new NoteSeenEventArgs
        ////    {
        ////        RoutedEvent = NoteSeenEvent,
        ////        Seen = args.Seen,
        ////        NoteViewModel = args.NoteViewModel
        ////    });
        ////}

        ////private void RaiseReplyAddedEvent(NoteReplyAddEventArgs args)
        ////{
        ////    RaiseEvent(new NoteReplyAddEventArgs
        ////    {
        ////        RoutedEvent = NoteReplyAddedEvent,
        ////        Text = args.Text,
        ////        NoteViewModelWithReplies = args.NoteViewModelWithReplies
        ////    });
        ////}

        ////private void OnNoteReplyAdded(object sender, RoutedEventArgs e)
        ////{
        ////    RaiseReplyAddedEvent(e as NoteReplyAddEventArgs);
        ////}



        //private void ConfirmParatextSend(object sender, RoutedEventArgs e)
        //{
        //    //ConfirmParatextSendPopup.IsOpen = true;
        //}

        //private void ParatextSendConfirmed(object sender, RoutedEventArgs e)
        //{
        //    //RaiseNoteEvent(NoteSendToParatextEvent);
        //    //ConfirmParatextSendPopup.IsOpen = false;
        //}

        //private void ParatextSendCancelled(object sender, RoutedEventArgs e)
        //{
        //    //ConfirmParatextSendPopup.IsOpen = false;
        //}

        ////private void RaiseNoteAssociationEvent(RoutedEvent routedEvent, RoutedEventArgs e)
        ////{
        ////    if (e is NoteAssociationEventArgs noteAssociationArgs)
        ////    {
        ////        RaiseEvent(new NoteAssociationEventArgs()
        ////        {
        ////            RoutedEvent = routedEvent,
        ////            Note = noteAssociationArgs.Note,
        ////            AssociatedEntityId = noteAssociationArgs.AssociatedEntityId
        ////        });
        ////    }
        ////}

        //////private void OnAssociationsButtonClick(object sender, RoutedEventArgs e)
        //////{
        //////    IsAssociationButtonClicked = true;
        //////    OnPropertyChanged(nameof(AssociationsVisibility));
        //////    OnPropertyChanged(nameof(AssociationsButtonVisibility));
        //////}

        ////private void OnNoteAssociationClicked(object sender, RoutedEventArgs e)
        ////{
        ////    RaiseNoteAssociationEvent(NoteAssociationClickedEvent, e);
        ////}

        ////private void OnNoteAssociationDoubleClicked(object sender, RoutedEventArgs e)
        ////{
        ////    RaiseNoteAssociationEvent(NoteAssociationDoubleClickedEvent, e);
        ////}

        ////private void OnNoteAssociationLeftButtonDown(object sender, RoutedEventArgs e)
        ////{
        ////    RaiseNoteAssociationEvent(NoteAssociationLeftButtonDownEvent, e);
        ////}

        ////private void OnNoteAssociationLeftButtonUp(object sender, RoutedEventArgs e)
        ////{
        ////    RaiseNoteAssociationEvent(NoteAssociationRightButtonUpEvent, e);
        ////}
        ////private void OnNoteAssociationRightButtonDown(object sender, RoutedEventArgs e)
        ////{
        ////    RaiseNoteAssociationEvent(NoteAssociationRightButtonDownEvent, e);
        ////}

        ////private void OnNoteAssociationRightButtonUp(object sender, RoutedEventArgs e)
        ////{
        ////    RaiseNoteAssociationEvent(NoteAssociationRightButtonUpEvent, e);
        ////}

        ////private void OnNoteAssociationMouseEnter(object sender, RoutedEventArgs e)
        ////{
        ////    RaiseNoteAssociationEvent(NoteAssociationMouseEnterEvent, e);
        ////}

        ////private void OnNoteAssociationMouseLeave(object sender, RoutedEventArgs e)
        ////{
        ////    RaiseNoteAssociationEvent(NoteAssociationMouseLeaveEvent, e);
        ////}

        ////private void OnMouseEnter(object sender, MouseEventArgs e)
        ////{
        ////    RaiseNoteEvent(NoteEditorMouseEnterEvent);
        ////}

        ////private void OnMouseLeave(object sender, MouseEventArgs e)
        ////{
        ////    RaiseNoteEvent(NoteEditorMouseLeaveEvent);
        ////}


        ////#endregion Private event handlers
        ////#region Public Properties

        /////// <summary>
        /////// Gets or sets whether the control is adding a new note or editing an existing one.
        /////// </summary>
        ////public bool AddMode
        ////{
        ////    get => (bool)GetValue(AddModeProperty);
        ////    set => SetValue(AddModeProperty, value);
        ////}

        ////// TODO: localize
        ////public string ApplyLabel => AddMode ? "Add Note" : "Update Note";

        ////private string? OriginalNoteText { get; set; } = string.Empty;

        ////private bool _isEditing;
        ////private bool _isChanged;

        ////private bool IsEditing
        ////{
        ////    get => _isEditing;
        ////    set
        ////    {
        ////        _isEditing = value;
        ////        OnPropertyChanged(nameof(NoteLabelVisibility));
        ////        OnPropertyChanged(nameof(NoteTextBoxVisibility));
        ////    }
        ////}

        ////private bool IsChanged
        ////{
        ////    get => _isChanged;
        ////    set
        ////    {
        ////        _isChanged = value;
        ////        OnPropertyChanged(nameof(TimestampRowVisibility));
        ////        OnPropertyChanged(nameof(NoteSaveCancelButtonVisibility));
        ////    }
        ////}

        //public LabelCollection Labels => Note?.Labels ?? new LabelCollection();

        ////public Visibility NoteLabelVisibility => IsEditing ? Visibility.Hidden : Visibility.Visible;
        ////public Visibility NoteTextBoxVisibility => IsEditing ? Visibility.Visible : Visibility.Hidden;
        ////public Visibility NoteSaveCancelButtonVisibility => IsChanged ? Visibility.Visible : Visibility.Collapsed;
        ////public Visibility TimestampRowVisibility => AddMode || IsChanged ? Visibility.Collapsed : Visibility.Visible;
        ////public Visibility NotePropertiesVisibility => AddMode ? Visibility.Collapsed : Visibility.Visible;
        //////public Visibility AssociationsVisibility => AddMode || !IsAssociationButtonClicked ? Visibility.Collapsed : Visibility.Visible;
        //////public Visibility AssociationsButtonVisibility => IsAssociationButtonClicked ? Visibility.Hidden : Visibility.Visible;
        ////public Visibility ParatextSendVisibility => !AddMode && Note != null && Note.EnableParatextSend ? Visibility.Visible : Visibility.Collapsed;
        //////private bool IsAssociationButtonClicked { get; set; }

        /////// <summary>
        /////// Gets or sets the <see cref="UserId"/> for the current user.
        /////// </summary>
        ////public UserId CurrentUserId
        ////{
        ////    get => (UserId)GetValue(CurrentUserIdProperty);
        ////    set => SetValue(CurrentUserIdProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the default <see cref="LabelGroupViewModel"/> for the user.
        /////// </summary>
        ////public LabelGroupViewModel DefaultLabelGroup
        ////{
        ////    get => (LabelGroupViewModel)GetValue(DefaultLabelGroupProperty);
        ////    set => SetValue(DefaultLabelGroupProperty, value);
        ////}

        ///// <summary>
        ///// Gets or sets the collection of <see cref="EntityId{T}"/> associated with the note.
        ///// </summary>
        //public EntityIdCollection EntityIds
        //{
        //    get => (EntityIdCollection)GetValue(EntityIdsProperty);
        //    set => SetValue(EntityIdsProperty, value);
        //}

        /////// <summary>
        /////// Gets or sets the background brush for individual label boxes.
        /////// </summary>
        ////public SolidColorBrush LabelBackground
        ////{
        ////    get => (SolidColorBrush)GetValue(LabelBackgroundProperty);
        ////    set => SetValue(LabelBackgroundProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the corner radius for individual label boxes.
        /////// </summary>
        ////public CornerRadius LabelCornerRadius
        ////{
        ////    get => (CornerRadius)GetValue(LabelCornerRadiusProperty);
        ////    set => SetValue(LabelCornerRadiusProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the font size for individual label boxes.
        /////// </summary>
        ////public double LabelFontSize
        ////{
        ////    get => (double)GetValue(LabelFontSizeProperty);
        ////    set => SetValue(LabelFontSizeProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets a collection of <see cref="LabelGroupViewModel"/> objects that the user can select from.
        /////// </summary>
        ////public LabelGroupViewModelCollection LabelGroups
        ////{
        ////    get => (LabelGroupViewModelCollection)GetValue(LabelGroupsProperty);
        ////    set => SetValue(LabelGroupsProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the margin for individual label boxes.
        /////// </summary>
        ////public Thickness LabelMargin
        ////{
        ////    get => (Thickness)GetValue(LabelMarginProperty);
        ////    set => SetValue(LabelMarginProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the padding for individual label boxes.
        /////// </summary>
        ////public Thickness LabelPadding
        ////{
        ////    get => (Thickness)GetValue(LabelPaddingProperty);
        ////    set => SetValue(LabelPaddingProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets a collection of <see cref="NotesLabel"/> objects for auto selection in the control.
        /////// </summary>
        ////public LabelCollection LabelSuggestions
        ////{
        ////    get => (LabelCollection)GetValue(LabelSuggestionsProperty);
        ////    set => SetValue(LabelSuggestionsProperty, value);
        ////}

        ///// <summary>
        ///// Gets or sets the <see cref="NoteViewModel"/> being displayed.
        ///// </summary>
        //public NoteViewModel Note
        //{
        //    get => (NoteViewModel)GetValue(NoteProperty);
        //    set => SetValue(NoteProperty, value);
        //}

        /////// <summary>
        /////// Gets or sets the font size for the note associations.
        /////// </summary>
        ////public FontFamily NoteAssociationFontFamily
        ////{
        ////    get => (FontFamily)GetValue(NoteAssociationFontFamilyProperty);
        ////    set => SetValue(NoteAssociationFontFamilyProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the font size for the note associations.
        /////// </summary>
        ////public double NoteAssociationFontSize
        ////{
        ////    get => (double)GetValue(NoteAssociationFontSizeProperty);
        ////    set => SetValue(NoteAssociationFontSizeProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the font weight for the note associations.
        /////// </summary>
        ////public FontWeight NoteAssociationFontWeight
        ////{
        ////    get => (FontWeight)GetValue(NoteAssociationFontWeightProperty);
        ////    set => SetValue(NoteAssociationFontWeightProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the font style for the note associations.
        /////// </summary>
        ////public FontStyle NoteAssociationFontStyle
        ////{
        ////    get => (FontStyle)GetValue(NoteAssociationFontStyleProperty);
        ////    set => SetValue(NoteAssociationFontStyleProperty, value);
        ////}
        /////// <summary>
        /////// Gets or sets the margin for individual note associations.
        /////// </summary>
        ////public Thickness NoteAssociationMargin
        ////{
        ////    get => (Thickness)GetValue(NoteAssociationMarginProperty);
        ////    set => SetValue(NoteAssociationMarginProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the padding for individual note associations.
        /////// </summary>
        ////public Thickness NoteAssociationPadding
        ////{
        ////    get => (Thickness)GetValue(NoteAssociationPaddingProperty);
        ////    set => SetValue(NoteAssociationPaddingProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the <see cref="SolidColorBrush"/> for the border around the note editor.
        /////// </summary>
        ////public SolidColorBrush NoteBorderBrush
        ////{
        ////    get => (SolidColorBrush)GetValue(NoteBorderBrushProperty);
        ////    set => SetValue(NoteBorderBrushProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the corner radius for the border around the note editor.
        /////// </summary>
        ////public CornerRadius NoteBorderCornerRadius
        ////{
        ////    get => (CornerRadius)GetValue(NoteBorderCornerRadiusProperty);
        ////    set => SetValue(NoteBorderCornerRadiusProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the padding of the border around the note editor.
        /////// </summary>
        ////public Thickness NoteBorderPadding
        ////{
        ////    get => (Thickness)GetValue(NoteBorderPaddingProperty);
        ////    set => SetValue(NoteBorderPaddingProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the thickness of the border around the note editor.
        /////// </summary>
        ////public Thickness NoteBorderThickness
        ////{
        ////    get => (Thickness)GetValue(NoteBorderThicknessProperty);
        ////    set => SetValue(NoteBorderThicknessProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the background <see cref="SolidColorBrush"/> to use when the note is hovered.
        /////// </summary>
        ////public SolidColorBrush NoteHoverBrush
        ////{
        ////    get => (SolidColorBrush)GetValue(NoteHoverBrushProperty);
        ////    set => SetValue(NoteHoverBrushProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the font size for the note text box.
        /////// </summary>
        ////public FontFamily NoteTextFontFamily
        ////{
        ////    get => (FontFamily)GetValue(NoteTextFontFamilyProperty);
        ////    set => SetValue(NoteTextFontFamilyProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the font size for the note text box.
        /////// </summary>
        ////public double NoteTextFontSize
        ////{
        ////    get => (double)GetValue(NoteTextFontSizeProperty);
        ////    set => SetValue(NoteTextFontSizeProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the font weight for the note text box.
        /////// </summary>
        ////public FontWeight NoteTextFontWeight
        ////{
        ////    get => (FontWeight)GetValue(NoteTextFontWeightProperty);
        ////    set => SetValue(NoteTextFontWeightProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the font style for the note text box.
        /////// </summary>
        ////public FontStyle NoteTextFontStyle
        ////{
        ////    get => (FontStyle)GetValue(NoteTextFontStyleProperty);
        ////    set => SetValue(NoteTextFontStyleProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the margin for the note text box.
        /////// </summary>
        ////public Thickness NoteTextMargin
        ////{
        ////    get => (Thickness)GetValue(NoteTextMarginProperty);
        ////    set => SetValue(NoteTextMarginProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the padding for the note text box.
        /////// </summary>
        ////public Thickness NoteTextPadding
        ////{
        ////    get => (Thickness)GetValue(NoteTextPaddingProperty);
        ////    set => SetValue(NoteTextPaddingProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the margin for the timestamp and user.
        /////// </summary>
        ////public Thickness TimestampMargin
        ////{
        ////    get => (Thickness)GetValue(TimestampMarginProperty);
        ////    set => SetValue(TimestampMarginProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the font family for displaying the timestamp.
        /////// </summary>
        ////public FontFamily TimestampFontFamily
        ////{
        ////    get => (FontFamily)GetValue(TimestampFontFamilyProperty);
        ////    set => SetValue(TimestampFontFamilyProperty, value);
        ////}
        /////// <summary>
        /////// Gets or sets the font size for displaying the timestamp.
        /////// </summary>
        ////public double TimestampFontSize
        ////{
        ////    get => (double)GetValue(TimestampFontSizeProperty);
        ////    set => SetValue(TimestampFontSizeProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the font style for displaying the timestamp.
        /////// </summary>
        ////public FontStyle TimestampFontStyle
        ////{
        ////    get => (FontStyle)GetValue(TimestampFontStyleProperty);
        ////    set => SetValue(TimestampFontStyleProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the font weight for displaying the timestamp.
        /////// </summary>
        ////public FontWeight TimestampFontWeight
        ////{
        ////    get => (FontWeight)GetValue(TimestampFontWeightProperty);
        ////    set => SetValue(TimestampFontStyleProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the font family for displaying the user name below the note.
        /////// </summary>
        ////public FontFamily UserFontFamily
        ////{
        ////    get => (FontFamily)GetValue(UserFontFamilyProperty);
        ////    set => SetValue(UserFontFamilyProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the font size for displaying the user name below the note.
        /////// </summary>
        ////public double UserFontSize
        ////{
        ////    get => (double)GetValue(UserFontSizeProperty);
        ////    set => SetValue(UserFontSizeProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the font style for displaying the user name below the note.
        /////// </summary>
        ////public FontStyle UserFontStyle
        ////{
        ////    get => (FontStyle)GetValue(UserFontStyleProperty);
        ////    set => SetValue(UserFontStyleProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the font weight for displaying the user name below the note.
        /////// </summary>
        ////public FontWeight UserFontWeight
        ////{
        ////    get => (FontWeight)GetValue(UserFontWeightProperty);
        ////    set => SetValue(UserFontStyleProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the margin for displaying the user below the note.
        /////// </summary>
        ////public Thickness UserMargin
        ////{
        ////    get => (Thickness)GetValue(UserMarginProperty);
        ////    set => SetValue(UserMarginProperty, value);
        ////}

        /////// <summary>
        /////// Gets or sets the watermark to display for new notes.
        /////// </summary>
        ////public string Watermark
        ////{
        ////    get => (string)GetValue(WatermarkProperty);
        ////    set => SetValue(WatermarkProperty, value);
        ////}

        ////#endregion
        ////#region Public Events

        /////// <summary>
        /////// Occurs when an existing label suggestion is selected.
        /////// </summary>
        ////public event RoutedEventHandler LabelSelected
        ////{
        ////    add => AddHandler(LabelSelectedEvent, value);
        ////    remove => RemoveHandler(LabelSelectedEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when an new label is added.
        /////// </summary>
        ////public event RoutedEventHandler LabelAdded
        ////{
        ////    add => AddHandler(LabelAddedEvent, value);
        ////    remove => RemoveHandler(LabelAddedEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when a label is deleted.
        /////// </summary>
        ////public event RoutedEventHandler LabelDeleted
        ////{
        ////    add => AddHandler(LabelDeletedEvent, value);
        ////    remove => RemoveHandler(LabelDeletedEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when a label is disassociated.
        /////// </summary>
        ////public event RoutedEventHandler LabelDisassociated
        ////{
        ////    add => AddHandler(LabelDisassociatedEvent, value);
        ////    remove => RemoveHandler(LabelDisassociatedEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when an new label group is added.
        /////// </summary>
        ////public event RoutedEventHandler LabelGroupAdded
        ////{
        ////    add => AddHandler(LabelGroupAddedEvent, value);
        ////    remove => RemoveHandler(LabelGroupAddedEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when a label is added to a label group.
        /////// </summary>
        ////public event RoutedEventHandler LabelGroupLabelAdded
        ////{
        ////    add => AddHandler(LabelGroupLabelAddedEvent, value);
        ////    remove => RemoveHandler(LabelGroupLabelAddedEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when a label is removed from a label group.
        /////// </summary>
        ////public event RoutedEventHandler LabelGroupLabelRemoved
        ////{
        ////    add => AddHandler(LabelGroupLabelRemovedEvent, value);
        ////    remove => RemoveHandler(LabelGroupLabelRemovedEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when an existing label group is removed.
        /////// </summary>
        ////public event RoutedEventHandler LabelGroupRemoved
        ////{
        ////    add => AddHandler(LabelGroupRemovedEvent, value);
        ////    remove => RemoveHandler(LabelGroupRemovedEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when an existing label group is selected.
        /////// </summary>
        ////public event RoutedEventHandler LabelGroupSelected
        ////{
        ////    add => AddHandler(LabelGroupSelectedEvent, value);
        ////    remove => RemoveHandler(LabelGroupSelectedEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when a label is removed.
        /////// </summary>
        ////public event RoutedEventHandler LabelRemoved
        ////{
        ////    add => AddHandler(LabelRemovedEvent, value);
        ////    remove => RemoveHandler(LabelRemovedEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when a label is updated.
        /////// </summary>
        ////public event RoutedEventHandler LabelUpdated
        ////{
        ////    add => AddHandler(LabelUpdatedEvent, value);
        ////    remove => RemoveHandler(LabelUpdatedEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when a note is applied.
        /////// </summary>
        ////public event RoutedEventHandler NoteAdded
        ////{
        ////    add => AddHandler(NoteAddedEvent, value);
        ////    remove => RemoveHandler(NoteAddedEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when an individual note association is clicked.
        /////// </summary>
        ////public event RoutedEventHandler NoteAssociationClicked
        ////{
        ////    add => AddHandler(NoteAssociationClickedEvent, value);
        ////    remove => RemoveHandler(NoteAssociationClickedEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when an individual note association is clicked two or more times.
        /////// </summary>
        ////public event RoutedEventHandler NoteAssociationDoubleClicked
        ////{
        ////    add => AddHandler(NoteAssociationDoubleClickedEvent, value);
        ////    remove => RemoveHandler(NoteAssociationDoubleClickedEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when the left mouse button is pressed while the mouse pointer is over a note association.
        /////// </summary>
        ////public event RoutedEventHandler NoteAssociationLeftButtonDown
        ////{
        ////    add => AddHandler(NoteAssociationLeftButtonDownEvent, value);
        ////    remove => RemoveHandler(NoteAssociationLeftButtonDownEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when the left mouse button is released while the mouse pointer is over a note association.
        /////// </summary>
        ////public event RoutedEventHandler NoteAssociationLeftButtonUp
        ////{
        ////    add => AddHandler(NoteAssociationLeftButtonUpEvent, value);
        ////    remove => RemoveHandler(NoteAssociationLeftButtonUpEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when the right mouse button is pressed while the mouse pointer is over a note association.
        /////// </summary>
        ////public event RoutedEventHandler NoteAssociationRightButtonDown
        ////{
        ////    add => AddHandler(NoteAssociationRightButtonDownEvent, value);
        ////    remove => RemoveHandler(NoteAssociationRightButtonDownEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when the right mouse button is released while the mouse pointer is over a note association.
        /////// </summary>
        ////public event RoutedEventHandler NoteAssociationRightButtonUp
        ////{
        ////    add => AddHandler(NoteAssociationRightButtonUpEvent, value);
        ////    remove => RemoveHandler(NoteAssociationRightButtonUpEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when the mouse pointer enters the bounds of a note association.
        /////// </summary>
        ////public event RoutedEventHandler NoteAssociationMouseEnter
        ////{
        ////    add => AddHandler(NoteAssociationMouseEnterEvent, value);
        ////    remove => RemoveHandler(NoteAssociationMouseEnterEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when the mouse pointer leaves the bounds of a note association.
        /////// </summary>
        ////public event RoutedEventHandler NoteAssociationMouseLeave
        ////{
        ////    add => AddHandler(NoteAssociationMouseLeaveEvent, value);
        ////    remove => RemoveHandler(NoteAssociationMouseLeaveEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when a note is deleted.
        /////// </summary>
        ////public event RoutedEventHandler NoteDeleted
        ////{
        ////    add => AddHandler(NoteDeletedEvent, value);
        ////    remove => RemoveHandler(NoteDeletedEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when the mouse enters the note editor.
        /////// </summary>
        ////public event RoutedEventHandler NoteEditorMouseEnter
        ////{
        ////    add => AddHandler(NoteEditorMouseEnterEvent, value);
        ////    remove => RemoveHandler(NoteEditorMouseEnterEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when the mouse leaves the note editor.
        /////// </summary>
        ////public event RoutedEventHandler NoteEditorMouseLeave
        ////{
        ////    add => AddHandler(NoteEditorMouseLeaveEvent, value);
        ////    remove => RemoveHandler(NoteEditorMouseLeaveEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when the "seen by me" checkbox is checked or unchecked.
        /////// </summary>
        ////public event RoutedEventHandler NoteSeen
        ////{
        ////    add => AddHandler(NoteSeenEvent, value);
        ////    remove => RemoveHandler(NoteSeenEvent, value);
        ////}

        /////// <summary>
        /////// Occurs when a new reply is added to a note.
        /////// </summary>
        ////public event RoutedEventHandler NoteReplyAdded
        ////{
        ////    add => AddHandler(NoteReplyAddedEvent, value);
        ////    remove => RemoveHandler(NoteReplyAddedEvent, value);
        ////}


        ///// <summary>
        ///// Occurs when the user requests a note be sent to Paratext.
        ///// </summary>
        //public event RoutedEventHandler NoteSendToParatext
        //{
        //    add => AddHandler(NoteSendToParatextEvent, value);
        //    remove => RemoveHandler(NoteSendToParatextEvent, value);
        //}

        /////// <summary>
        /////// Occurs when a note is updated.
        /////// </summary>
        ////public event RoutedEventHandler NoteUpdated
        ////{
        ////    add => AddHandler(NoteUpdatedEvent, value);
        ////    remove => RemoveHandler(NoteUpdatedEvent, value);
        ////}

        //private static void OnNotePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    var control = (NotesView)d;
        //    control.OnPropertyChanged(nameof(Labels));
        //    //control.OnPropertyChanged(nameof(ParatextSendVisibility));
        //}

        ////private static void OnAddModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        ////{
        ////    var control = (NotesView)obj;
        ////    control.UpdateControlLayout();
        ////    control.OnPropertyChanged(nameof(ApplyLabel));
        ////    control.OnPropertyChanged(nameof(NotePropertiesVisibility));
        ////}
        ////#endregion Public events

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkbox && checkbox.DataContext is NoteViewModel note)
            {
                _vm.CheckedNoteViewModels.Add(note);
            }
        }

        private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkbox && checkbox.DataContext is NoteViewModel note)
            {
                _vm.CheckedNoteViewModels.Remove(note);
            }
            
        }
    }
}
