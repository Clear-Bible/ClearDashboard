using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Application.Validators
{
    public class NameValidator : ValidationRule
    {
        public override ValidationResult Validate(object? value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new ValidationResult(false, "Value cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(false, "Value cannot be empty.");
            }

            try
            {
                var isMatch = Regex.IsMatch(value.ToString(), @"^([a-zA-Z0-9\s\._-]+)$");
                if (isMatch)
                {
                    return ValidationResult.ValidResult;
                }
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Enter in a valid name.");
            }
            return new ValidationResult(false, "Enter in a valid name.");
        }

    }
}
