using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.ParatextPlugin.CQRS.Features.CheckUsfm;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup.ProjectTemplate
{
    public class ScopeSelectionStepViewModel : DashboardApplicationWorkflowStepViewModel<StartupDialogViewModel>
    {
        public SelectedBookManager SelectedBookManager { get; private set; }

        private bool _continueEnabled;
        public bool ContinueEnabled
        {
            get => _continueEnabled;
            set => Set(ref _continueEnabled, value);
        }

        public ScopeSelectionStepViewModel(DashboardProjectManager projectManager, SelectedBookManager selectedBookManager,
            INavigationService navigationService, ILogger<ProjectSetupViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            SelectedBookManager = selectedBookManager;

            CanMoveForwards = true;
            CanMoveBackwards = true;
            EnableControls = true;
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var usfmErorsByParatextProjectId = new Dictionary<string, IEnumerable<UsfmError>>();

            if (ParentViewModel!.SelectedParatextProject != null)
            {
                var result = await Mediator!.Send(new GetCheckUsfmQuery(ParentViewModel!.SelectedParatextProject.Id), cancellationToken);
                if (!result.Success)
                {
                    Logger!.LogError($"Error checking USFM for Paratext project: '{ParentViewModel!.SelectedParatextProject.Name}': {result.Message}");
                }

                usfmErorsByParatextProjectId.Add(ParentViewModel!.SelectedParatextProject.Id!, result.Data!.UsfmErrors);
            }

            if (ParentViewModel!.SelectedParatextBtProject != null)
            {
                var result = await Mediator!.Send(new GetCheckUsfmQuery(ParentViewModel!.SelectedParatextBtProject.Id), cancellationToken);
                if (!result.Success)
                {
                    Logger!.LogError($"Error checking USFM for Paratext BT project: '{ParentViewModel!.SelectedParatextBtProject.Name}': {result.Message}");
                }

                usfmErorsByParatextProjectId.Add(ParentViewModel!.SelectedParatextBtProject.Id!, result.Data!.UsfmErrors);
            }

            if (ParentViewModel!.SelectedParatextLwcProject != null)
            {
                var result = await Mediator!.Send(new GetCheckUsfmQuery(ParentViewModel!.SelectedParatextLwcProject.Id), cancellationToken);
                if (!result.Success)
                {
                    Logger!.LogError($"Error checking USFM for Paratext LWC project: '{ParentViewModel!.SelectedParatextLwcProject.Name}': {result.Message}");
                }

                usfmErorsByParatextProjectId.Add(ParentViewModel!.SelectedParatextLwcProject.Id!, result.Data!.UsfmErrors);
            }

            await SelectedBookManager!.InitializeBooks(usfmErorsByParatextProjectId, CancellationToken.None);
            ContinueEnabled = SelectedBookManager.SelectedBooks.Any();
            await base.OnActivateAsync(cancellationToken);
        }

        public async Task CreateProjectTemplate()
        {
            // FIXME
            await ParentViewModel!.GoToStep(1);
            //ParentViewModel?.Ok();
        }
    }


}
