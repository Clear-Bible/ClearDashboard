using System;
using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using LicenseUser = ClearDashboard.Wpf.Models.LicenseUser;

namespace ClearDashboard.Wpf.Validators
{
    public class LicenseUserValidator : AbstractValidator<LicenseUser>
    {
        public LicenseUserValidator(ILogger<LicenseUserValidator> logger)
        {
            RuleFor(x => x.FirstName).Custom((firstName, context) => {

                if (string.IsNullOrEmpty(firstName))
                {
                    context.AddFailure($"First name missing."); ;
                }
            });

            RuleFor(x => x.LastName).Custom((lastName, context) => {

                if (string.IsNullOrEmpty(lastName))
                {
                    context.AddFailure($"Last name missing."); ;
                }
            });

            RuleFor(x => x.LicenseKey).Custom((licenseKey, context) => {

                if (string.IsNullOrEmpty(licenseKey))
                {
                    context.AddFailure($"License key missing."); ;
                }
            });
        }

    }
}
