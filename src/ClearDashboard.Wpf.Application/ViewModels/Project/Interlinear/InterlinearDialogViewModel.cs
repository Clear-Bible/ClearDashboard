using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.Wpf.Application.Helpers;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Project.Interlinear
{
    public class InterlinearDialogViewModel : DashboardApplicationScreen
    {

        private TranslationSource? _translationSource;
        private AlignmentSetId? _selectedAlignmentSet;

        public AlignmentSetId? SelectedAlignmentSet
        {
            get => _selectedAlignmentSet;
            set => Set(ref _selectedAlignmentSet, value);
        }

        private List<AlignmentSetId>? _alignmentSets;
        private bool _canCreate;
        private bool _canCancel;
        public List<AlignmentSetId>? AlignmentSets
        {
            get => _alignmentSets;
            set => Set(ref _alignmentSets, value);
        }

        public ParallelCorpusId? ParallelCorpusId { get; set; }
       

        public InterlinearDialogViewModel()
        {
            AlignmentSets = new List<AlignmentSetId>();
        }

        public InterlinearDialogViewModel(ParallelCorpusId parallelCorpusId, TranslationSource? translationSource, INavigationService navigationService,
            ILogger<InterlinearDialogViewModel> logger, DashboardProjectManager? projectManager, IEventAggregator eventAggregator,
            IWindowManager windowManager, IMediator mediator, ILifetimeScope lifetimeScope)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            _translationSource = translationSource;
            ParallelCorpusId = parallelCorpusId;
            Logger!.LogInformation("'InterlinearDialogViewModel' ctor called.");
            AlignmentSets = new List<AlignmentSetId>();
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            var tuple = await AlignmentSet.GetAllAlignmentSetIds(Mediator!, ParallelCorpusId,
                new UserId(ProjectManager!.CurrentUser.Id, ProjectManager.CurrentUser.FullName!));

            AlignmentSets = tuple.Select(t => t.alignmentSetId).ToList();
            SelectedAlignmentSet = AlignmentSets.FirstOrDefault();
            CanCreate = true;
            CanCancel = true;
        }

        public bool CanCreate
        {
            get => _canCreate;
            set => Set(ref _canCreate, value);
        }

        public bool CanCancel
        {
            get => _canCancel;
            set => Set(ref _canCancel, value);
        }

        public async void Create()
        {
            await TryCloseAsync(false);
        }
    }
}
