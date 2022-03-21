using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Converters
{
    public class ProjectFileNameValidationRule : ValidationRule
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public ProjectFileNameValidationRule()
        {
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string projectName = (string)value;

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
}
