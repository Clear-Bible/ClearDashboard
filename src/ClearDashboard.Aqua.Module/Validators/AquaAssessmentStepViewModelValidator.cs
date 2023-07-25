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
    internal class AquaAssessmentStepViewModelValidator : AbstractValidator<AquaAssessmentStepViewModel>
    {
        private readonly ILocalizationService _localizationService;

        public AquaAssessmentStepViewModelValidator(
            ILogger<AquaAssessmentStepViewModelValidator> logger, AquaLocalizationService localizationService)
        {
            //_localizationService = localizationService;
            //RuleFor(x => x.Revision)
            //    .Custom((x, context) =>
            //    {
            //        if (x != null && x != string.Empty && (!(int.TryParse(x, out int value)) || value <= 0))
            //        {
            //            var messageFormat = _localizationService["NumericTextValidationMessage"];
            //            context.AddFailure(string.Format(messageFormat, x));
            //        }
            //    });
        }
    }
}
