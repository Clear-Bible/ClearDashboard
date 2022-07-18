using System;
using System.IO;
using System.Text.RegularExpressions;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Helpers;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Models;

namespace ClearDashboard.Wpf.Validators
{
    public class LicenseValidator : AbstractValidator<LicenseUser>
    {
        public LicenseValidator(ILogger<LicenseValidator> logger)
        {
            RuleFor(x => x.LicenseKey).Custom((licenseKey, context) => {

                if (string.IsNullOrEmpty(licenseKey))
                {
                    return;
                }

                var foundMatch = false;
                try
                {
                    foundMatch = Regex.IsMatch(licenseKey, @"^([a-zA-Z0-9\s\-]+)$");
                }
                catch (ArgumentException ex)
                {
                    context.AddFailure($"Error {ex.Message}");
                }

                if (!foundMatch)
                {
                    context.AddFailure($"The license key '{licenseKey}' contains illegal characters.  Valid characters include 'A-Z' (lowercase and uppercase), numbers '0-9' and '-'.");
                }

                //// check to see if the project directory already exists:
             
                //var projectDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ClearDashboard_Projects", projectName);

                //if (Directory.Exists(projectDirectory))
                //{
                //    var test = LocalizationStrings.Get("Landing_newproject", logger);
                //    context.AddFailure($"A project with the name '{projectName}' already exists. Please choose a unique name.");
                //}
            });


        }

    }
}
