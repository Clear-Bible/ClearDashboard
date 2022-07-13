using System;
using System.IO;
using System.Text.RegularExpressions;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Helpers;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Validators
{
    public class ProjectValidator : AbstractValidator<Project>
    {
        public ProjectValidator(ILogger<ProjectValidator> logger)
        {
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
                    context.AddFailure($"The project name '{projectName}' contains illegal characters.  Valid characters include 'A-Z' (lowercase and uppercase), numbers '0-9' and the characters '-' and '_'.");
                }

                // check to see if the project directory already exists:
             
                var projectDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ClearDashboard_Projects", projectName);

                if (Directory.Exists(projectDirectory))
                {
                    var test = LocalizationStrings.Get("Landing_newproject", logger);
                    context.AddFailure($"A project with the name '{projectName}' already exists. Please choose a unique name.");
                }
            });


        }

    }
}
