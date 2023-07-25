using System.ComponentModel;
using ClearApplicationFoundation.Services;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.LicenseGenerator;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Services;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.Validators
{
    public class LicenseUserValidator : AbstractValidator<DashboardUser>
    {
        private ILogger _logger;
        private ILocalizationService _localizationService;
        public LicenseUserValidator(ILogger<LicenseUserValidator> logger, ILocalizationService localizationService)
        {
            _localizationService = localizationService;
            //RuleFor(x => x.FirstName).Custom((firstName, context) => {

            //    if (string.IsNullOrEmpty(firstName))
            //    {
            //        context.AddFailure(LocalizationStrings.Get("LicenseValidator_FirstMissing",_logger)); ;
            //    }
            //});

            //RuleFor(x => x.LastName).Custom((lastName, context) => {

            //    if (string.IsNullOrEmpty(lastName))
            //    {
            //        context.AddFailure(LocalizationStrings.Get("LicenseValidator_LastMissing", _logger)); ;
            //    }
            //});

            //RuleFor(x => x.LicenseKey).Custom((licenseKey, context) => {

            //    if (string.IsNullOrEmpty(licenseKey))
            //    {
            //        context.AddFailure(LocalizationStrings.Get("LicenseValidator_LicenseMissing", _logger)); ;
            //    }
            //});

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage(_localizationService.Get("LicenseValidator_FirstMissing"));
            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage(_localizationService.Get("LicenseValidator_LastMissing"));
            RuleFor(x => x.LicenseKey)
                .NotEmpty()
                .WithMessage(_localizationService.Get("LicenseValidator_LicenseMissing"));
        }

    }
}
