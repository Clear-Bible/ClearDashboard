using System;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels;

public abstract class ValidatingApplicationScreen<TEntity> : ApplicationScreen, IDataErrorInfo
{
    private ValidationResult _validationResult;
    public ValidationResult ValidationResult
    {
        get => _validationResult;
        set => Set(ref _validationResult, value);
    }

    protected ValidatingApplicationScreen(INavigationService navigationService, ILogger logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator, IValidator<TEntity> validator) :
        base(navigationService, logger, projectManager, eventAggregator)
    {
        Validator = validator;
    }

    protected ValidatingApplicationScreen()
    {

    }


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
                    .FirstOrDefault(validationFailure => validationFailure.PropertyName == columnName);
                if (firstOrDefault != null)
                {
                    return Validator != null ? firstOrDefault.ErrorMessage : emptyString;
                }
            }
               
            return emptyString;

        }
    }

    protected abstract ValidationResult Validate();

    public IValidator<TEntity> Validator { get; protected set; }


      
}