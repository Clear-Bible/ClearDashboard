using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class CreateParallelCorpusVersionCommandHandler : AlignmentDbContextCommandHandler<
        CreateParallelCorpusVersionCommand,
        RequestResult<ParallelCorpusVersionId>, ParallelCorpusVersionId>
    {

        public CreateParallelCorpusVersionCommandHandler(ProjectNameDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<CreateParallelCorpusVersionCommandHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override Task<RequestResult<ParallelCorpusVersionId>> SaveData(CreateParallelCorpusVersionCommand request, CancellationToken cancellationToken)
        {
            //DB Impl notes:
            //1. Create a new record in ParallelCorpusVersionId table with command.ParallelCorpusId as parent,
            //2. insert all the VerseMapping, referencing command.SourceCorpus and command.TargetCorpus Verses, based on command.EngineVerseMapping

            //Assert.IsType<TokenizedTextCorpus>(command.engineParallelTextCorpus.SourceCorpus); //Should be created ParallelCorpusVersionId's sourceCorpus FK
            //Assert.IsType<TokenizedTextCorpus>(command.engineParallelTextCorpus.TargetCorpus); //Should be created ParallelCorpusVersionId's targetCorpus FK
            //Assert.NotNull(command.engineParallelTextCorpus.EngineVerseMappingList);


            return Task.FromResult(
                new RequestResult<ParallelCorpusVersionId>
                (result: new ParallelCorpusVersionId(new Guid(), DateTime.UtcNow),
                    success: true,
                    message: "successful result from test"));
        }

       
        
    }
}
