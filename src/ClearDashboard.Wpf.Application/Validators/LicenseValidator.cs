using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Helpers;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.Validators
{
    public class LicenseUserValidator : AbstractValidator<LicenseUser>
    {
        ILogger _logger;
        public LicenseUserValidator(ILogger<LicenseUserValidator> logger)
        {
            RuleFor(x => x.FirstName).Custom((firstName, context) => {

                if (string.IsNullOrEmpty(firstName))
                {
                    context.AddFailure(LocalizationStrings.Get("LicenseValidator_FirstMissing",_logger)); ;
                }
            });

            RuleFor(x => x.LastName).Custom((lastName, context) => {

                if (string.IsNullOrEmpty(lastName))
                {
                    context.AddFailure(LocalizationStrings.Get("LicenseValidator_LastMissing", _logger)); ;
                }
            });

            RuleFor(x => x.LicenseKey).Custom((licenseKey, context) => {

                if (string.IsNullOrEmpty(licenseKey))
                {
                    context.AddFailure(LocalizationStrings.Get("LicenseValidator_LicenseMissing",_logger)); ;
                }
            });
        }

    }
}
