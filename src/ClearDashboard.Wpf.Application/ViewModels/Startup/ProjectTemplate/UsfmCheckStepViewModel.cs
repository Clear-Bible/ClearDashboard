using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.ParatextPlugin.CQRS.Features.CheckUsfm;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog;
using System.Text;
using System.Windows;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup.ProjectTemplate
{
    public class UsfmCheckStepViewModel : DashboardApplicationWorkflowStepViewModel<StartupDialogViewModel>, IUsfmErrorHost
    {

        public SelectedBookManager SelectedBookManager { get; private set; }

        public UsfmCheckStepViewModel(DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger<ProjectSetupViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            CanMoveForwards = true;
            CanMoveBackwards = true;
            EnableControls = true;
        }


        private async Task<List<UsfmErrorsWrapper>> PerformUsfmErrorCheck(CancellationToken cancellationToken)
        {
            var usfmErrors = new List<UsfmErrorsWrapper>();
            if (ParentViewModel!.SelectedParatextProject != null)
            {
                var result = await Mediator!.Send(new GetCheckUsfmQuery(ParentViewModel!.SelectedParatextProject.Id),
                    cancellationToken);
                if (!result.Success)
                {
                    Logger!.LogError(
                        $"Error checking USFM for Paratext project: '{ParentViewModel!.SelectedParatextProject.Name}': {result.Message}");
                }
                else
                {
                    var errorTitle = (result.Data.NumberOfErrors == 0)
                        ? LocalizationService!.Get("AddParatextCorpusDialog_NoErrors")
                        : LocalizationService!.Get("AddParatextCorpusDialog_ErrorCount");

                    usfmErrors.Add(new UsfmErrorsWrapper
                    {
                        ErrorTitle = errorTitle,
                        ProjectId = ParentViewModel.SelectedParatextProject.Id!,
                        ProjectName = ParentViewModel!.SelectedParatextProject!.LongName!,
                        UsfmErrors = new ObservableCollection<UsfmError>(result.Data!.UsfmErrors)
                    });
                }
            }

            if (ParentViewModel!.SelectedParatextBtProject != null)
            {
                var result = await Mediator!.Send(new GetCheckUsfmQuery(ParentViewModel!.SelectedParatextBtProject.Id),
                    cancellationToken);
                if (!result.Success)
                {
                    Logger!.LogError(
                        $"Error checking USFM for Paratext BT project: '{ParentViewModel!.SelectedParatextBtProject.Name}': {result.Message}");
                }
                else
                {
                    var errorTitle = (result.Data.NumberOfErrors == 0)
                        ? LocalizationService!.Get("AddParatextCorpusDialog_NoErrors")
                        : LocalizationService!.Get("AddParatextCorpusDialog_ErrorCount");

                    usfmErrors.Add(new UsfmErrorsWrapper
                    {
                        ErrorTitle = errorTitle,
                        ProjectId = ParentViewModel.SelectedParatextBtProject.Id!,
                        ProjectName = ParentViewModel!.SelectedParatextBtProject!.LongName!,
                        UsfmErrors = new ObservableCollection<UsfmError>(result.Data!.UsfmErrors)
                    });
                }
            }

            if (ParentViewModel!.SelectedParatextLwcProject != null)
            {
                var result = await Mediator!.Send(new GetCheckUsfmQuery(ParentViewModel!.SelectedParatextLwcProject.Id),
                    cancellationToken);
                if (!result.Success)
                {
                    Logger!.LogError(
                        $"Error checking USFM for Paratext LWC project: '{ParentViewModel!.SelectedParatextLwcProject.Name}': {result.Message}");
                }
                else
                {
                    var errorTitle = (result.Data.NumberOfErrors == 0)
                        ? LocalizationService!.Get("AddParatextCorpusDialog_NoErrors")
                        : LocalizationService!.Get("AddParatextCorpusDialog_ErrorCount");

                    usfmErrors.Add(new UsfmErrorsWrapper
                    {
                        ErrorTitle = errorTitle,
                        ProjectId = ParentViewModel.SelectedParatextLwcProject.Id!,
                        ProjectName = ParentViewModel!.SelectedParatextLwcProject!.LongName!,
                        UsfmErrors = new ObservableCollection<UsfmError>(result.Data!.UsfmErrors)
                    });
                }
            }

            return usfmErrors;
        }

        //public ObservableCollection<UsfmError> UsfmErrors { get; }

        private List<UsfmErrorsWrapper> _usfmErrorsByProject;
        public List<UsfmErrorsWrapper> UsfmErrorsByProject
        {
            get => _usfmErrorsByProject;
            set => Set(ref _usfmErrorsByProject, value);
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await Initialize(cancellationToken);
            await base.OnActivateAsync(cancellationToken);
        }

        private bool _continueEnabled;
        public bool ContinueEnabled
        {
            get => _continueEnabled;
            set => Set(ref _continueEnabled, value);
        }

        private Visibility _progressIndicatorVisibility = Visibility.Hidden;
    
        public Visibility ProgressIndicatorVisibility
        {
            get => _progressIndicatorVisibility;
            set => Set(ref _progressIndicatorVisibility, value);
        }


        public override async Task Initialize(CancellationToken cancellationToken)
        {
            ContinueEnabled = false;
            UsfmErrorsByProject = new List<UsfmErrorsWrapper>();

            ProgressIndicatorVisibility = Visibility.Visible;

            DisplayName = string.Format(LocalizationService!["ProjectPicker_ProjectTemplateWizardTemplate"], ParentViewModel!.ProjectName);

            SelectedBookManager = ParentViewModel!.SelectedBookManager;

            UsfmErrorsByProject = await PerformUsfmErrorCheck(cancellationToken);

            //var usfmErrors = UsfmErrorsByProject.ToDictionary(e => e.ProjectId, e => e.UsfmErrors.AsEnumerable());

            //await SelectedBookManager!.InitializeBooks(usfmErrors, false, true, cancellationToken);

            ContinueEnabled = true;

            ProgressIndicatorVisibility = Visibility.Hidden;

            await base.Initialize(cancellationToken);

        }



        public string GetUsfmErrorsFileName()
        {
            return $"{ParentViewModel.ProjectName}-USFM-Errors.txt";
        }
        public string GetFormattedUsfmErrors()
        {
            var sb = new StringBuilder();

            foreach (var u in UsfmErrorsByProject)
            {
                if (u.UsfmErrors.Count > 0)
                {
                    sb.AppendLine($"Project Name: {u.ProjectName}, number of errors: {u.UsfmErrors.Count} ");

                    foreach (var error in u.UsfmErrors)
                    {
                        sb.AppendLine($"\t{error.Reference}\t{error.Error}");
                    }

                    sb.AppendLine();
                }

            }

            return sb.ToString();
        }
    }


}
