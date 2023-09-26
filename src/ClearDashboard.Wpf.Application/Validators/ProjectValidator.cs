using ClearDashboard.Wpf.Application.Helpers;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.RegularExpressions;
using ClearDashboard.Wpf.Application.Services;

namespace ClearDashboard.Wpf.Application.Validators
{
    public class ProjectValidator : AbstractValidator<DataAccessLayer.Models.Project>
    {
        private ILogger? _logger;
        private ILocalizationService? _localizationService;
        public ProjectValidator(ILogger<ProjectValidator> logger, ILocalizationService? localizationService)
        {
            _localizationService = localizationService;
            RuleFor(x => x.ProjectName).Custom((projectName, context) => {

                if (string.IsNullOrEmpty(projectName))
                {
                    return;
                }

                var foundMatch = false;
                try
                {
                    foundMatch = Regex.IsMatch(projectName, @"^([a-zA-Z0-9\s\._-]+)$");
                }
                catch (ArgumentException ex)
                {
                    context.AddFailure($"Error {ex.Message}");
                }

                if (!foundMatch)
                {
                    //context.AddFailure($"The project name '{projectName}' contains illegal characters.  Valid characters include 'A-Z' (lowercase and uppercase), numbers '0-9' and the characters '-' and '_'.");
                    context.AddFailure(_localizationService.Get("ProjectValidator_IllegalCharacters"));
                }

                // check to see if the project directory already exists:
             
                var projectDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ClearDashboard_Projects", projectName);

                if (Directory.Exists(projectDirectory))
                {
                    var test = _localizationService.Get("Landing_NewProject");
                    //context.AddFailure($"A project with the name '{projectName}' already exists. Please choose a unique name.");
                    context.AddFailure(_localizationService.Get("ProjectValidator_SameName"));
                }

                var maximumProjectNameLength = 98;
                if (projectName.Length >= maximumProjectNameLength)
                {
                    context.AddFailure(_localizationService.Get("ProjectValidator_TooLong"));
                }
            });
        }

    }
}
