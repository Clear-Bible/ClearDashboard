using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;

namespace ViewModels.Popups
{
    internal class NewProjectPopupViewModel : ApplicationScreen, IDataErrorInfo
    {
        private readonly IValidator<Project> _projectValidator;

        public NewProjectPopupViewModel()
        {
            // used by Caliburn Micro for design time    
        }

        public NewProjectPopupViewModel(IValidator<Project> projectValidator, INavigationService navigationService,
            ILogger<WorkSpaceViewModel> logger,
            DashboardProjectManager projectManager, IEventAggregator eventAggregator)
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            _projectValidator = projectValidator;
            if (!ProjectManager.HasDashboardProject)
            {
                ProjectManager.CreateDashboardProject();
            }

            Title = "Create New Project";
        }

        private string _projectName;
        private bool _canCreate;
        private ValidationResult _validationResult;

        public string ProjectName
        {
            get => _projectName;
            set
            {
                Set(ref _projectName, value);
                ProjectManager.CurrentDashboardProject.ProjectName = value;
                var validationResult = _projectValidator.Validate(new Project {ProjectName = value});
                CanCreate = validationResult.IsValid;
                
            }
        }

        //public void ValidateProjectName(string projectName)
        //{
        //    var validationResult = _projectValidator.Validate(new Project { ProjectName = projectName });
        //    CanCreate = validationResult.IsValid;

        //    if (!validationResult.IsValid)
        //    {
        //        RaiseErrorsChanged(new DataErrorsChangedEventArgs(nameof(ProjectName)));
        //    }
        //}

        public bool CanCancel => true /* can always cancel */;

        public async void Cancel()
        {
            await TryCloseAsync(false);
        }


        public bool CanCreate
        {
            get => _canCreate;
            set => Set(ref _canCreate , value);
        }
    

        public async void Create()
        {
           await TryCloseAsync(true);
        }

        //public IEnumerable GetErrors(string propertyName)
        //{
        //    return _validationResult.Errors.Where(error => error.PropertyName == propertyName)
        //        .Select(error => error.ErrorMessage);
        //}

        //public bool HasErrors => ValidationResult is { IsValid: false };

        public ValidationResult ValidationResult
        {
            get => _validationResult;
            set => Set(ref _validationResult , value);
        }

        //public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        //private void RaiseErrorsChanged(DataErrorsChangedEventArgs args)
        //{
        //    ErrorsChanged?.Invoke(this, args);
        //}

        public string Error
        {
            get
            {
                var results = _projectValidator?.Validate(new Project { ProjectName = ProjectName }); ;
                if (results != null && results.Errors.Any())
                {
                    var errors = string.Join(Environment.NewLine, results.Errors.Select(x => x.ErrorMessage).ToArray());
                    return errors;
                }
                return string.Empty;
            }
        }

        public string this[string columnName]
        {
            get
            {
                var emptyString = string.Empty;
                if (!string.IsNullOrEmpty(ProjectName))
                {
                    var firstOrDefault = _projectValidator.Validate(new Project { ProjectName = ProjectName }).Errors
                        .FirstOrDefault(lol => lol.PropertyName == columnName);
                    if (firstOrDefault != null)
                        return _projectValidator != null ? firstOrDefault.ErrorMessage : emptyString;
                }
                return emptyString;
                

               
            }
        }
    }
}
