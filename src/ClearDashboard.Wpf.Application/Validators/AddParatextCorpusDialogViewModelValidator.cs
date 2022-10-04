using ClearDashboard.Wpf.Application.ViewModels.Project;
using ClearDashboard.Wpf.Application.ViewModels.Project.Interlinear;
using ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;
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

    public class InterlinearDialogViewModelValidator : AbstractValidator<InterlinearDialogViewModel>
    {
        public InterlinearDialogViewModelValidator()
        {
            RuleFor(x => x.TranslationSetDisplayName).NotNull();
        }
    }

    public class TranslationSetStepViewModelValidator : AbstractValidator<TranslationSetStepViewModel>
    {
        public TranslationSetStepViewModelValidator()
        {
            RuleFor(x => x.TranslationSetDisplayName).NotNull();
        }
    }

    public class AlignmentSetStepViewModelValidator : AbstractValidator<AlignmentSetStepViewModel>
    {
        public AlignmentSetStepViewModelValidator()
        {
            RuleFor(x => x.AlignmentSetDisplayName).NotNull();
        }
    }

    public class ParallelCorpusStepViewModelValidator : AbstractValidator<ParallelCorpusStepViewModel>
    {
        public ParallelCorpusStepViewModelValidator()
        {
            RuleFor(x => x.ParallelCorpusDisplayName).NotNull();
        }
    }
}
