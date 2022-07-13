using Caliburn.Micro;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace ClearDashboard.Wpf.ViewModels
{
    public abstract class ValidatingApplicationScreen<TEntity> : ApplicationScreen, IDataErrorInfo
    {
        private ValidationResult _validationResult;
        public ValidationResult ValidationResult
        {
            get => _validationResult;
            set => Set(ref _validationResult, value);
        }


        //public abstract string Error { get; }
        //public abstract string this[string columnName] { get; }

        public  string Error
        {
            get
            {

                ValidationResult = Validate();
                if (ValidationResult != null && ValidationResult.Errors.Any())
                {
                    var errors = string.Join(Environment.NewLine, ValidationResult.Errors.Select(x => x.ErrorMessage).ToArray());
                    return errors;
                }
                return string.Empty;
            }
        }

        public  string this[string columnName]
        {
            get
            {
                var emptyString = string.Empty;

                ValidationResult = Validate();

                if (ValidationResult != null)
                {
                    var firstOrDefault = ValidationResult.Errors
                        .FirstOrDefault(lol => lol.PropertyName == columnName);
                    if (firstOrDefault != null)
                        return Validator != null ? firstOrDefault.ErrorMessage : emptyString;
                }
               
                //if (!string.IsNullOrEmpty(ProjectName))
                //{
                //    var firstOrDefault = Validator.Validate(Project).Errors
                //        .FirstOrDefault(lol => lol.PropertyName == columnName);
                //    if (firstOrDefault != null)
                //        return Validator != null ? firstOrDefault.ErrorMessage : emptyString;
                //}
                return emptyString;



            }
        }

        protected abstract ValidationResult Validate();

        public IValidator<TEntity> Validator { get; protected set; }


        protected ValidatingApplicationScreen(INavigationService navigationService, ILogger logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator, IValidator<TEntity> validator):
            base(navigationService, logger, projectManager, eventAggregator)
        {
            Validator = validator;
        }

        protected ValidatingApplicationScreen()
        {

        }
    }
    public abstract class ApplicationScreen : Screen, IDisposable
    {
        public ILogger Logger { get; private set; }
        public INavigationService NavigationService { get; private set; }
        public DashboardProjectManager ProjectManager { get; private set; }
        public IEventAggregator EventAggregator { get; private set; }
        
        private bool isBusy_;
        public bool IsBusy
        {
            get => isBusy_;
            set => Set(ref isBusy_, value, nameof(IsBusy));
        }

        private FlowDirection _windowFlowDirection = FlowDirection.LeftToRight;
        public FlowDirection WindowFlowDirection
        {
            get => _windowFlowDirection;
            set => Set(ref _windowFlowDirection, value, nameof(WindowFlowDirection));
        }

        private string _title;
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        protected ApplicationScreen()
        {
            
        }

        protected ApplicationScreen(INavigationService navigationService, ILogger logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator)
        {
            NavigationService = navigationService;
            Logger = logger;
            ProjectManager = projectManager;
            EventAggregator = eventAggregator;
            WindowFlowDirection = ProjectManager.CurrentLanguageFlowDirection;

        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            EventAggregator.SubscribeOnUIThread(this);
            return base.OnActivateAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            EventAggregator.Unsubscribe(this);
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose of unmanaged resources here...
            }
        }



        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        public Task<TResponse> ExecuteRequest<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            IsBusy = true;
            try
            {
                return ProjectManager.ExecuteRequest(request, cancellationToken);
            }
            finally
            {
                IsBusy = false;
            }
        }

     
    }
}
