using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using SIL.Scripture;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup.ProjectTemplate
{
    public class ProjectSelectionStepViewModel : DashboardApplicationValidatingWorkflowStepViewModel<StartupDialogViewModel, ProjectSelectionStepViewModel>
    {
        private List<ParatextProjectMetadata>? _projects;
        public List<ParatextProjectMetadata>? Projects
        {
            get => _projects;
            set => Set(ref _projects, value);
        }

        private ParatextProjectMetadata? _selectedParatextProject;
        public ParatextProjectMetadata? SelectedParatextProject
        {
            get => _selectedParatextProject;
            set => Set(ref _selectedParatextProject, value);
        }

        private ParatextProjectMetadata? _selectedParatextBtProject;
        public ParatextProjectMetadata? SelectedParatextBtProject
        {
            get => _selectedParatextBtProject;
            set => Set(ref _selectedParatextBtProject, value);
        }

        private ParatextProjectMetadata? _selectedParatextLwcProject;
        public ParatextProjectMetadata? SelectedParatextLwcProject
        {
            get => _selectedParatextLwcProject;
            set => Set(ref _selectedParatextLwcProject, value);
        }

        private bool _showBiblicalTexts = true;
        public bool ShowBiblicalTexts
        {
            get => _showBiblicalTexts;
            set => Set(ref _showBiblicalTexts, value);
        }

        private bool _isEnabledSelectedParatextProject = true;
        public bool IsEnabledSelectedParatextProject
        {
            get => _isEnabledSelectedParatextProject;
            set => Set(ref _isEnabledSelectedParatextProject, value);
        }

        private bool _isEnabledSelectedParatextBtProject = true;
        public bool IsEnabledSelectedParatextBtProject
        {
            get => _isEnabledSelectedParatextBtProject;
            set => Set(ref _isEnabledSelectedParatextBtProject, value);
        }

        private bool _isEnabledSelectedParatextLwcProject = true;
        public bool IsEnabledSelectedParatextLwcProject
        {
            get => _isEnabledSelectedParatextLwcProject;
            set => Set(ref _isEnabledSelectedParatextLwcProject, value);
        }

        private DataAccessLayer.Models.Project _project;
        public DataAccessLayer.Models.Project Project
        {
            get => _project;
            private init => Set(ref _project, value);
        }

        private string _projectName;
        public string ProjectName
        {
            get => _projectName;
            set
            {
                Set(ref _projectName, value.Replace(' ', '_'));
                ProjectManager!.CurrentDashboardProject.ProjectName = _projectName;
                Project.ProjectName = _projectName;
                ValidationResult = Validator!.Validate(this);
                CanCreate = !string.IsNullOrEmpty(_projectName) && ValidationResult.IsValid;
                NotifyOfPropertyChange(nameof(Project));
            }
        }

        private bool _canCreate;
        public bool CanCreate
        {
            get => _canCreate;
            set => Set(ref _canCreate, value);
        }

        public async void ParatextProjectSelected()
        {
            //CanOk = false;

            //await CheckUsfm(ParentViewModel);

            //ValidationResult = Validator?.Validate(this);
            //CanOk = ValidationResult.IsValid;

            await Task.CompletedTask;
        }

        public async void ParatextBtProjectSelected()
        {
            //CanOk = false;

            //await CheckUsfm(ParentViewModel);

            //ValidationResult = Validator?.Validate(this);
            //CanOk = ValidationResult.IsValid;

            await Task.CompletedTask;
        }

        public async void ParatextLwcProjectSelected()
        {
            //CanOk = false;

            //await CheckUsfm(ParentViewModel);

            //ValidationResult = Validator?.Validate(this);
            //CanOk = ValidationResult.IsValid;

            await Task.CompletedTask;
        }

        public ProjectSelectionStepViewModel(DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger<ProjectSetupViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, IValidator<ProjectSelectionStepViewModel> validator, 
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator, localizationService)
        {
            if (!ProjectManager!.HasDashboardProject)
            {
                ProjectManager.CreateDashboardProject();
            }

            _project = new DataAccessLayer.Models.Project();
            _projectName = string.Empty;

            CanMoveForwards = true;
            CanMoveBackwards = true;
            EnableControls = true;
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var result = await ProjectManager!.ExecuteRequest(new GetProjectMetadataQuery(), cancellationToken);
            if (result.Success)
            {
                Projects = result.Data!.OrderBy(p => p.Name).ToList();

                // send new metadata results to the Main Window    
                //await EventAggregator.PublishOnUIThreadAsync(new ProjectsMetadataChangedMessage(result.Data), cancellationToken);
            }

            await base.OnActivateAsync(cancellationToken);
        }

        public override async Task MoveBackwardsAction()
        {
            await ParentViewModel!.GoToStep(1);
        }

        public override async Task MoveForwardsAction()
        {
            ParentViewModel!.SelectedParatextProject = SelectedParatextProject;
            ParentViewModel!.SelectedParatextBtProject = SelectedParatextBtProject;
            ParentViewModel!.SelectedParatextLwcProject = SelectedParatextLwcProject;
            ParentViewModel!.ShowBiblicalTexts = ShowBiblicalTexts;

            await base.MoveForwardsAction();
        }

        protected override FluentValidation.Results.ValidationResult? Validate()
        {
            return (!string.IsNullOrEmpty(ProjectName)) ? Validator!.Validate(this) : null;
        }
    }


}
