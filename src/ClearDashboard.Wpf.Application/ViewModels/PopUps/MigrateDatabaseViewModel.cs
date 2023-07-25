using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Features.Projects;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Startup;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearApplicationFoundation.Services;
using ParallelCorpus = ClearDashboard.DAL.Alignment.Corpora.ParallelCorpus;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class MigrateDatabaseViewModel: DashboardApplicationScreen
    {
        #region Member Variables

        private readonly ILogger<AboutViewModel> _logger;
        private readonly IMediator _mediator;

        private TopLevelProjectIds _topLevelProjectIds;
        private bool _closing = false;

        private bool _runsCompleted;

        #endregion //Member Variables


        #region Public Properties

        public DashboardProject? Project { get; set; }

        #endregion //Public Properties


        #region Observable Properties

        private ObservableCollection<ParallelIdList> _parallelIdLists = new();
        public ObservableCollection<ParallelIdList> ParallelIdLists
        {
            get => _parallelIdLists;
            set
            {
                _parallelIdLists = value;
                NotifyOfPropertyChange(() => ParallelIdLists);
            }
        }

        private Visibility _progressCircle = Visibility.Visible;
        public Visibility ProgressCircle
        {
            get => _progressCircle;
            set
            {
                _progressCircle = value; 
                NotifyOfPropertyChange(() =>ProgressCircle);
            }
        }


        private bool _startEnabled = true;
        public bool StartEnabled
        {
            get => _startEnabled;
            set
            {
                _startEnabled = value;
                NotifyOfPropertyChange(() => StartEnabled);
            }
        }

        private bool _closeEnabled = true;
        public bool CloseEnabled
        {
            get => _closeEnabled;
            set
            {
                _closeEnabled = value;
                NotifyOfPropertyChange(() => CloseEnabled);
            }
        }

        public ProjectPickerViewModel ProjectPickerViewModel { get; set; }
        public Guid ParallelId { get; set; }

        #endregion //Observable Properties


        #region Constructor

#pragma warning disable CS8618
        public MigrateDatabaseViewModel()
        {
            // no-op
        }


        public MigrateDatabaseViewModel(INavigationService navigationService, 
#pragma warning restore CS8618
            ILogger<AboutViewModel> logger,
            DashboardProjectManager? projectManager, 
            IEventAggregator eventAggregator, 
            IMediator mediator, 
            ILifetimeScope? lifetimeScope, 
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _logger = logger;
            _mediator = mediator;
        }

        protected override async void OnViewLoaded(object view)
        {
            StartEnabled = false;

            if (Project is not null)
            {
                ProjectManager!.CurrentDashboardProject = Project;

                await Task.Run(async () =>
                {
                    await ProjectManager!.LoadProject(Project.ProjectName);
                });
            }

            ProjectManager!.CheckForCurrentUser();


            await Task.Run(async () =>
            {
                _topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
            });

            var parallelCorpusIds = _topLevelProjectIds.ParallelCorpusIds;

            ParallelIdLists = new();
            if (Project is not null)  // we are loading all the parallel corpus for the project
            {
                foreach (var parallelCorpusId in parallelCorpusIds)
                {
                    ParallelIdLists.Add(new ParallelIdList
                    {
                        Status = ParallelIdList.JobStatus.NeedToRun,
                        ParallelCorpusId = parallelCorpusId,
                    });
                }
            }
            else
            {
                // we are only loading up one parallel corpus from the right click menu
                var parallelCorpus = parallelCorpusIds.FirstOrDefault(x => x.Id.ToString() == this.ParallelId.ToString());
                if (parallelCorpus is not null)
                {
                    ParallelIdLists.Add(new ParallelIdList
                    {
                        Status = ParallelIdList.JobStatus.NeedToRun,
                        ParallelCorpusId = parallelCorpus,
                    });
                }
            }

            ProgressCircle = Visibility.Collapsed;

            StartEnabled = true;

            base.OnViewLoaded(view);
        }


        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (_closing == false)
            {
                Close();
            }

            return base.OnDeactivateAsync(close, cancellationToken);
        }

        #endregion //Constructor


        #region Methods

        private async Task RunRegenerationOnParallelCorpus(ParallelCorpus parallelCorpus)
        {
            parallelCorpus.SourceTextIdToVerseMappings = new SourceTextIdToVerseMappingsFromVerseMappings(
                EngineParallelTextCorpus.VerseMappingsForAllVerses(
                    ((TokenizedTextCorpus)parallelCorpus.SourceCorpus).Versification,
                    ((TokenizedTextCorpus)parallelCorpus.TargetCorpus).Versification
                )
            );

            await parallelCorpus.Update(_mediator);
        }

        public async void Start()
        {
            StartEnabled = false;
            CloseEnabled = false;
            bool errorEncountered = false;

            ProgressCircle = Visibility.Visible;

            foreach (var parallelId in ParallelIdLists)
            {
                parallelId.Status = ParallelIdList.JobStatus.Working;
                NotifyOfPropertyChange(nameof(ParallelIdLists));

                var parallelCorpus = await ParallelCorpus.Get(_mediator, parallelId.ParallelCorpusId);
                await Task.Run(async () =>
                {
                    try
                    {
                        await RunRegenerationOnParallelCorpus(parallelCorpus);
                    }
                    catch (Exception e)
                    {
                        errorEncountered = true;
                        _logger.LogError(e.Message, e);
                    }
                });

                parallelId.Status = ParallelIdList.JobStatus.Completed;
                NotifyOfPropertyChange(nameof(ParallelIdLists));
            }

            _runsCompleted = false;

            if (Project is not null && errorEncountered == false) // we have updated the whole project so reset the version number if no errors
            {
                // update the database to current app version
                var result = await Mediator!.Send(new LoadProjectQuery(Project.ProjectName!));
                if (result.Success)
                {
                    var project = result.Data;
                    if (project != null)
                    {
                        project.AppVersion = Assembly.GetEntryAssembly()!.GetName().Version?.ToString();

                        var result2 = await Mediator.Send(new UpdateProjectCommand(project));
                        if (result2.Success == false)
                        {
                            _logger.LogError(result2.Message);
                        }
                    }
                }
                _runsCompleted = true;
            }
            
            CloseEnabled = true;

            PlaySound.PlaySoundFromResource();
            ProgressCircle = Visibility.Collapsed;
        }

        public async void Close()
        {
            if (_runsCompleted)
            {
                await ProjectPickerViewModel.RefreshProjectList();
            }

            _closing = true;

            await this.TryCloseAsync();
        }

        #endregion // Methods
    }

    public class ParallelIdList : INotifyPropertyChanged
    {
        public enum JobStatus
        {
            NeedToRun,
            Working,
            Completed
        }

        private JobStatus _status;
        public JobStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged($"{nameof(Status)}");
            }
        }


        private ParallelCorpusId parallelCorpusId;
        public ParallelCorpusId ParallelCorpusId
        {
            get => parallelCorpusId;
            set
            {
                parallelCorpusId = value; 
                OnPropertyChanged($"{nameof(ParallelCorpusId)}");
            }
        }




        public event PropertyChangedEventHandler? PropertyChanged;

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
    }
}
