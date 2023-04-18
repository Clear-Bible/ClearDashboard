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
    public class DeleteAlignmentSetByAlignmentSetIdCommandHandler : ProjectDbContextCommandHandler<DeleteAlignmentSetByAlignmentSetIdCommand,
        RequestResult<Unit>, Unit>
    {
        private readonly IMediator _mediator;

        public DeleteAlignmentSetByAlignmentSetIdCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<DeleteAlignmentSetByAlignmentSetIdCommandHandler> logger)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(
            DeleteAlignmentSetByAlignmentSetIdCommand request, CancellationToken cancellationToken)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            var alignmentSet = ProjectDbContext.AlignmentSets
                .Where(e => e.Id == request.AlignmentSetId.Id)
                .FirstOrDefault();

            if (alignmentSet is null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid AlignmentSetId '{request.AlignmentSetId.Id}' found in request"
                );
            }

            ProjectDbContext.Remove(alignmentSet);
            _ = await ProjectDbContext.SaveChangesAsync(cancellationToken);

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end)");
#endif

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}