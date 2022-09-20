using ClearDashboard.DataAccessLayer.Models;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.Validators;

public class ParallelCorpusValidator : AbstractValidator<ParallelCorpus>
{
    private ILogger? _logger;

    public ParallelCorpusValidator(ILogger<ParallelCorpusValidator> logger)
    {

        RuleFor(x => x.DisplayName).Custom((displayName, context) =>
        {

            if (string.IsNullOrEmpty(displayName))
            {
                return;
            }
        });
    }

}