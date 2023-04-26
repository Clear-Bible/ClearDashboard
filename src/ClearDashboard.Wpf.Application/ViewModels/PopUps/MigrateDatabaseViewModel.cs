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

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class MigrateDatabaseViewModel: DashboardApplicationScreen
    {
        #region Member Variables

        private readonly ILogger<AboutViewModel> _logger;
        private readonly DashboardProjectManager? _projectManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly IMediator _mediator;
        private readonly ILocalizationService _localizationService;

        private TopLevelProjectIds _topLevelProjectIds;
        private ProjectDbContext _projectDbContext;

        #endregion //Member Variables


        #region Public Properties

        public DashboardProject Project { get; set; }

        #endregion //Public Properties


        #region Observable Properties

        #endregion //Observable Properties


        #region Constructor

        public MigrateDatabaseViewModel()
        {
            
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
            _localizationService = localizationService;
        }

        protected override async void OnViewReady(object view)
        {
            ProjectManager!.CurrentDashboardProject = Project;

            await ProjectManager!.LoadProject(Project.ProjectName);

            _topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator!);

            var parallelCorpusIds = _topLevelProjectIds.ParallelCorpusIds;

            base.OnViewReady(view);
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


        #endregion // Methods
    }
}
