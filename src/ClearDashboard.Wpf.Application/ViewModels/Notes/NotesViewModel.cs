﻿using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.Views.Notes;
using FuzzyString;
using MediatR;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ClearDashboard.Wpf.Application.ViewModels.Notes
{
    public class NotesViewModel : 
        ToolViewModel, 
        IHandle<NoteAddedMessage>,
        IHandle<NoteDeletedMessage>,
        IHandle<NoteUpdatedMessage>,
        IHandle<NoteLabelAttachedMessage>,
        IHandle<NoteLabelDetachedMessage>
    {
        private const string TaskName = "Notes";
        private const int ToleranceContainsFuzzyAssociationsDescriptions = 1;
        private const int ToleranceContainsFuzzyNoteText = 1;

        private readonly LongRunningTaskManager longRunningTaskManager_;
        private readonly NoteManager? noteManager_;
        private NotesView view_;
        private LongRunningTask? currentLongRunningTask_;

        public enum FilterNoteStatusEnum
        {
            Any,
            Open,
            Resolved
        }

        private Visibility _progressBarVisibility = Visibility.Visible;
        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set
            {
                _progressBarVisibility = value;
                NotifyOfPropertyChange(() => ProgressBarVisibility);
            }
        }

        private FilterNoteStatusEnum filterStatus_;
        public FilterNoteStatusEnum FilterStatus
        {
            get => filterStatus_;
            set
            {
                filterStatus_ = value;
                NotifyOfPropertyChange(() => FilterStatus);
                NotesCollectionView.Refresh();
            }
        }


        private string filterAssociationsDescriptionText_ = string.Empty;
        public string FilterAssociationsDescriptionText
        {
            get => filterAssociationsDescriptionText_;
            set
            {
                value ??= string.Empty;

                filterAssociationsDescriptionText_ = value;
                //NotifyOfPropertyChange(() => FilterAssociationsDescriptionText);
                NotesCollectionView.Refresh();
            }
        }

        private string filterNoteText_ = string.Empty;
        public string FilterNoteText
        {
            get => filterNoteText_;
            set
            {
                value ??= string.Empty;

                filterNoteText_ = value;
                //NotifyOfPropertyChange(() => FilterAssociationsDescriptionText);
                NotesCollectionView.Refresh();
            }
        }

        private NoteViewModel _selectedNoteViewModel;
        public NoteViewModel SelectedNoteViewModel
        {
            get => _selectedNoteViewModel;
            set
            {
                _selectedNoteViewModel = value;
                NotifyOfPropertyChange(() => SelectedNoteViewModel);
                UpdateSelectedNote(SelectedNoteViewModel);
            }
        }

        private string selectedNoteText_ = string.Empty;
        public string SelectedNoteText
        {
            get => selectedNoteText_;
            set
            {
                selectedNoteText_ = value;
                NotifyOfPropertyChange(() => SelectedNoteText);
            }
        }

        private string? message_;
        public string? Message
        {
            get => message_;
            set => Set(ref message_, value);
        }

        private ICollectionView notesCollectionView_;
        public ICollectionView NotesCollectionView
        {
            get => notesCollectionView_;
            set
            {
                notesCollectionView_ = value;
            }
        }

        private BindableCollection<NoteViewModel> noteViewModels_ = new();
        public BindableCollection<NoteViewModel> NoteViewModels
        {
            get => noteViewModels_;
            set
            {
                noteViewModels_ = value;
            }
        }

        private BindableCollection<DAL.Alignment.Notes.Label> filterLabelsChoices_ = new();
        public BindableCollection<DAL.Alignment.Notes.Label> FilterLabelsChoices
        {
            get => filterLabelsChoices_;
            set
            {
                filterLabelsChoices_ = value;
                //NotifyOfPropertyChange(() => FilterLabelsChoices);
            }
        }

        private BindableCollection<DAL.Alignment.Notes.Label> filterLabels_ = new();
        public BindableCollection<DAL.Alignment.Notes.Label> FilterLabels
        {
            get => filterLabels_;
            set
            {
                filterLabels_ = value;
                NotesCollectionView.Refresh();
            }
        }


        private BindableCollection<string> filterUsersChoices_ = new();
        public BindableCollection<string> FilterUsersChoices
        {
            get => filterUsersChoices_;
            set
            {
                filterUsersChoices_ = value;
            }
        }

        private BindableCollection<string> filterUsers_ = new();
        public BindableCollection<string> FilterUsers
        {
            get => filterUsers_;
            set
            {
                filterUsers_ = value;
                NotesCollectionView.Refresh();
            }
        }

        private BindableCollection<DAL.Alignment.Notes.Label> selectedNoteLabels_ = new();
        public BindableCollection<DAL.Alignment.Notes.Label> SelectedNoteLabels
        {
            get => selectedNoteLabels_;
            set
            {
                selectedNoteLabels_ = value;
            }
        }

        private BindableCollection<NoteViewModel> selectedNoteRepliesNoteViewModels_ = new();
        public BindableCollection<NoteViewModel> SelectedNoteRepliesNoteViewModels
        {
            get => selectedNoteRepliesNoteViewModels_;
            set
            {
                selectedNoteRepliesNoteViewModels_ = value;
            }
        }

        private BindableCollection<string> selectedNoteAssociationDescriptions_ = new();
        public BindableCollection<string> SelectedNoteAssociationDescriptions
        {
            get => selectedNoteAssociationDescriptions_;
            set
            {
                selectedNoteAssociationDescriptions_ = value;
            }
        }

        public NotesViewModel()
        {
            // used by Caliburn Micro for design time    
        }

        public NotesViewModel(
            INavigationService navigationService,
            ILogger<NotesViewModel> logger,
            DashboardProjectManager? projectManager,
            IEventAggregator? eventAggregator,
            IMediator mediator,
            ILifetimeScope lifetimeScope,
            LongRunningTaskManager longRunningTaskManager,
            ILocalizationService localizationService,
            NoteManager noteManager)
            : base(navigationService, logger, projectManager, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            longRunningTaskManager_ = longRunningTaskManager;
            noteManager_ = noteManager;

            //FIXME: why is this here and in MainViewModel line 1113??
            Title = "⌺ " + LocalizationService!.Get("MainView_WindowsNotes");
            ContentId = "NOTES";
            DockSide = DockSide.Bottom;

            NotesCollectionView = CollectionViewSource.GetDefaultView(noteViewModels_);
            NotesCollectionView.Filter = FilterNotesCollectionView;
            NotesCollectionView.SortDescriptions.Clear();
            NotesCollectionView.SortDescriptions.Add(new SortDescription("Created", ListSortDirection.Ascending));
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);
        }
        protected override void OnViewAttached(object view, object context)
        {
            view_ = (NotesView)view;
            Logger!.LogInformation("OnViewAttached");
            base.OnViewAttached(view, context);
        }

        protected override void OnViewLoaded(object view)
        {
            Logger!.LogInformation("OnViewLoaded");
            base.OnViewLoaded(view);
        }

        protected override async void OnViewReady(object view)
        {
            await GetAllNotesAndSetNoteViewModelsAsync();

            Logger!.LogInformation("OnViewReady");
            base.OnViewReady(view);
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            await base.OnDeactivateAsync(close, cancellationToken);
        }

        private bool FilterNotesCollectionView(object obj)
        {
            if (obj is NoteViewModel noteViewModel)
            {
                var associationDescriptions = noteViewModel.Associations
                    .Select(a => a.Description)
                    .Aggregate((all, next) => $"{all} {next}");

                if (
                    (FilterStatus.Equals(FilterNoteStatusEnum.Any) || FilterStatus.ToString().Equals(noteViewModel.NoteStatus.ToString())) &&
                    (FilterUsers.Count() == 0 || FilterUsers.Contains(noteViewModel.ModifiedBy)) &&
                    (FilterLabels.Count() == 0 || FilterLabels.Intersect(noteViewModel.Labels).Any()) &&
                    associationDescriptions.ContainsFuzzy(FilterAssociationsDescriptionText, ToleranceContainsFuzzyAssociationsDescriptions) &&
                    noteViewModel.Text.ContainsFuzzy(FilterNoteText, ToleranceContainsFuzzyNoteText)
                )
                {
                    return true;
                }
                else
                    return false;
            }
            throw new Exception($"object provided to FilterNotesCollectionView is type {obj.GetType().FullName} and not type NoteViewModel");
        }

        public void LaunchMirrorView(double actualWidth, double actualHeight)
        {
            LaunchMirrorView<NotesView>.Show(this, actualWidth, actualHeight);
        }


        private async Task<IEnumerable<NoteViewModel>> AssembleNotes(
            NoteManager noteManager,
            CancellationToken cancellationToken,
            string taskName, 
            Func<string, LongRunningTaskStatus, CancellationToken, string?, Exception?, Task> reportStatus)
        {
            HashSet<NoteId> noteIds = new HashSet<NoteId>();

            await reportStatus(taskName, LongRunningTaskStatus.Running, cancellationToken, "Getting all note ids", null);

            _ = (await noteManager.GetNoteIdsAsync(cancellationToken: cancellationToken))
                    .SelectMany(d => d.Value)
                    .Select(nid =>
                    {
                        noteIds.Add(nid);
                        return nid;
                    })
                    .ToList();

            await reportStatus(taskName, LongRunningTaskStatus.Running, cancellationToken, "Collecting note details for notes", null);

            return await noteIds
                .Select(async nid => await noteManager.GetNoteDetailsAsync(nid))
                .WhenAll();
        }
        private void UpdateSelectedNote(NoteViewModel? selectedNoteViewModel)
        {
            SelectedNoteLabels.Clear();
            selectedNoteViewModel?.Labels
                .Select(l => { 
                    SelectedNoteLabels.Add(l);
                    return l;
                })
                .ToList();

            SelectedNoteRepliesNoteViewModels.Clear();
            if (selectedNoteViewModel != null)
            {
                SelectedNoteRepliesNoteViewModels = selectedNoteViewModel.Replies;
            }

            SelectedNoteAssociationDescriptions.Clear();
            if (selectedNoteViewModel != null)
            {
                selectedNoteViewModel.Associations
                    .Select(a =>
                    {
                        SelectedNoteAssociationDescriptions.Add(a.Description);
                        return a;
                    })
                    .ToList();
            }

            SelectedNoteText = string.Empty;
            if (selectedNoteViewModel != null)
                SelectedNoteText = selectedNoteViewModel.Text;
        }
        public async Task GetAllNotesAndSetNoteViewModelsAsync()
        {
            try
            {
                ProgressBarVisibility = Visibility.Visible;

                var taskName = "Get notes";
                var processStatus = await RunLongRunningTask(
                    taskName,
                    (cancellationToken) => AssembleNotes(noteManager_!, cancellationToken, taskName, SendBackgroundStatus),
                    (noteVms) =>
                    {
                        BindableCollection<NoteViewModel> noteViewModels = new();

                        NoteViewModels.Clear();

                        noteVms
                            .Select(nvm =>
                            {
                                NoteViewModels.Add(nvm);
                                return nvm;
                            })
                            .ToList();
                        NoteViewModels.NotifyOfPropertyChange(NoteViewModels.GetType().Name);

                        FilterUsersChoices.Clear();
                        noteVms
                            .Select(nvm => nvm.ModifiedBy)
                            .Distinct()
                            .OrderBy(mb => mb)
                            .Select(mb =>
                            {
                                FilterUsersChoices.Add(mb);
                                return mb;
                            })
                            .ToList();

                        FilterLabelsChoices.Clear();
                        noteVms
                            .SelectMany(nvm => nvm.Labels)
                            .Distinct()
                            .OrderBy(l => l.Text)
                            .Select(l =>
                            {
                                FilterLabelsChoices.Add(l);
                                return l;
                            })
                            .ToList();
                    });

                switch (processStatus)
                {
                    case LongRunningTaskStatus.Completed:
                        break;
                    case LongRunningTaskStatus.Failed:
                        break;
                    case LongRunningTaskStatus.Cancelled:
                        break;
                    case LongRunningTaskStatus.NotStarted:
                        break;
                    case LongRunningTaskStatus.Running:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                Logger!.LogError(ex, TaskName);
            }
            finally
            {
                ProgressBarVisibility = Visibility.Hidden;
            }
        }
        public async Task SendBackgroundStatus(string name, LongRunningTaskStatus status, CancellationToken cancellationToken, string? description = null, Exception? exception = null)
        {
            Message = !string.IsNullOrEmpty(description) ? description : null;
            var backgroundTaskStatus = new BackgroundTaskStatus
            {
                Name = name,
                EndTime = DateTime.Now,
                Description = !string.IsNullOrEmpty(description) ? description : null,
                ErrorMessage = exception != null ? $"{exception}" : null,
                TaskLongRunningProcessStatus = status
            };
            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(backgroundTaskStatus), cancellationToken);
        }
        protected async Task<LongRunningTaskStatus> RunLongRunningTask<TResult>(
            string taskName,
            Func<CancellationToken, Task<TResult>> awaitableFunction,
            Action<TResult> ProcessResult)
        {
            IsBusy = true;
            currentLongRunningTask_ = longRunningTaskManager_!.Create(taskName, LongRunningTaskStatus.Running);
            var cancellationToken = currentLongRunningTask_!.CancellationTokenSource?.Token
                ?? throw new Exception("Cancellation source is not set.");
            try
            {
                currentLongRunningTask_.Status = LongRunningTaskStatus.Running;
                await SendBackgroundStatus(
                    taskName,
                    LongRunningTaskStatus.Running,
                    cancellationToken,
                    $"{taskName} running");
                Logger!.LogInformation(taskName);

                ProcessResult(await awaitableFunction(cancellationToken));

                await SendBackgroundStatus(
                    taskName,
                    LongRunningTaskStatus.Completed,
                    cancellationToken,
                    $"{taskName} complete");

                Logger!.LogInformation($"{taskName} complete.");
            }
            catch (OperationCanceledException)
            {
                Logger!.LogInformation($"{taskName}: operation cancelled.");
            }
            catch (MediatorErrorEngineException ex)
            {
                if (ex.Message.Contains("The operation was canceled."))
                {
                    Logger!.LogInformation($"{taskName}: operation cancelled.");
                }
                else
                {
                    Logger!.LogError(ex, $"{taskName}: unexpected error.");
                }

            }
            catch (Exception ex)
            {
                Logger!.LogError(ex, $"{taskName}: unexpected error.");
                if (!cancellationToken.IsCancellationRequested)
                {
                    await SendBackgroundStatus(taskName,
                        LongRunningTaskStatus.Failed,
                        cancellationToken,
                        exception: ex);
                }

                currentLongRunningTask_.Status = LongRunningTaskStatus.Failed;
            }
            finally
            {
                longRunningTaskManager_.TaskComplete(taskName);
                if (cancellationToken.IsCancellationRequested)
                {
                    currentLongRunningTask_.Status = LongRunningTaskStatus.Cancelled;
                    await SendBackgroundStatus(taskName,
                        LongRunningTaskStatus.Completed,
                        cancellationToken,
                        $"{taskName} canceled.");
                }
                IsBusy = false;
                Message = string.Empty;
            }
            return currentLongRunningTask_.Status;
        }

        public async Task UpdateNote(NoteViewModel noteViewModel)
        {
            await noteManager_!.UpdateNoteAsync(noteViewModel);
        }
        public async Task HandleAsync(NoteAddedMessage message, CancellationToken cancellationToken)
        {
            var noteViewModelWithDetails = await noteManager_!.GetNoteDetailsAsync(message.Note.NoteId!);
            NoteViewModels.Add(noteViewModelWithDetails);
        }

        public async Task HandleAsync(NoteDeletedMessage message, CancellationToken cancellationToken)
        {
            await GetAllNotesAndSetNoteViewModelsAsync();
        }

        public async Task HandleAsync(NoteUpdatedMessage message, CancellationToken cancellationToken)
        {
            await GetAllNotesAndSetNoteViewModelsAsync();
        }

        public async Task HandleAsync(NoteLabelAttachedMessage message, CancellationToken cancellationToken)
        {
            await GetAllNotesAndSetNoteViewModelsAsync();
        }

        public async Task HandleAsync(NoteLabelDetachedMessage message, CancellationToken cancellationToken)
        {
            await GetAllNotesAndSetNoteViewModelsAsync();
        }
    }

    public static class Extensions
    {
        public static bool ContainsFuzzy(this string? str, string input, int tolerance)
        {
            if (str == null)
                throw new InvalidParameterEngineException(name: "str", value: "null");
            else if (input.Length == 0)
                return true;
            else
            {
                if (str.Contains(input))
                    return true;
                else
                    //see https://github.com/kdjones/fuzzystring
                    return str.LongestCommonSubsequence(input).Length
                        >=
                        input.Length - tolerance;
            }
        }
    }
}