using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;

namespace Converters
{

    public static class ProjectNameValidator
    {
        public static ValidationResult ValidProjectName(string projectName)
        {
            bool foundMatch = false;
            try
            {
                foundMatch = Regex.IsMatch(projectName, @"^([a-zA-Z0-9\s\._-]+)$");
            }
            catch (ArgumentException ex)
            {
                return new ValidationResult(false, $"Error {ex.Message}");
            }

            if (!foundMatch)
            {
                return new ValidationResult(false, "Illegal Characters in Project Name");
            }

            // check to see if the project directory already exists:
            // get the projects directory
            string appPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            appPath = Path.Combine(appPath, "ClearDashboard_Projects");

            //check to see if the directory exists already
            string newDir = Path.Combine(appPath, projectName);

            if (Directory.Exists(newDir))
            {
                return new ValidationResult(false, "Directory Exists");
            }

            return ValidationResult.ValidResult;
        }
    }
    public class ProjectFileNameValidationRule : ValidationRule
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var projectName = (string)value;
            return ProjectNameValidator.ValidProjectName(projectName);
        }
    }
}
