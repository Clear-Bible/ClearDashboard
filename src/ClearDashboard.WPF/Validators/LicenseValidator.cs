using System;
using System.Text.RegularExpressions;
using ClearDashboard.Wpf.Helpers;
using FluentValidation;
using Microsoft.Extensions.Logging;
using LicenseUser = ClearDashboard.Wpf.Models.LicenseUser;

namespace ClearDashboard.Wpf.Validators
{
    public class LicenseUserValidator : AbstractValidator<LicenseUser>
    {
        ILogger _logger;
        public LicenseUserValidator(ILogger<LicenseUserValidator> logger)
        {
            RuleFor(x => x.FirstName).Custom((firstName, context) => {

                if (string.IsNullOrEmpty(firstName))
                {
                    context.AddFailure(LocalizationStrings.Get("LicenseValidator_firstMissing",_logger)); ;
                }
            });

            RuleFor(x => x.LastName).Custom((lastName, context) => {

                if (string.IsNullOrEmpty(lastName))
                {
                    context.AddFailure(LocalizationStrings.Get("LicenseValidator_lastMissing", _logger)); ;
                }
            });

            RuleFor(x => x.LicenseKey).Custom((licenseKey, context) => {

                if (string.IsNullOrEmpty(licenseKey))
                {
                    context.AddFailure(LocalizationStrings.Get("LicenseValidator_licenseMissing",_logger)); ;
                }
            });
        }

    }
}
