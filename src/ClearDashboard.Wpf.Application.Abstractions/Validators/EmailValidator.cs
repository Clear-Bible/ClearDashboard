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
    public class EmailValidator : ValidationRule
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
                // from https://uibakery.io/regex-library/email-regex-csharp
                Regex validateEmailRegex = new Regex("(?:[a-z0-9!$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])");
                var isMatch = validateEmailRegex.IsMatch(value.ToString()!.Trim());
                if (isMatch)
                {
                    return ValidationResult.ValidResult;
                }
                //MailAddress mail = new MailAddress(value.ToString()!.Trim());
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Enter in a Valid email address.");
            }
            return new ValidationResult(false, "Enter in a Valid email address.");
        }

    }
}
