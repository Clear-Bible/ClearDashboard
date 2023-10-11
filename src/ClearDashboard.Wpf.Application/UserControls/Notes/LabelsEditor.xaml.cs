using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Annotations;
using ClearDashboard.Wpf.Application.Collections.Notes;
using ClearDashboard.Wpf.Application.Events.Notes;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Notes;
using NotesLabel = ClearDashboard.DAL.Alignment.Notes.Label;

namespace ClearDashboard.Wpf.Application.UserControls.Notes
{
    /// <summary>
    /// A control that displays and edits the collection of labels associated with a note, optionally grouped by label groups.
    /// </summary>
    public partial class LabelsEditor : INotifyPropertyChanged, IHandle<LabelGroupAddedMessage>, IHandle<NoteLabelDetachedMessage>
    {
        #region Static Routed Events

        /// <summary>
        /// Identifies the LabelAddedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelAddedEvent = EventManager.RegisterRoutedEvent
            (nameof(LabelAdded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LabelsEditor));

        /// <summary>
        /// Identifies the LabelAssociatedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelAssociatedEvent = EventManager.RegisterRoutedEvent
            (nameof(LabelAssociated), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LabelsEditor));

        /// <summary>
        /// Identifies the LabelDeletedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelDeletedEvent = EventManager.RegisterRoutedEvent
            (nameof(LabelDeleted), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LabelsEditor));

        /// <summary>
        /// Identifies the LabelDisassociatedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelDisassociatedEvent = EventManager.RegisterRoutedEvent
            (nameof(LabelDisassociated), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LabelsEditor));

        /// <summary>
        /// Identifies the LabelGroupAddedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelGroupAddedEvent = EventManager.RegisterRoutedEvent
            (nameof(LabelGroupAdded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LabelsEditor));

        /// <summary>
        /// Identifies the LabelGroupLabelAddedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelGroupLabelAddedEvent = EventManager.RegisterRoutedEvent
            (nameof(LabelGroupLabelAdded), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LabelsEditor));

        /// <summary>
        /// Identifies the LabelGroupLabelRemovedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelGroupLabelRemovedEvent = EventManager.RegisterRoutedEvent
            (nameof(LabelGroupLabelRemoved), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LabelsEditor));

        /// <summary>
        /// Identifies the LabelGroupRemovedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelGroupRemovedEvent = EventManager.RegisterRoutedEvent
            (nameof(LabelGroupRemoved), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LabelsEditor));

        /// <summary>
        /// Identifies the LabelGroupSelectedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelGroupSelectedEvent = EventManager.RegisterRoutedEvent
            (nameof(LabelGroupSelected), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LabelsEditor));

        /// <summary>
        /// Identifies the LabelRemovedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelRemovedEvent = EventManager.RegisterRoutedEvent
            (nameof(LabelRemoved), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LabelsEditor));

        /// <summary>
        /// Identifies the LabelSelectedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelSelectedEvent = EventManager.RegisterRoutedEvent
            (nameof(LabelSelected), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LabelsEditor));

        /// <summary>
        /// Identifies the LabelUpdatedEvent routed event.
        /// </summary>
        public static readonly RoutedEvent LabelUpdatedEvent = EventManager.RegisterRoutedEvent
            (nameof(LabelUpdated), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LabelsEditor));

        #endregion Static Routed Events
        #region Static Dependency Properties

        /// <summary>
        /// Identifies the CurrentLabelGroup dependency property.
        /// </summary>
        public static readonly DependencyProperty CurrentLabelGroupProperty = DependencyProperty.Register(nameof(CurrentLabelGroup), typeof(LabelGroupViewModel), typeof(LabelsEditor));

        /// <summary>
        /// Identifies the CurrentUserId dependency property.
        /// </summary>
        public static readonly DependencyProperty CurrentUserIdProperty = DependencyProperty.Register(nameof(CurrentUserId), typeof(UserId), typeof(LabelsEditor));

        /// <summary>
        /// Identifies the DefaultLabelGroup dependency property.
        /// </summary>
        public static readonly DependencyProperty DefaultLabelGroupProperty = DependencyProperty.Register(nameof(DefaultLabelGroup), typeof(LabelGroupViewModel), typeof(LabelsEditor));

        /// <summary>
        /// Identifies the LabelGroups dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelGroupsProperty = DependencyProperty.Register(nameof(LabelGroups), typeof(LabelGroupViewModelCollection), typeof(LabelsEditor));

        /// <summary>
        /// Identifies the LabelBackground dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelBackgroundProperty = DependencyProperty.Register(nameof(LabelBackground), typeof(SolidColorBrush), typeof(LabelsEditor));

        /// <summary>
        /// Identifies the LabelCornerRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelCornerRadiusProperty = DependencyProperty.Register(nameof(LabelCornerRadius), typeof(CornerRadius), typeof(LabelsEditor),
            new PropertyMetadata(new CornerRadius(0)));

        /// <summary>
        /// Identifies the LabelFontFamily dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelFormFontFamilyProperty = DependencyProperty.Register(nameof(LabelFontFamily), typeof(FontFamily), typeof(LabelsEditor),
            new PropertyMetadata(new FontFamily(new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Font.xaml"), ".Resources/Roboto/#Roboto")));

        /// <summary>
        /// Identifies the LabelFontSize dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelFontSizeProperty = DependencyProperty.Register(nameof(LabelFontSize), typeof(double), typeof(LabelsEditor),
            new PropertyMetadata(14d));

        /// <summary>
        /// Identifies the LabelFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelFontStyleProperty = DependencyProperty.Register(nameof(LabelFontStyle), typeof(FontStyle), typeof(LabelsEditor),
            new PropertyMetadata(FontStyles.Normal));

        /// <summary>
        /// Identifies the LabelFontStyle dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelFontWeightProperty = DependencyProperty.Register(nameof(LabelFontWeight), typeof(FontWeight), typeof(LabelsEditor),
            new PropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// Identifies the LabelMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelMarginProperty = DependencyProperty.Register(nameof(LabelMargin), typeof(Thickness), typeof(LabelsEditor),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        /// <summary>
        /// Identifies the LabelPadding dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelPaddingProperty = DependencyProperty.Register(nameof(LabelPadding), typeof(Thickness), typeof(LabelsEditor),
            new PropertyMetadata(new Thickness(10, 6, 10, 50)));

        /// <summary>
        /// Identifies the LabelSuggestions dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelSuggestionsProperty = DependencyProperty.Register(nameof(LabelSuggestions), typeof(LabelCollection), typeof(LabelsEditor));

        /// <summary>
        /// Identifies the LabelWidth dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelWidthProperty = DependencyProperty.Register(nameof(LabelWidth), typeof(double), typeof(LabelsEditor),
            new PropertyMetadata(Double.NaN));

        /// <summary>
        /// Identifies the Labels dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelsProperty = DependencyProperty.Register(nameof(Labels), typeof(LabelCollection), typeof(LabelsEditor));

        /// <summary>
        /// Identifies the Note dependency property.
        /// </summary>
        public static readonly DependencyProperty NoteProperty = DependencyProperty.Register(nameof(Note), typeof(NoteViewModel), typeof(LabelsEditor));

        #endregion
        #region Private Properties

        private string _newLabelGroupName = string.Empty;
        private Visibility _newLabelGroupInitializeVisibility = Visibility.Hidden;
        private Visibility _newLabelGroupErrorVisibility = Visibility.Hidden;

        #endregion
        #region Private Methods

        private void RaiseLabelEvent(RoutedEvent routedEvent)
        {
            RaiseEvent(new LabelEventArgs
            {
                RoutedEvent = routedEvent,
                Label = CurrentLabel,
                LabelGroup = CurrentLabelGroup,
                Note = Note
            });
        }

        private void RaiseLabelGroupEvent(RoutedEvent routedEvent, LabelGroupViewModel labelGroup)
        {
            RaiseEvent(new LabelGroupEventArgs
            {
                RoutedEvent = routedEvent,
                LabelGroup = labelGroup,
            });
        }

        private void RaiseLabelGroupAddedEvent(RoutedEvent routedEvent, LabelGroupViewModel labelGroup, LabelGroupViewModel? sourceLabelGroup)
        {
            RaiseEvent(new LabelGroupAddedEventArgs
            {
                RoutedEvent = routedEvent,
                LabelGroup = labelGroup,
                SourceLabelGroup = sourceLabelGroup
            });
        }

        private void RaiseLabelGroupLabelEvent(RoutedEvent routedEvent, LabelGroupViewModel labelGroup, NotesLabel label)
        {
            RaiseEvent(new LabelGroupLabelEventArgs
            {
                RoutedEvent = routedEvent,
                LabelGroup = labelGroup,
                Label = label,
            });
        }

        #endregion
        #region Public Event Handlers
        
        public Task HandleAsync(LabelGroupAddedMessage message, CancellationToken cancellationToken)
        {
            CurrentLabelGroup = message.LabelGroup;
            return Task.CompletedTask;
        }

        #endregion
        #region Private Event Handlers

        private void CacheCurrentLabel(RoutedEventArgs args)
        {
            if (args is LabelEventArgs labelEventArgs)
            {
                CurrentLabel = labelEventArgs.Label;
                CurrentLabelName = CurrentLabel?.Text;

                if (CurrentLabelGroup?.LabelGroupId == null)
                {
                    CurrentLabelName += " ?";
                }
            }
            CurrentLabelGroupName = CurrentLabelGroup?.LabelGroupId != null ? CurrentLabelGroup.Name : string.Empty;
        }

        private void AddCurrentLabel()
        {
            Labels.AddDistinct(CurrentLabel);
            OnPropertyChanged(nameof(Labels));
        }

        private void RemoveCurrentLabel()
        {
            Labels.RemoveIfExists(CurrentLabel);
            OnPropertyChanged(nameof(Labels));
        }

        private void OnLabelAdded(object sender, RoutedEventArgs e)
        {
            CacheCurrentLabel(e);
            ConfirmAddLabelPopup.IsOpen = true;
        }

        private void OnLabelAddConfirmed(object sender, RoutedEventArgs e)
        {
            RaiseLabelEvent(LabelAddedEvent);
            AddCurrentLabel();
            ConfirmAddLabelPopup.IsOpen = false;
        }

        private void OnLabelAddCancelled(object sender, RoutedEventArgs e)
        {
            ConfirmAddLabelPopup.IsOpen = false;
        }

        private void OnLabelSelected(object sender, RoutedEventArgs e)
        {
            CacheCurrentLabel(e);
            RaiseLabelEvent(LabelSelectedEvent);
            AddCurrentLabel();
        }

        private void OnLabelDeleted(object sender, RoutedEventArgs e)
        {
            CacheCurrentLabel(e);

            if (CurrentLabelGroup?.LabelGroupId != null)
            {
                ConfirmDisassociateLabelPopup.IsOpen = true;
            }
            else
            {
                ConfirmDeleteLabelPopup.IsOpen = true;
            }
        }

        private void OnLabelDeleteConfirmed(object sender, RoutedEventArgs e)
        {
            RemoveCurrentLabel();
            RaiseLabelEvent(LabelDeletedEvent);
            ConfirmDeleteLabelPopup.IsOpen = false;
        }

        private void OnLabelDeleteCancelled(object sender, RoutedEventArgs e)
        {
            ConfirmDeleteLabelPopup.IsOpen = false;
        }

        private void OnLabelDisassociateConfirmed(object sender, RoutedEventArgs e)
        {
            RaiseLabelEvent(LabelDisassociatedEvent);
            ConfirmDisassociateLabelPopup.IsOpen = false;
        }

        private void OnLabelDisassociateCancelled(object sender, RoutedEventArgs e)
        {
            ConfirmDisassociateLabelPopup.IsOpen = false;
        }

        private void OnLabelRemoved(object sender, RoutedEventArgs e)
        {
            if (e is LabelEventArgs args)
            {
                CurrentLabel = args.Label;
                RemoveCurrentLabel();
                RaiseLabelEvent(LabelRemovedEvent);
            }
        }

        private void OnLabelUpdated(object sender, RoutedEventArgs e)
        {
            var args = e as LabelEventArgs;
            RaiseEvent(new LabelEventArgs
            {
                RoutedEvent = LabelUpdatedEvent,
                Label = args!.Label,
            });
        }

        private void OnLabelGroupAddClicked(object sender, RoutedEventArgs e)
        {
            AddLabelGroupPopup.IsOpen = true;
   
            NewLabelGroupName = string.Empty;
            NewLabelGroupTextBox.Focusable = true;
            NewLabelGroupTextBox.Focus();

            CurrentSourceLabelGroup = LabelGroups.FirstOrDefault();

            Keyboard.Focus(NewLabelGroupTextBox);
        }

        private void OnLabelGroupAddEscapePressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                OnLabelGroupAddCancelled(sender, e);
            }
        }

        private void OnLabelGroupAddNameChanged(object sender, TextChangedEventArgs e)
        {
            NewLabelGroupInitializationVisibility = NewLabelGroupTextBox.Text.Length > 0 ? Visibility.Visible : Visibility.Hidden;
            NewLabelGroupErrorVisibility = LabelGroups.All(lg => lg.Name != NewLabelGroupTextBox.Text) ? Visibility.Hidden : Visibility.Collapsed;
        }

        private void OnLabelGroupAddConfirmed(object sender, RoutedEventArgs e)
        {
            if (LabelGroups.All(lg => lg.Name != NewLabelGroupTextBox.Text))
            {
                var sourceLabelGroup = ((LabelGroupViewModel)SourceLabelGroupComboBox.SelectedItem);
                if (sourceLabelGroup is { LabelGroupId: null })
                {
                    sourceLabelGroup = null;
                }
                RaiseLabelGroupAddedEvent(LabelGroupAddedEvent, new LabelGroupViewModel { Name = NewLabelGroupTextBox.Text }, sourceLabelGroup);

                NewLabelGroupErrorVisibility = Visibility.Hidden;
                NewLabelGroupName = string.Empty;
                AddLabelGroupPopup.IsOpen = false;
            }
            else
            {
                NewLabelGroupErrorVisibility = Visibility.Visible;
                AddLabelGroupPopup.IsOpen = true;
            }
        }

        private void OnLabelGroupAddCancelled(object sender, RoutedEventArgs e)
        {
            AddLabelGroupPopup.IsOpen = false;
        }

        private void OnLabelGroupSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RaiseLabelGroupEvent(LabelGroupSelectedEvent, CurrentLabelGroup);
            LabelSuggestions = CurrentLabelGroup.Labels;

            OnPropertyChanged(nameof(LabelGroupDeleteVisibility));
        }

        private void ConfirmLabelGroupDeletion(object sender, RoutedEventArgs e)
        {
            ConfirmDeleteLabelGroupPopup.IsOpen = true;
        }

        private void OnLabelGroupRemoveConfirmed(object sender, RoutedEventArgs e)
        {
            //if (LabelGroupComboBox.SelectedItem is LabelGroupViewModel labelGroup)
            //{
            //    RaiseLabelGroupEvent(LabelGroupRemovedEvent, labelGroup);
            //    CurrentLabelGroup = LabelGroups.First();
            //}

            ConfirmDeleteLabelGroupPopup.IsOpen = false;
        }

        private void OnDeleteLabelGroupCancelled(object sender, RoutedEventArgs e)
        {
            ConfirmDeleteLabelGroupPopup.IsOpen = false;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Private event handlers
        #region Public Properties

        private string? _currentLabelName;
        private string? _currentLabelGroupName;
        private LabelGroupViewModel? _currentSourceLabelGroup;
        private NotesLabel _currentLabel;

        /// <summary>
        /// Gets or sets the label pending addition or removal.
        /// </summary>
        public NotesLabel CurrentLabel
        {
            get => _currentLabel;
            set
            {
                _currentLabel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the label name for the Add/Remove Label confirmation.
        /// </summary>
        public string? CurrentLabelName
        {
            get => _currentLabelName;
            set
            {
                if (value == _currentLabelName) return;
                _currentLabelName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the label group name for the Add/Remove Label confirmation.
        /// </summary>
        public string? CurrentLabelGroupName
        {
            get => _currentLabelGroupName;
            set
            {
                if (value == _currentLabelGroupName) return;
                _currentLabelGroupName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PopupLabelGroupVisibility));
            }
        }        
        
        /// <summary>
        /// Gets or sets the source label group for the Add Label Group confirmation.
        /// </summary>
        public LabelGroupViewModel? CurrentSourceLabelGroup
        {
            get => _currentSourceLabelGroup;
            set
            {
                if (value == _currentSourceLabelGroup) return;
                _currentSourceLabelGroup = value;
                OnPropertyChanged();
            }
        }

        public Visibility PopupLabelGroupVisibility => string.IsNullOrWhiteSpace(CurrentLabelGroupName) ? Visibility.Collapsed : Visibility.Visible;

        /// <summary>
        /// Gets or sets the currently-selected label group.
        /// </summary>
        public LabelGroupViewModel CurrentLabelGroup
        {
            get => (LabelGroupViewModel)GetValue(CurrentLabelGroupProperty);
            set => SetValue(CurrentLabelGroupProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="UserId"/> for the current user.
        /// </summary>
        public UserId CurrentUserId
        {
            get => (UserId)GetValue(CurrentUserIdProperty);
            set => SetValue(CurrentUserIdProperty, value);
        }

        /// <summary>
        /// Gets or sets the default <see cref="LabelGroupViewModel"/> for the user.
        /// </summary>
        public LabelGroupViewModel DefaultLabelGroup
        {
            get => (LabelGroupViewModel)GetValue(DefaultLabelGroupProperty);
            set => SetValue(DefaultLabelGroupProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="EventAggregator"/> to be used for participating in the Caliburn Micro eventing system.
        /// </summary>
        public static IEventAggregator? EventAggregator { get; set; }

        /// <summary>
        /// Gets or sets the label group collection associated with the editor.
        /// </summary>
        public LabelGroupViewModelCollection LabelGroups
        {
            get => (LabelGroupViewModelCollection)GetValue(LabelGroupsProperty);
            set => SetValue(LabelGroupsProperty, value);
        }

        /// <summary>
        /// Gets or sets the background brush for individual label boxes.
        /// </summary>
        public SolidColorBrush LabelBackground
        {
            get => (SolidColorBrush)GetValue(LabelBackgroundProperty);
            set => SetValue(LabelBackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the corner radius for individual label boxes.
        /// </summary>
        public CornerRadius LabelCornerRadius
        {
            get => (CornerRadius)GetValue(LabelCornerRadiusProperty);
            set => SetValue(LabelCornerRadiusProperty, value);
        }

        /// <summary>
        /// Gets or sets the font family for individual label boxes.
        /// </summary>
        public FontFamily LabelFontFamily
        {
            get => (FontFamily)GetValue(LabelFormFontFamilyProperty);
            set => SetValue(LabelFormFontFamilyProperty, value);
        }

        /// <summary>
        /// Gets or sets the font size for individual label boxes.
        /// </summary>
        public double LabelFontSize
        {
            get => (double)GetValue(LabelFontSizeProperty);
            set => SetValue(LabelFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the font style for individual label boxes.
        /// </summary>
        public FontStyle LabelFontStyle
        {
            get => (FontStyle)GetValue(LabelFontStyleProperty);
            set => SetValue(LabelFontStyleProperty, value);
        }

        /// <summary>
        /// Gets or sets the font weight for individual label boxes.
        /// </summary>
        public FontWeight LabelFontWeight
        {
            get => (FontWeight)GetValue(LabelFontWeightProperty);
            set => SetValue(LabelFontWeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the margin for individual label boxes.
        /// </summary>
        public Thickness LabelMargin
        {
            get => (Thickness)GetValue(LabelMarginProperty);
            set => SetValue(LabelMarginProperty, value);
        }

        /// <summary>
        /// Gets or sets the padding for individual label boxes.
        /// </summary>
        public Thickness LabelPadding
        {
            get => (Thickness)GetValue(LabelPaddingProperty);
            set => SetValue(LabelPaddingProperty, value);
        }

        /// <summary>
        /// Gets or sets a collection of <see cref="NotesLabel"/> objects for auto selection in the control.
        /// </summary>
        public LabelCollection LabelSuggestions
        {
            get => (LabelCollection)GetValue(LabelSuggestionsProperty);
            set => SetValue(LabelSuggestionsProperty, value);
        }

        /// <summary>
        /// Gets or sets the width of individual label text boxes.
        /// </summary>
        /// <remarks><see cref="Double.NaN"/> is equivalent to "Auto" in XAML.</remarks>
        public double LabelWidth
        {
            get => (double)GetValue(LabelWidthProperty);
            set => SetValue(LabelWidthProperty, value);
        }

        /// <summary>
        /// Gets or sets a collection of <see cref="NotesLabel"/> objects to display in the control.
        /// </summary>
        public LabelCollection Labels
        {
            get => (LabelCollection)GetValue(LabelsProperty);
            set => SetValue(LabelsProperty, value);
        }

        /// <summary>
        /// Gets or sets the visibility of the label group delete button.
        /// </summary>
        public Visibility LabelGroupDeleteVisibility => CurrentLabelGroup is { LabelGroupId: not null } ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
         /// Gets or sets the visibility of the new label group validation message.
         /// </summary>
        public Visibility NewLabelGroupErrorVisibility
        {
            get => _newLabelGroupErrorVisibility;
            set
            {
                if (value == _newLabelGroupErrorVisibility) return;
                _newLabelGroupErrorVisibility = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the visibility of the new label group initialization message.
        /// </summary>
        public Visibility NewLabelGroupInitializationVisibility
        {
            get => _newLabelGroupInitializeVisibility;
            set
            {
                if (value == _newLabelGroupInitializeVisibility) return;
                _newLabelGroupInitializeVisibility = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the name for a new label group.
        /// </summary>
        public string NewLabelGroupName
        {
            get => _newLabelGroupName;
            set
            {
                if (value == _newLabelGroupName) return;
                _newLabelGroupName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="NoteViewModel"/> that the labels are associated with.
        /// </summary>
        public NoteViewModel Note
        {
            get => (NoteViewModel)GetValue(NoteProperty);
            set => SetValue(NoteProperty, value);
        }

        #endregion
        #region Public Events

        /// <summary>
        /// Occurs when an new label is added.
        /// </summary>
        public event RoutedEventHandler LabelAdded
        {
            add => AddHandler(LabelAddedEvent, value);
            remove => RemoveHandler(LabelAddedEvent, value);
        }

        /// <summary>
        /// Occurs when an new label is associated to a label group.
        /// </summary>
        public event RoutedEventHandler LabelAssociated
        {
            add => AddHandler(LabelAssociatedEvent, value);
            remove => RemoveHandler(LabelAssociatedEvent, value);
        }

        /// <summary>
        /// Occurs when a label is deleted.
        /// </summary>
        public event RoutedEventHandler LabelDeleted
        {
            add => AddHandler(LabelDeletedEvent, value);
            remove => RemoveHandler(LabelDeletedEvent, value);
        }

        /// <summary>
        /// Occurs when a label is disassociated.
        /// </summary>
        public event RoutedEventHandler LabelDisassociated
        {
            add => AddHandler(LabelDisassociatedEvent, value);
            remove => RemoveHandler(LabelDisassociatedEvent, value);
        }

        /// <summary>
        /// Occurs when an new label group is added.
        /// </summary>
        public event RoutedEventHandler LabelGroupAdded
        {
            add => AddHandler(LabelGroupAddedEvent, value);
            remove => RemoveHandler(LabelGroupAddedEvent, value);
        }

        /// <summary>
        /// Occurs when a label is added to a label group.
        /// </summary>
        public event RoutedEventHandler LabelGroupLabelAdded
        {
            add => AddHandler(LabelGroupLabelAddedEvent, value);
            remove => RemoveHandler(LabelGroupLabelAddedEvent, value);
        }

        /// <summary>
        /// Occurs when a label is removed from a label group.
        /// </summary>
        public event RoutedEventHandler LabelGroupLabelRemoved
        {
            add => AddHandler(LabelGroupLabelRemovedEvent, value);
            remove => RemoveHandler(LabelGroupLabelRemovedEvent, value);
        }

        /// <summary>
        /// Occurs when an existing label group is removed.
        /// </summary>
        public event RoutedEventHandler LabelGroupRemoved
        {
            add => AddHandler(LabelGroupRemovedEvent, value);
            remove => RemoveHandler(LabelGroupRemovedEvent, value);
        }

        /// <summary>
        /// Occurs when an existing label group is selected.
        /// </summary>
        public event RoutedEventHandler LabelGroupSelected
        {
            add => AddHandler(LabelGroupSelectedEvent, value);
            remove => RemoveHandler(LabelGroupSelectedEvent, value);
        }

        /// <summary>
        /// Occurs when an existing label is removed.
        /// </summary>
        public event RoutedEventHandler LabelRemoved
        {
            add => AddHandler(LabelRemovedEvent, value);
            remove => RemoveHandler(LabelRemovedEvent, value);
        }

        /// <summary>
        /// Occurs when an existing label suggestion is selected.
        /// </summary>
        public event RoutedEventHandler LabelSelected
        {
            add => AddHandler(LabelSelectedEvent, value);
            remove => RemoveHandler(LabelSelectedEvent, value);
        }
        
        /// <summary>
        /// Occurs when an existing label is updated.
        /// </summary>
        public event RoutedEventHandler LabelUpdated
        {
            add => AddHandler(LabelUpdatedEvent, value);
            remove => RemoveHandler(LabelUpdatedEvent, value);
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion Public events

        public LabelsEditor()
        {
            InitializeComponent();

            EventAggregator?.SubscribeOnUIThread(this);
        }

        public Task HandleAsync(NoteLabelDetachedMessage message, CancellationToken cancellationToken)
        {
            Labels.RemoveIfExists(message.Label);
            return Task.CompletedTask;
        }

        ~LabelsEditor()
        {
            EventAggregator?.Unsubscribe(this);
        }
    }
}
