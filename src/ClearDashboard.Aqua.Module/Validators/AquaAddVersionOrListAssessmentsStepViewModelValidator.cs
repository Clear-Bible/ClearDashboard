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
    internal class AquaAddVersionOrListAssessmentsStepViewModelValidator : AbstractValidator<AquaVersionStepViewModel>
    {
        private readonly ILocalizationService _localizationService;

        public AquaAddVersionOrListAssessmentsStepViewModelValidator(
            ILogger<AquaAddVersionOrListAssessmentsStepViewModelValidator> logger, AquaLocalizationService localizationService)
        {
            _localizationService = localizationService;

            RuleFor(x => x.Name).NotNull().NotEmpty();
            RuleFor(x => x.IsoLanguage).NotNull().NotEmpty();
            RuleFor(x => x.IsoScript).NotNull().NotEmpty();
            RuleFor(x => x.Abbreviation).NotNull().NotEmpty();
            //RuleFor(x => x.Rights).NotNull().NotEmpty();
            RuleFor(x => x.ForwardTranslationToVersionId)
                .Custom((x, context) =>
                {
                    if (x != null && x != string.Empty && (!(int.TryParse(x, out int value)) || value <= 0))
                    {
                        var messageFormat = _localizationService["NumericTextValidationMessage"];
                        context.AddFailure(string.Format(messageFormat, x));
                    }
                });
            RuleFor(x => x.BackTranslationToVersionId)
                .Custom((x, context) =>
                {
                    if (x != null && x != string.Empty && (!(int.TryParse(x, out int value)) || value <= 0))
                    {
                        var messageFormat = _localizationService["NumericTextValidationMessage"];
                        context.AddFailure(string.Format(messageFormat, x));
                    }
                });
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
