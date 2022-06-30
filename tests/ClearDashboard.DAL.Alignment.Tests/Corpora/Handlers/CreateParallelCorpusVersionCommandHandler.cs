using System;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;
using Xunit;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.Handlers
{
    public class CreateParallelCorpusVersionCommandHandler : IRequestHandler<
        CreateParallelCorpusVersionCommand,
        RequestResult<ParallelCorpusVersionId>>
    {
        public Task<RequestResult<ParallelCorpusVersionId>>
            Handle(CreateParallelCorpusVersionCommand command, CancellationToken cancellationToken)
        {
            //DB Impl notes:
            //1. Create a new record in ParallelCorpusVersionId table with command.ParallelCorpusId as parent,
            //2. insert all the VerseMapping, referencing command.SourceCorpus and command.TargetCorpus Verses, based on command.EngineVerseMapping

            Assert.IsType<TokenizedTextCorpus>(command.EngineParallelTextCorpus.SourceCorpus); //Should be created ParallelCorpusVersionId's sourceCorpus FK
            Assert.IsType<TokenizedTextCorpus>(command.EngineParallelTextCorpus.TargetCorpus); //Should be created ParallelCorpusVersionId's targetCorpus FK
            Assert.NotNull(command.EngineParallelTextCorpus.EngineVerseMappingList);
              

            return Task.FromResult(
                new RequestResult<ParallelCorpusVersionId>
                (result: new ParallelCorpusVersionId(new Guid(), DateTime.UtcNow),
                success: true,
                message: "successful result from test"));
        }
    }
}
