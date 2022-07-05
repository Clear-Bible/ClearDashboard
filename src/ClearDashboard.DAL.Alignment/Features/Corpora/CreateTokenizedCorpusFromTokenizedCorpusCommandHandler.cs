using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class CreateTokenizedCorpusFromTokenizedCorpusCommandHandler : ProjectDbContextCommandHandler<
        CreateTokenizedCorpusFromTokenizedCorpusCommand,
        RequestResult<TokenizedTextCorpus>,
        TokenizedTextCorpus>
    {
        private readonly IMediator _mediator;

        public CreateTokenizedCorpusFromTokenizedCorpusCommandHandler(IMediator mediator, ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<CreateTokenizedCorpusFromTokenizedCorpusCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

       

        protected override Task<RequestResult<TokenizedTextCorpus>> SaveDataAsync(CreateTokenizedCorpusFromTokenizedCorpusCommand request, CancellationToken cancellationToken)
        {
            //DB Impl notes:
            //create a new TokenizedCorpus under the same Corpus parent
            //enumerate the TokensTextRows and insert associated Tokens
            //return a new TokensTextRow constructed with the new TokenizedCorpus.Id.
            // Assert.All(command.textCorpus, tc => Assert.IsType<TokensTextRow>(tc));

            return Task.FromResult(
                new RequestResult<TokenizedTextCorpus>
                (result: Task.Run(() => TokenizedTextCorpus.Get( _mediator, new TokenizedCorpusId(new Guid())), cancellationToken).GetAwaiter().GetResult(),
                    //run async from sync like constructor: good desc. https://stackoverflow.com/a/40344759/13880559
                    success: true,
                    message: "successful result from test"));
        }
    }
}
