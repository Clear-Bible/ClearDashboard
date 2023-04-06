using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.Extensions;

using ModelCorpusType = ClearDashboard.DataAccessLayer.Models.CorpusType;
using ModelCorpus = ClearDashboard.DataAccessLayer.Models.Corpus;
using System.Diagnostics;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class DeleteTranslationSetByTranslationSetIdCommandHandler : ProjectDbContextCommandHandler<DeleteTranslationSetByTranslationSetIdCommand,
        RequestResult<Unit>, Unit>
    {
        private readonly IMediator _mediator;

        public DeleteTranslationSetByTranslationSetIdCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<DeleteTranslationSetByTranslationSetIdCommandHandler> logger)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(
            DeleteTranslationSetByTranslationSetIdCommand request, CancellationToken cancellationToken)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            var translationSet = ProjectDbContext.TranslationSets
                .Where(e => e.Id == request.TranslationSetId.Id)
                .FirstOrDefault();

            if (translationSet is null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid TranslationSetId '{request.TranslationSetId.Id}' found in request"
                );
            }

            ProjectDbContext.Remove(translationSet);
            _ = await ProjectDbContext.SaveChangesAsync(cancellationToken);

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end)");
#endif

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}