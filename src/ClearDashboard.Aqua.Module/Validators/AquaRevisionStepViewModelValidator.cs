using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using ClearApplicationFoundation.Services;
using ClearDashboard.Aqua.Module.Services;
using ClearDashboard.Aqua.Module.ViewModels.AquaDialog;
using ClearDashboard.Wpf.Application.Services;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Aqua.Module.Validators
{
    internal class AquaRevisionStepViewModelValidator : AbstractValidator<AquaRevisionStepViewModel>
    {
        private readonly ILocalizationService _localizationService;

        public AquaRevisionStepViewModelValidator(
            ILogger<AquaRevisionStepViewModelValidator> logger, AquaLocalizationService localizationService)
        {
            /*
            RuleFor(x => x.ValidatedText).NotNull().NotEmpty();

            RuleFor(x => x.LengthText).MinimumLength(1).MaximumLength(6);

            RuleFor(x => x.NumericText)
                .Custom((x, context) =>
            {
                if ((!(int.TryParse(x, out int value)) || value <= 0))
                {
                    var messageFormat = _localizationService["NumericTextValidationMessage"]; //" {0} must be greater than 1.";
                    context.AddFailure(string.Format(messageFormat, x));
                }
            });
            */
        }
    }
}
