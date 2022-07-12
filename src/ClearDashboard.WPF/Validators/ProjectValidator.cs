using ClearDashboard.DataAccessLayer.Models;
using FluentValidation;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Validators
{
    public class ProjectValidator : AbstractValidator<Project>
    {
        public ProjectValidator()
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
                    context.AddFailure($"The project directory {projectDirectory} already exists.");
                }
            });


        }

    }
}
