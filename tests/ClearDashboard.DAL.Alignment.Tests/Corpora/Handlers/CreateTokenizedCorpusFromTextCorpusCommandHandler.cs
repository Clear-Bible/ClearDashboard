using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System;

using ClearBible.Alignment.DataServices.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearBible.Alignment.DataServices.Features.Corpora;
using Xunit;
using ClearBible.Engine.Corpora;
using System.Collections.Generic;
using System.Linq;

namespace ClearBible.Alignment.DataServices.Tests.Corpora.Handlers
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
            // 2. creates a new associated CorpusVersion,
            // 3. creates a new associated TokenizedCorpus,
            // 4. then iterates through command.TextCorpus, casting to TokensTextRow, extracting tokens, and inserting associated to TokenizedCorpus into the Tokens table.
            Assert.All(command.TextCorpus, tc => Assert.IsType<TokensTextRow>(tc));

            return Task.FromResult(
                new RequestResult<TokenizedTextCorpus>
                (result: Task.Run(() => TokenizedTextCorpus.Get(new MediatorMock(), new TokenizedCorpusId(new Guid()))).GetAwaiter().GetResult(), 
                //run async from sync like constructor: good desc. https://stackoverflow.com/a/40344759/13880559
                success: true,
                message: "successful result from test"));
        }
    }

}
