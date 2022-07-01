using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class CreateParallelTokenizedCorpusCommandHandler : AlignmentDbContextCommandHandler<
        CreateParallelTokenizedCorpusCommand,
        RequestResult<ParallelTokenizedCorpusId>,
        ParallelTokenizedCorpusId>
    {

        public CreateParallelTokenizedCorpusCommandHandler(ProjectNameDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<CreateParallelTokenizedCorpusCommandHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override Task<RequestResult<ParallelTokenizedCorpusId>> SaveData(CreateParallelTokenizedCorpusCommand request, CancellationToken cancellationToken)
        {
            //DB Impl notes:
            //Create a new record in ParallelTokenizedCorpus table with command.sourceCorpusIdVersionId and command.targetCorpusIdVersionId and parent command.ParallelCorpusVersionId

            return Task.FromResult(
                new RequestResult<ParallelTokenizedCorpusId>
                (result: new ParallelTokenizedCorpusId(new Guid()),
                    success: true,
                    message: "successful result from test"));
        }

       

       
    }
}
