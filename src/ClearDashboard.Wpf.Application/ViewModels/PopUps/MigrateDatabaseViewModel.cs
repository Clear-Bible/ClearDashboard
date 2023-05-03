using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Models;
using ParallelCorpus = ClearDashboard.DAL.Alignment.Corpora.ParallelCorpus;
using ClearDashboard.DataAccessLayer.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.ViewModels.Startup;
using ClearDashboard.DataAccessLayer;
using System.Reflection;
using ClearDashboard.DataAccessLayer.Features.Projects;
using System.Xml.XPath;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class MigrateDatabaseViewModel: DashboardApplicationScreen
    {
        #region Member Variables

        private readonly ILogger<AboutViewModel> _logger;
        private readonly DashboardProjectManager? _projectManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly IMediator _mediator;
        private readonly ILifetimeScope? _lifetimeScope;
        private readonly ILocalizationService _localizationService;

        private TopLevelProjectIds _topLevelProjectIds;
        private ProjectDbContext _projectDbContext;

        private bool _completedRuns = false;

        #endregion //Member Variables


        #region Public Properties

        public DashboardProject Project { get; set; }

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
                NotifyOfPropertyChange(nameof(ProgressCircle));
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

        #endregion //Observable Properties


        #region Constructor

        public MigrateDatabaseViewModel()
        {
            // no-op
        }

        public MigrateDatabaseViewModel(INavigationService navigationService, 
            ILogger<AboutViewModel> logger,
            DashboardProjectManager? projectManager, 
            IEventAggregator eventAggregator, 
            IMediator mediator, 
            ILifetimeScope? lifetimeScope, 
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _logger = logger;
            _projectManager = projectManager;
            _eventAggregator = eventAggregator;
            _mediator = mediator;
            _lifetimeScope = lifetimeScope;
            _localizationService = localizationService;
        }

        protected override void OnViewReady(object view)
        {
            base.OnViewReady(view);
        }


        protected override async void OnViewLoaded(object view)
        {
            StartEnabled = false;

            ProjectManager!.CurrentDashboardProject = Project;

            await Task.Run(async () =>
            {
                await ProjectManager!.LoadProject(Project.ProjectName);

                _topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);
            });

            
            var parallelCorpusIds = _topLevelProjectIds.ParallelCorpusIds;

            ParallelIdLists = new();

            foreach (var parallelCorpusId in parallelCorpusIds)
            {
                ParallelIdLists.Add(new ParallelIdList
                {
                    Status = ParallelIdList.JobStatus.NeedToRun,
                    ParallelCorpusId = parallelCorpusId,
                });
            }

            ProgressCircle = Visibility.Collapsed;

            StartEnabled = true;

            base.OnViewLoaded(view);
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

            ProgressCircle = Visibility.Visible;

            foreach (var parallelId in ParallelIdLists)
            {
                parallelId.Status = ParallelIdList.JobStatus.Working;
                NotifyOfPropertyChange(nameof(ParallelIdLists));

                var parallelCorpus = await ParallelCorpus.Get(_mediator, parallelId.ParallelCorpusId);
                await Task.Run(async () =>
                {
                    await RunRegenerationOnParallelCorpus(parallelCorpus);
                });

                parallelId.Status = ParallelIdList.JobStatus.Completed;
                NotifyOfPropertyChange(nameof(ParallelIdLists));
            }

            // update the database to current app version
            var result = await Mediator.Send(new LoadProjectQuery(Project.ProjectName));
            if (result.Success)
            {
                var project = result.Data;
                project.AppVersion = Assembly.GetEntryAssembly()!.GetName().Version.ToString();
                
                var result2 = await Mediator.Send(new UpdateProjectCommand(project));
                if (result2.Success == false)
                {
                    _logger.LogError(result2.Message);
                }
            }

            _completedRuns = true;

            CloseEnabled = true;

            PlaySound.PlaySoundFromResource(SoundType.Success);
            ProgressCircle = Visibility.Collapsed;
        }

        public async void Close()
        {
            if (_completedRuns)
            {
                await ProjectPickerViewModel.RefreshProjectList();
            }

            this.TryCloseAsync();
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
