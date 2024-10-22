﻿using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup.ProjectTemplate
{
    public class ProjectSelectionStepViewModel : DashboardApplicationValidatingWorkflowStepViewModel<StartupDialogViewModel, ProjectSelectionStepViewModel>
    {
        #region Member Variables

        private bool _selectionChanging = false;

        #endregion //Member Variables


        #region Public Properties

        #endregion //Public Properties


        #region Observable Properties

        private BindableCollection<ParatextProjectMetadata>? _projects;
        public BindableCollection<ParatextProjectMetadata>? Projects
        {
            get => _projects;
            set => Set(ref _projects, value);
        }

        private BindableCollection<ParatextProjectMetadata>? _paratextProjects;
        public BindableCollection<ParatextProjectMetadata>? ParatextProjects
        {
            get => _paratextProjects;
            set => Set(ref _paratextProjects, value);
        }

        private BindableCollection<ParatextProjectMetadata>? _paratextBtProjects;
        public BindableCollection<ParatextProjectMetadata>? ParatextBtProjects
        {
            get => _paratextBtProjects;
            set => Set(ref _paratextBtProjects, value);
        }

        private BindableCollection<ParatextProjectMetadata>? _paratextLwcProjects;
        public BindableCollection<ParatextProjectMetadata>? ParatextLwcProjects
        {
            get => _paratextLwcProjects;
            set => Set(ref _paratextLwcProjects, value);
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

		private bool _importWordAnalysesParatextProject = false;
		public bool ImportWordAnalysesParatextProject
		{
			get => _importWordAnalysesParatextProject;
			set => Set(ref _importWordAnalysesParatextProject, value);
		}

		private bool _importWordAnalysesParatextBtProject = false;
		public bool ImportWordAnalysesParatextBtProject
		{
			get => _importWordAnalysesParatextBtProject;
			set => Set(ref _importWordAnalysesParatextBtProject, value);
		}

		private bool _importWordAnalysesParatextLwcProject = false;
		public bool ImportWordAnalysesParatextLwcProject
		{
			get => _importWordAnalysesParatextLwcProject;
			set => Set(ref _importWordAnalysesParatextLwcProject, value);
		}

		private bool _showOtBiblicalTexts = true;
        public bool ShowOtBiblicalTexts
        {
            get => _showOtBiblicalTexts;
            set => Set(ref _showOtBiblicalTexts, value);
        }

        private bool _showNtBiblicalTexts = true;
        public bool ShowNtBiblicalTexts
        {
            get => _showNtBiblicalTexts;
            set => Set(ref _showNtBiblicalTexts, value);
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

		private bool _isEnabledImportWordAnalysesParatextProject = false;
		public bool IsEnabledImportWordAnalysesParatextProject
		{
			get => _isEnabledImportWordAnalysesParatextProject;
			set => Set(ref _isEnabledImportWordAnalysesParatextProject, value);
		}

		private bool _isEnabledImportWordAnalysesParatextBtProject = false;
		public bool IsEnabledImportWordAnalysesParatextBtProject
		{
			get => _isEnabledImportWordAnalysesParatextBtProject;
			set => Set(ref _isEnabledImportWordAnalysesParatextBtProject, value);
		}

		private bool _isEnabledImportWordAnalysesParatextLwcProject = false;
		public bool IsEnabledImportWordAnalysesParatextLwcProject
		{
			get => _isEnabledImportWordAnalysesParatextLwcProject;
			set => Set(ref _isEnabledImportWordAnalysesParatextLwcProject, value);
		}


		private string? _projectName;
        public string? ProjectName
        {
            get => _projectName;
            set
            {
                Set(ref _projectName, value.Replace(' ', '_'));
                ValidationResult = Validator!.Validate(this);
                CanMoveForwards = (SelectedParatextProject != null && !string.IsNullOrEmpty(ProjectName) && ValidationResult.IsValid);
            }
        }

        #endregion //Observable Properties


        #region Constructor

        public ProjectSelectionStepViewModel(DashboardProjectManager projectManager,
            INavigationService navigationService, ILogger<ProjectSetupViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, IValidator<ProjectSelectionStepViewModel> validator,
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, validator, localizationService)
        {
            ProjectName = string.Empty;
            CanMoveForwards = true;
            CanMoveBackwards = true;
            EnableControls = true;
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            ProjectName = ParentViewModel.ProjectName ?? string.Empty; 

            await Initialize(cancellationToken);
            await base.OnActivateAsync(cancellationToken);
        }

        #endregion //Constructor


        #region Methods

        private BindableCollection<ParatextProjectMetadata>? GetSelectableParatextProjects()
        {
            if (Projects != null)
            {
                var otherSelectedIds = new List<string?>
                    {
                        SelectedParatextBtProject?.Id,
                        SelectedParatextLwcProject?.Id
                    }
                    .Where(p => p != null);

                return new(Projects.Where(project => project.CorpusType != CorpusType.BackTranslation && !otherSelectedIds.Contains(project.Id)));
            }
            return null;
        }

        private BindableCollection<ParatextProjectMetadata>? GetSelectableParatextBtProjects()
        {
            if (Projects != null)
            {
                var otherSelectedIds = new List<string?>
                    {
                        SelectedParatextProject?.Id,
                        SelectedParatextLwcProject?.Id
                    }
                    .Where(p => p != null);

                return new(Projects.Where(project => project.CorpusType == CorpusType.BackTranslation && !otherSelectedIds.Contains(project.Id)));
            }
            return null;
        }

        private BindableCollection<ParatextProjectMetadata>? GetSelectableParatextLwcProjects()
        {
            if (Projects != null)
            {
                var otherSelectedIds = new List<string?>
                    {
                        SelectedParatextProject?.Id,
                        SelectedParatextBtProject?.Id
                    }
                    .Where(p => p != null);

                return new(Projects.Where(project => project.CorpusType != CorpusType.BackTranslation && !otherSelectedIds.Contains(project.Id)));
            }
            return null;
        }

        public async void ParatextProjectSelected(SelectionChangedEventArgs args)
        {
            if (_selectionChanging) return;

            if (Projects != null)
            {
                try
                {
                    _selectionChanging = true;
                    ParatextBtProjects = GetSelectableParatextBtProjects();
                    ParatextLwcProjects = GetSelectableParatextLwcProjects();
                }
                finally
                {
                    _selectionChanging = false;
                }
            }

            ValidationResult = Validator!.Validate(this);
            CanMoveForwards = (SelectedParatextProject != null && !string.IsNullOrEmpty(ProjectName) && ValidationResult.IsValid);

			IsEnabledImportWordAnalysesParatextProject = SelectedParatextProject?.CorpusType == CorpusType.Standard || SelectedParatextProject?.CorpusType == CorpusType.BackTranslation;

            // Instead of presenting a checkbox to the user so that they can choose whether or not to
            // import word analyses for the selected corpus (just after tokenization), the current rule
            // is to always do the import, if allowable for the given CorpusType
            ImportWordAnalysesParatextProject = IsEnabledImportWordAnalysesParatextProject;

			if (SelectedParatextBtProject == null)
            {
                // FIXME:  Dirk knows some way to get the back translation for a given paratext project
                // so we can in theory prefill SelectedParatextBtProject value
            }

            if (SelectedParatextLwcProject == null)
            {
                // FIXME:  Dirk knows some way to get the lwc translation for a given paratext project
                // so we can in theory prefill SelectedParatextLwcProject value
            }

            await Task.CompletedTask;
        }

        public async void ParatextBtProjectSelected(SelectionChangedEventArgs args)
        {
            if (_selectionChanging) return;

            if (Projects != null)
            {
                try
                {
                    _selectionChanging = true;

                    ParatextProjects = GetSelectableParatextProjects();
                    ParatextLwcProjects = GetSelectableParatextLwcProjects();
                }
                finally
                {
                    _selectionChanging = false;
                }
            }

			IsEnabledImportWordAnalysesParatextBtProject = SelectedParatextBtProject?.CorpusType == CorpusType.Standard || SelectedParatextBtProject?.CorpusType == CorpusType.BackTranslation;

			// Instead of presenting a checkbox to the user so that they can choose whether or not to
			// import word analyses for the selected corpus (just after tokenization), the current rule
			// is to always do the import, if allowable for the given CorpusType
			ImportWordAnalysesParatextBtProject = IsEnabledImportWordAnalysesParatextBtProject;

			await Task.CompletedTask;
        }

        public async void ParatextLwcProjectSelected(SelectionChangedEventArgs args)
        {
            if (_selectionChanging) return;

            if (Projects != null)
            {
                try
                {
                    _selectionChanging = true;
                    
                    ParatextProjects = GetSelectableParatextProjects();
                    ParatextBtProjects = GetSelectableParatextBtProjects();
                }
                finally
                {
                    _selectionChanging = false;
                }
            }

			IsEnabledImportWordAnalysesParatextLwcProject = SelectedParatextLwcProject?.CorpusType == CorpusType.Standard || SelectedParatextLwcProject?.CorpusType == CorpusType.BackTranslation;

			// Instead of presenting a checkbox to the user so that they can choose whether or not to
			// import word analyses for the selected corpus (just after tokenization), the current rule
			// is to always do the import, if allowable for the given CorpusType
			ImportWordAnalysesParatextLwcProject = IsEnabledImportWordAnalysesParatextLwcProject;

			await Task.CompletedTask;
        }

        public void ClearSelectedParatextProject()
        {
            SelectedParatextProject = null;
            ImportWordAnalysesParatextProject = false;
            IsEnabledImportWordAnalysesParatextProject = false;
        }

        public void ClearSelectedParatextBtProject()
        {
            SelectedParatextBtProject = null;
            ImportWordAnalysesParatextBtProject = false;
            IsEnabledImportWordAnalysesParatextBtProject = false;
        }

        public void ClearSelectedParatextLwcProject()
        {
            SelectedParatextLwcProject = null;
            ImportWordAnalysesParatextLwcProject = false;
            IsEnabledImportWordAnalysesParatextLwcProject = false;
        }

        public override async Task Reset(CancellationToken cancellationToken)
        {
            await Initialize(cancellationToken);
            await base.Reset(cancellationToken);
        }

        public override async Task Initialize(CancellationToken cancellationToken)
        {
            if (Projects == null)
            {
                var result = await ProjectManager!.ExecuteRequest(new GetProjectMetadataQuery(), cancellationToken);
                if (result.Success)
                {
                    Projects = new(result.Data!.OrderBy(p => p.Name).ToList());
                }
            }

            if (Projects != null)
            {
                var backTranslationProjects = Projects.Where(project => project.CorpusType == CorpusType.BackTranslation);
                var otherProjects = Projects.Where(project => project.CorpusType != CorpusType.BackTranslation);
                ParatextProjects = new(otherProjects);
                ParatextBtProjects = new(backTranslationProjects);
                ParatextLwcProjects = new(otherProjects);
            }

            DisplayName = string.Format(LocalizationService!["ProjectPicker_ProjectTemplateWizardTemplate"], ProjectName);

            SelectedParatextProject = ParentViewModel!.SelectedParatextProject;
            SelectedParatextBtProject = ParentViewModel!.SelectedParatextBtProject;
            SelectedParatextLwcProject = ParentViewModel!.SelectedParatextLwcProject;

			ImportWordAnalysesParatextProject = ParentViewModel!.ImportWordAnalysesParatextProject;
			ImportWordAnalysesParatextBtProject = ParentViewModel!.ImportWordAnalysesParatextBtProject;
			ImportWordAnalysesParatextLwcProject = ParentViewModel!.ImportWordAnalysesParatextLwcProject;

            ShowOtBiblicalTexts = ParentViewModel!.IncludeOtBiblicalTexts;
            ShowNtBiblicalTexts = ParentViewModel!.IncludeNtBiblicalTexts;

            ValidationResult = await Validator!.ValidateAsync(this, cancellationToken);
            CanMoveForwards = (SelectedParatextProject != null && !string.IsNullOrEmpty(ProjectName) && ValidationResult.IsValid);
            await base.Initialize(cancellationToken);
        }

        public async Task CreateAsync()
        {

            var currentlyOpenProjectsList = OpenProjectManager.DeserializeOpenProjectList();
            if (currentlyOpenProjectsList.Contains(ProjectName))
            {
                return;
            }
            

            OpenProjectManager.AddProjectToOpenProjectList(ProjectManager!);


        }

        public override async Task MoveBackwardsAction()
        {
            ProjectName = string.Empty;
            ParentViewModel.ProjectName = ProjectName;
            ParentViewModel!.Reset();
            await ParentViewModel!.GoToStep(1);
        }

        public override async Task MoveForwardsAction()
        {
            await CreateAsync();

            ParentViewModel!.SelectedParatextProject = SelectedParatextProject;
            ParentViewModel!.SelectedParatextBtProject = SelectedParatextBtProject;
            ParentViewModel!.SelectedParatextLwcProject = SelectedParatextLwcProject;

			ParentViewModel!.ImportWordAnalysesParatextProject = ImportWordAnalysesParatextProject;
			ParentViewModel!.ImportWordAnalysesParatextBtProject = ImportWordAnalysesParatextBtProject;
			ParentViewModel!.ImportWordAnalysesParatextLwcProject = ImportWordAnalysesParatextLwcProject;

            ParentViewModel!.IncludeOtBiblicalTexts = ShowOtBiblicalTexts;
            ParentViewModel!.IncludeNtBiblicalTexts = ShowNtBiblicalTexts;
            ParentViewModel!.ProjectName = ProjectName;

            ParentViewModel!.SelectedBookIds = null;
            
            await base.MoveForwardsAction();
        }

        protected override FluentValidation.Results.ValidationResult? Validate()
        {
            return (!string.IsNullOrEmpty(ProjectName)) ? Validator!.Validate(this) : null;
        }

        #endregion // Methods
    }


}
