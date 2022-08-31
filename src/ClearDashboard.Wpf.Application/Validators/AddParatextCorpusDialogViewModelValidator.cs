using ClearDashboard.Wpf.Application.ViewModels;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.Validators
{
    public class AddParatextCorpusDialogViewModelValidator : AbstractValidator<AddParatextCorpusDialogViewModel>
    {
        public AddParatextCorpusDialogViewModelValidator(ILogger<AddParatextCorpusDialogViewModelValidator> logger)
        {
            RuleFor(x => x.SelectedProject).NotNull();
        }

    }
}
