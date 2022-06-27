using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

using ClearBible.Alignment.DataServices.Corpora;
using ClearBible.Alignment.DataServices.Features.Corpora;
using ClearDashboard.DAL.CQRS;
using Xunit;

namespace ClearBible.Alignment.DataServices.Tests.Corpora.Handlers
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

            Assert.IsType<TokenizedTextCorpus>(command.engineParallelTextCorpus.SourceCorpus); //Should be created ParallelCorpusVersionId's sourceCorpus FK
            Assert.IsType<TokenizedTextCorpus>(command.engineParallelTextCorpus.TargetCorpus); //Should be created ParallelCorpusVersionId's targetCorpus FK
            Assert.NotNull(command.engineParallelTextCorpus.EngineVerseMappingList);
              

            return Task.FromResult(
                new RequestResult<ParallelCorpusVersionId>
                (result: new ParallelCorpusVersionId(new Guid(), DateTime.UtcNow),
                success: true,
                message: "successful result from test"));
        }
    }
}
