using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Properties;
using ValidationResult = FluentValidation.Results.ValidationResult;
using ClearDashboard.Wpf.Application.ViewModels.Shell;
using SIL.Machine.Translation;
using ClearDashboard.DAL.Alignment;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.Interlinear
{
    public class InterlinearDialogViewModel : DashboardApplicationValidatingWorkflowStepViewModel<IParallelCorpusDialogViewModel, InterlinearDialogViewModel>
    {
        #region Member Variables

        private TranslationSource? _translationSource;
        private readonly IMediator _mediator;
        public ParallelCorpusId? ParallelCorpusId { get; set; }

        private readonly SystemPowerModes _systemPowerModes;
        private readonly LongRunningTaskManager _longRunningTaskManager;
        public LongRunningTask? CurrentTask { get; set; }
        private readonly BackgroundTasksViewModel _backgroundTasksViewModel;

        public IWordAlignmentModel WordAlignmentModel { get; set; }
        public ParallelCorpus ParallelTokenizedCorpus { get; set; }
        public TranslationSet? TranslationSet { get; set; }
        private TopLevelProjectIds _topLevelProjectIds;

        #endregion //Member Variables


        #region Observable Properties

        private string? _message;
        public string? Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

        private AlignmentSetId? _selectedAlignmentSet;
        public AlignmentSetId? SelectedAlignmentSet
        {
            get => _selectedAlignmentSet;
            set
            {
                Set(ref _selectedAlignmentSet, value);
                if (value is not null)
                {
                    CanCreate = true;
                }
                else
                {
                    CanCreate = false;
                }
            }
        }


        private List<AlignmentSetId>? _alignmentSets = new();
        public List<AlignmentSetId>? AlignmentSets
        {
            get => _alignmentSets;
            set => Set(ref _alignmentSets, value);
        }


        private bool _canCreate;
        public bool CanCreate
        {
            get => _canCreate;
            set => Set(ref _canCreate, value);
        }


        private bool _canCancel;
        public bool CanCancel
        {
            get => _canCancel;
            set => Set(ref _canCancel, value);
        }


        private string? _translationSetDisplayName;
        public string TranslationSetDisplayName
        {
            get => _translationSetDisplayName;
            set
            {
                Set(ref _translationSetDisplayName, value);
                ValidationResult = Validator.Validate(this);
                CanCreate = !string.IsNullOrEmpty(value) && ValidationResult.IsValid;
            }
        }

        #endregion //Observable Properties


        #region Constructor

        public InterlinearDialogViewModel()
        {
            // no-op
        }

        public InterlinearDialogViewModel(ParallelCorpusId parallelCorpusId,
            TranslationSource? translationSource, INavigationService navigationService,
            ILogger<InterlinearDialogViewModel> logger, DashboardProjectManager? projectManager, IEventAggregator eventAggregator,
            IWindowManager windowManager, IMediator mediator, ILifetimeScope lifetimeScope, IValidator<InterlinearDialogViewModel> validator, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator, localizationService)
        {
            _translationSource = translationSource;
            _mediator = mediator;
            ParallelCorpusId = parallelCorpusId;
            Logger!.LogInformation("'InterlinearDialogViewModel' ctor called.");
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            // get all the existing alignment sets for this parallelcorpusid
            AlignmentSets = (await AlignmentSet.GetAllAlignmentSetIds(
                    Mediator!, 
                    ParallelCorpusId,
                    new UserId(ProjectManager!.CurrentUser.Id, ProjectManager.CurrentUser.FullName!)))
                .ToList();

            // get all the existing translation sets for this parallelcorpusid
            var translationSets = (await TranslationSet.GetAllTranslationSetIds(_mediator, ParallelCorpusId)).ToList();

            //_topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);

            // parse down to only those which we do not have an existing translation set
            //for (var index = AlignmentSets.Count - 1; index >= 0; index--)
            //{
            //    var alignmentSet = AlignmentSets[index];
            //    var translationSet = translationSets.FirstOrDefault(x => x.AlignmentSetId == alignmentSet.Id);
            //    if (translationSet != null)
            //    {
            //        AlignmentSets.RemoveAt(index);
            //    }
            //}

            CanCreate = false;
            CanCancel = true;

            if (AlignmentSets.Count == 1)
            {
                SelectedAlignmentSet = AlignmentSets[0];
                CanCreate = true;
            }
            else
            {
                SelectedAlignmentSet = null;
            }
        }

        #endregion //Constructor


        #region Methods

        protected override ValidationResult? Validate()
        {
            return (!string.IsNullOrEmpty(TranslationSetDisplayName)) ? Validator.Validate(this) : null;
        }

        public async void Create()
        {
            //await TryCloseAsync(false);
        }

        public async void Close()
        {
            await TryCloseAsync(false);
        }

        public async Task<LongRunningTaskStatus> AddTranslationSet(string translationSetDisplayName)
        {
            IsBusy = true;

            // check to see if we want to run this in High Performance mode
            if (Settings.Default.EnablePowerModes && _systemPowerModes.IsLaptop)
            {
                await _systemPowerModes.TurnOnHighPerformanceMode();
            }

            var soundType = SoundType.Success;
            var taskName = ParallelCorpusDialogViewModel.TaskNames.TranslationSet;
            CurrentTask = _longRunningTaskManager.Create(taskName, LongRunningTaskStatus.Running);
            var cancellationToken = CurrentTask.CancellationTokenSource.Token;
            try
            {
                CurrentTask.Status = LongRunningTaskStatus.Running;
                await SendBackgroundStatus(taskName,
                    LongRunningTaskStatus.Running,
                    cancellationToken,
                    $"Creating the TranslationSet '{translationSetDisplayName}'...");

                var translationModel = WordAlignmentModel.GetTranslationTable();

                cancellationToken.ThrowIfCancellationRequested();

                var alignmentSet = AlignmentSet.Get(new AlignmentSetId(Guid.Parse("")), _mediator).Result;

                // RUSSELL - code review
                TranslationSet = await TranslationSet.Create(null, alignmentSet.AlignmentSetId,
                    translationSetDisplayName, new Dictionary<string, object>(),
                    ParallelTokenizedCorpus.ParallelCorpusId, Mediator, cancellationToken);



                await SendBackgroundStatus(taskName,
                    LongRunningTaskStatus.Completed,
                    cancellationToken,
                    $"Completed creation of the TranslationSet '{translationSetDisplayName}'.");
                Logger!.LogInformation($"Completed creating the TranslationSet '{translationSetDisplayName}'");

                CurrentTask.Status = LongRunningTaskStatus.Completed;
            }
            catch (OperationCanceledException ex)
            {
                Logger!.LogInformation($"AddTranslationSet - operation canceled.");
            }
            catch (MediatorErrorEngineException ex)
            {
                if (ex.Message.Contains("The operation was canceled."))
                {
                    Logger!.LogInformation($"AddTranslationSet - operation canceled.");
                }
                else
                {
                    Logger!.LogError(ex, "An unexpected Engine exception was thrown.");
                }

            }
            catch (Exception ex)
            {
                Logger!.LogError(ex, $"An unexpected error occurred while creating creating the TranslationSet.");
                if (!cancellationToken.IsCancellationRequested)
                {
                    soundType = SoundType.Error;
                    await SendBackgroundStatus(taskName,
                        LongRunningTaskStatus.Failed,
                        cancellationToken,
                        exception: ex);
                }

                CurrentTask.Status = LongRunningTaskStatus.Failed;
            }
            finally
            {
                _longRunningTaskManager.TaskComplete(taskName);
                if (cancellationToken.IsCancellationRequested)
                {
                    CurrentTask.Status = LongRunningTaskStatus.Cancelled;
                    await SendBackgroundStatus(taskName,
                        LongRunningTaskStatus.Completed,
                        cancellationToken,
                        $"Creation of the TranslationSet was canceled.'{translationSetDisplayName}'.");
                }
                IsBusy = false;
                Message = string.Empty;

                // check to see if there are still High Performance Tasks still out there
                var numTasks = _backgroundTasksViewModel.GetNumberOfPerformanceTasksRemaining();
                if (numTasks == 0 && _systemPowerModes.IsHighPerformanceEnabled)
                {
                    // shut down high performance mode
                    await _systemPowerModes.TurnOffHighPerformanceMode();
                }
            }

            PlaySound.PlaySoundFromResource(soundType);

            return CurrentTask.Status;

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

        public void SelectItem(AlignmentSetId item)
        {
            SelectedAlignmentSet = item;
            CanCreate = true;
        }

        #endregion // Methods
    }
}
