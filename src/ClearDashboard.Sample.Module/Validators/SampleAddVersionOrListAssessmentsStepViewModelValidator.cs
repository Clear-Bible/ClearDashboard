using Autofac.Features.AttributeFilters;
using ClearDashboard.Sample.Module.Services;
using ClearDashboard.Sample.Module.ViewModels.SampleDialog;
using ClearDashboard.Wpf.Application.Services;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearApplicationFoundation.Services;

namespace ClearDashboard.Sample.Module.Validators
{
    internal class SampleAddVersionOrListAssessmentsStepViewModelValidator : AbstractValidator<SampleAddVersionOrListAssessmentsStepViewModel>
    {
        private readonly ILocalizationService _localizationService;

        public SampleAddVersionOrListAssessmentsStepViewModelValidator(
            ILogger<SampleAddVersionOrListAssessmentsStepViewModelValidator> logger, SampleLocalizationService localizationService)
        {
            _localizationService = localizationService;

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
        }
    }
}
