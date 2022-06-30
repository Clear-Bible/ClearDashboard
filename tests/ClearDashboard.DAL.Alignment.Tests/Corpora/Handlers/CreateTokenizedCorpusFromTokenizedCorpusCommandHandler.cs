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
    public class CreateTokenizedCorpusFromTokenizedCorpusCommandHandler : IRequestHandler<
        CreateTokenizedCorpusFromTokenizedCorpusCommand,
        RequestResult<TokenizedTextCorpus>>
    {
        public Task<RequestResult<TokenizedTextCorpus>>
            Handle(CreateTokenizedCorpusFromTokenizedCorpusCommand command, CancellationToken cancellationToken)
        {
            //DB Impl notes:
            //create a new TokenizedCorpus under the same Corpus parent
            //enumerate the TokensTextRows and insert associated Tokens
            //return a new TokensTextRow constructed with the new TokenizedCorpus.Id.
            Assert.All(command.TextCorpus, tc => Assert.IsType<TokensTextRow>(tc));

            return Task.FromResult(
                new RequestResult<TokenizedTextCorpus>
                (result: Task.Run(() => TokenizedTextCorpus.Get(command.ProjectName, new MediatorMock(), new TokenizedCorpusId(new Guid()))).GetAwaiter().GetResult(),
                //run async from sync like constructor: good desc. https://stackoverflow.com/a/40344759/13880559
                success: true,
                message: "successful result from test"));
        }
    }
}
