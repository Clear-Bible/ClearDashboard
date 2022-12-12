using ClearDashboard.Wpf.Application.ViewModels.Project;
using ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Validators
{
    

        public class ParatextCorpusStepViewModelValidator : AbstractValidator<ParatextCorpusStepViewModel>
        {
            public ParatextCorpusStepViewModelValidator(ILogger<ParatextCorpusStepViewModelValidator> logger)
            {
                //RuleFor(x => x.SelectedProject).NotNull();
            }

        }


        public class AddParatextCorpusStepViewModelValidator : AbstractValidator<AddParatextCorpusStepViewModel>
        {
            public AddParatextCorpusStepViewModelValidator(ILogger<AddParatextCorpusStepViewModelValidator> logger)
            {
                //RuleFor(x => x.SelectedProject).NotNull();
            }

        }

}
