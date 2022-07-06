using System;
using System.Threading;
using System.Threading.Tasks;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;
using Xunit;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.Handlers
{
    public class CreateTokenizedCorpusFromTextCorpusCommandHandler : IRequestHandler<
        CreateTokenizedCorpusFromTextCorpusCommand,
        RequestResult<TokenizedTextCorpus>>
    {
        public Task<RequestResult<TokenizedTextCorpus>>
            Handle(CreateTokenizedCorpusFromTextCorpusCommand command, CancellationToken cancellationToken)
        {
            //DB Impl notes:
            // 1. creates a new Corpus,
            // 2. creates a new associated TokenizedCorpus,
            // 3. then iterates through command.TextCorpus, casting to TokensTextRow, extracting tokens, and inserting associated to TokenizedCorpus into the Tokens table.
            Assert.All(command.TextCorpus, tc => Assert.IsType<TokensTextRow>(tc));

            return Task.FromResult(
                new RequestResult<TokenizedTextCorpus>
                (result: Task.Run(() => TokenizedTextCorpus.Get( new MediatorMock(), new TokenizedCorpusId(new Guid()))).GetAwaiter().GetResult(), 
                //run async from sync like constructor: good desc. https://stackoverflow.com/a/40344759/13880559
                success: true,
                message: "successful result from test"));
        }
    }

}
