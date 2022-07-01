using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class CreateParallelCorpusCommandHandler : AlignmentDbContextCommandHandler<CreateParallelCorpusCommand, RequestResult<ParallelCorpusId>, ParallelCorpusId>
    {

        public CreateParallelCorpusCommandHandler(ProjectNameDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<CreateParallelCorpusCommandHandler> logger) : base(projectNameDbContextFactory,projectProvider,  logger)
        {
        }

        protected override async Task<RequestResult<ParallelCorpusId>> SaveData(CreateParallelCorpusCommand request, CancellationToken cancellationToken)
        {
            //DB Impl notes:
            //Create a new record in ParallelCorpus and return its id.

            var parallelCorpus = new ParallelCorpus();
            await AlignmentContext.ParallelCorpa.AddAsync(parallelCorpus, cancellationToken);
            await AlignmentContext.SaveChangesAsync(cancellationToken);

            return new RequestResult<ParallelCorpusId>
                    (result: new ParallelCorpusId(parallelCorpus.Id),
                    success: true,
                    message: "successful result from test");
        }
    }
}
