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
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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

        private Visibility _progressIndicatorVisibility = Visibility.Hidden;
        public Visibility ProgressIndicatorVisibility
        {
            get => _progressIndicatorVisibility;
            set => Set(ref _progressIndicatorVisibility, value);
        }

        public ScopeSelectionStepViewModel(DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger<ProjectSetupViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            CanMoveForwards = true;
            CanMoveBackwards = true;
            EnableControls = true;
        }

        private void OnSelectedBookManagerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            ContinueEnabled = SelectedBookManager.SelectedBooks.Any(book=>book.IsSelected);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            SelectedBookManager.PropertyChanged -= OnSelectedBookManagerPropertyChanged;
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        public override async Task Initialize(CancellationToken cancellationToken)
        {
            SelectedBookManager = ParentViewModel!.SelectedBookManager;
           
            SelectedBookManager.PropertyChanged += OnSelectedBookManagerPropertyChanged;

            DisplayName = string.Format(LocalizationService!["ProjectPicker_ProjectTemplateWizardTemplate"], ParentViewModel!.ProjectName);

            if (ParentViewModel!.SelectedBookIds is not null)
            {
                ContinueEnabled = true;
                await base.OnActivateAsync(cancellationToken);
                return;
            }

            ProgressIndicatorVisibility = Visibility.Visible;

            var usfmErrorsByParatextProjectId = new Dictionary<string, IEnumerable<UsfmError>>();

            if (ParentViewModel!.SelectedParatextProject != null)
            {
                var result = await Mediator!.Send(new GetCheckUsfmQuery(ParentViewModel!.SelectedParatextProject.Id), cancellationToken);
                if (!result.Success)
                {
                    Logger!.LogError($"Error checking USFM for Paratext project: '{ParentViewModel!.SelectedParatextProject.Name}': {result.Message}");
                }

                usfmErrorsByParatextProjectId.Add(ParentViewModel!.SelectedParatextProject.Id!, result.Data!.UsfmErrors);
            }

            if (ParentViewModel!.SelectedParatextBtProject != null)
            {
                var result = await Mediator!.Send(new GetCheckUsfmQuery(ParentViewModel!.SelectedParatextBtProject.Id), cancellationToken);
                if (!result.Success)
                {
                    Logger!.LogError($"Error checking USFM for Paratext BT project: '{ParentViewModel!.SelectedParatextBtProject.Name}': {result.Message}");
                }

                usfmErrorsByParatextProjectId.Add(ParentViewModel!.SelectedParatextBtProject.Id!, result.Data!.UsfmErrors);
            }

            if (ParentViewModel!.SelectedParatextLwcProject != null)
            {
                var result = await Mediator!.Send(new GetCheckUsfmQuery(ParentViewModel!.SelectedParatextLwcProject.Id), cancellationToken);
                if (!result.Success)
                {
                    Logger!.LogError($"Error checking USFM for Paratext LWC project: '{ParentViewModel!.SelectedParatextLwcProject.Name}': {result.Message}");
                }

                usfmErrorsByParatextProjectId.Add(ParentViewModel!.SelectedParatextLwcProject.Id!, result.Data!.UsfmErrors);
            }

            await SelectedBookManager!.InitializeBooks(usfmErrorsByParatextProjectId, false, cancellationToken);




            ProgressIndicatorVisibility = Visibility.Hidden;
            await base.Initialize(cancellationToken);
        }

        public override async Task Reset(CancellationToken cancellationToken)
        {
            await Initialize(cancellationToken);
            await base.Reset(cancellationToken);
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await Initialize(cancellationToken);
            await base.OnActivateAsync(cancellationToken);
        }

        public override async Task MoveForwardsAction()
        {
            ParentViewModel!.SelectedBookIds = SelectedBookManager.SelectedAndEnabledBookAbbreviations;
            await ParentViewModel!.GoToStep(5);
        }

        // ReSharper disable once UnusedMember.Global
        public void UnselectAllBooks()
        {
            SelectedBookManager.UnselectAllBooks();
        }

        // ReSharper disable once UnusedMember.Global
        public void SelectAllBooks()
        {
            SelectedBookManager.SelectAllBooks();
        }

        // ReSharper disable once UnusedMember.Global
        public void SelectNewTestamentBooks()
        {
            SelectedBookManager.SelectNewTestamentBooks();
        }

        // ReSharper disable once UnusedMember.Global
        public void SelectOldTestamentBooks()
        {
            SelectedBookManager.SelectOldTestamentBooks();
        }
    }
}
