using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Application.Validators
{

    public class HtmlUrlValidator : ValidationRule
    {
        public override ValidationResult Validate(object? value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new ValidationResult(false, "Value cannot be empty.");
            }

            try
            {
                // from https://uibakery.io/regex-library/url-regex-csharp
                // Validate URL
                Regex validateUrlRegex = new Regex("^https?:\\/\\/(?:www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b(?:[-a-zA-Z0-9()@:%_\\+.~#?&\\/=]*)$");
                var isMatch = validateUrlRegex.IsMatch(value.ToString()!.Trim());
                if (isMatch)
                {
                    return ValidationResult.ValidResult;
                }
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Enter in a Valid Url address.");
            }
            return new ValidationResult(false, "Enter in a Valid Url address.");
        }

    }
}
