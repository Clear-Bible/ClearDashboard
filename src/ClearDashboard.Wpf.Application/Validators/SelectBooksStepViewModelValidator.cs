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

    public class SelectBooksStepViewModelValidator : AbstractValidator<SelectBooksStepViewModel>
    {
        public SelectBooksStepViewModelValidator(ILogger<SelectBooksStepViewModelValidator> logger)
        {
            //RuleFor(x => x.SelectedProject).NotNull();
        }

    }
}
