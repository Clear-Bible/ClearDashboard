using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
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

            try
            {
                MailAddress mail = new MailAddress(value.ToString()!.Trim());
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Enter in a Valid email address.");
            }
            return ValidationResult.ValidResult;
        }

    }
}
